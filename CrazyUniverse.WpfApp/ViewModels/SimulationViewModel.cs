
using CrazyUniverse.Core.Simulation;
using CrazyUniverse.WpfApp.Commands;
using CrazyUniverse.WpfApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrazyUniverse.WpfApp.ViewModels
{
    public sealed class SimulationViewModel : INotifyPropertyChanged
    {
        private readonly AnimalSimulationService _simulationService;
        private CancellationTokenSource? _currentCts;
        private bool _isBusy;
        private bool _simulateError;
        private int _workload = 10_000_000;
        private string _totalTimeText = "00:00:00";
        private int _successfulCount;
        private int _errorCount;
        private int _cancelledCount;

        // Keeps the same snapshots for sequential and parallel runs.
        private List<(SimulationCardViewModel Card, AnimalSnapshot Snapshot)>? _cachedSnapshots;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SimulationCardViewModel> AnimalCards { get; } = new();

        public ObservableCollection<SimulationResultRowViewModel> ResultsTable { get; } = new();

        public ObservableCollection<string> EventLog { get; } = new();

        // Shows how many animals are currently selected.
        public string SelectedAnimalsText =>
            $"{AnimalCards.Count(card => card.IsSelected)} animals selected";

        // Enables or disables the test error for the Lion simulation.
        public bool SimulateError
        {
            get => _simulateError;
            set
            {
                if (_simulateError == value) return;

                _simulateError = value;
                OnPropertyChanged();
            }
        }

        // Sets the number of calculation steps.
        public int Workload
        {
            get => _workload;
            set
            {
                int newValue = Math.Clamp(value, 100_000, 100_000_000);

                if (_workload == newValue) return;

                _workload = newValue;
                OnPropertyChanged();
            }
        }

        // Shows if a simulation is currently running.
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Displays the total simulation time.
        public string TotalTimeText
        {
            get => _totalTimeText;
            set
            {
                _totalTimeText = string.IsNullOrWhiteSpace(value)
                    ? "00:00:00"
                    : value;

                OnPropertyChanged();
            }
        }

        // Stores the number of successful simulations.
        public int SuccessfulCount
        {
            get => _successfulCount;
            set
            {
                _successfulCount = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        // Stores the number of simulations with errors.
        public int ErrorCount
        {
            get => _errorCount;
            set
            {
                _errorCount = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        // Stores the number of cancelled simulations.
        public int CancelledCount
        {
            get => _cancelledCount;
            set
            {
                _cancelledCount = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        public ICommand RunSequentialCommand { get; }
        public ICommand RunParallelCommand { get; }
        public ICommand CancelCommand { get; }

        public SimulationViewModel()
        {
            _simulationService = new AnimalSimulationService();

            // Starts sequential simulation when the command is executed.
            RunSequentialCommand = new RelayCommand(
                async _ => await RunSequentialAsync(),
                _ => !IsBusy);

            // Starts parallel simulation when the command is executed.
            RunParallelCommand = new RelayCommand(
                async _ => await RunParallelAsync(),
                _ => !IsBusy);

            // Requests cancellation of the current simulation.
            CancelCommand = new RelayCommand(
                _ => CancelExecution(),
                _ => IsBusy);

            // Creates the default animal cards and listens for selection changes.
            foreach (var card in AnimalCardFactory.CreateDefaultCards())
            {
                card.PropertyChanged += AnimalCard_PropertyChanged;
                AnimalCards.Add(card);
            }
        }

        private void AnimalCard_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SimulationCardViewModel.IsSelected))
            {
                // A new selection requires new snapshots.
                _cachedSnapshots = null;

                OnPropertyChanged(nameof(SelectedAnimalsText));
            }
        }

        // Creates snapshots for selected animals or returns the cached ones.
        private List<(SimulationCardViewModel Card, AnimalSnapshot Snapshot)>
            GetSnapshotsForSelectedAnimals()
        {
            if (_cachedSnapshots == null)
            {
                _cachedSnapshots = AnimalCards
                    .Where(card => card.IsSelected)
                    .Select(card => (card, card.CreateSnapshot()))
                    .ToList();
            }

            return _cachedSnapshots;
        }

        // Runs animal calculations one by one.
        private async Task RunSequentialAsync()
        {
            if (IsBusy) return;

            var selectedItems = GetSnapshotsForSelectedAnimals();

            if (selectedItems.Count == 0)
            {
                LogEvent("Please select at least one animal.");
                return;
            }

            PrepareForRun();

            // Keeps the test error setting unchanged during this run.
            bool simulateError = SimulateError;

            // Measures the total time of the sequential run.
            var stopwatch = Stopwatch.StartNew();

            try
            {
                LogEvent("Starting sequential simulation mode...");

                for (int i = 0; i < selectedItems.Count; i++)
                {
                    var item = selectedItems[i];
                    var card = item.Card;
                    var snapshot = item.Snapshot;

                    // Marks all remaining animals as cancelled if needed.
                    if (_currentCts!.IsCancellationRequested)
                    {
                        for (int j = i; j < selectedItems.Count; j++)
                        {
                            MarkAsCancelled(selectedItems[j].Card);
                        }

                        break;
                    }

                    card.Status = "Running";

                    LogEvent($"Starting simulation for {snapshot.Name}...");

                    // Sends progress updates to the animal card.
                    var progress = new Progress<SimulationProgress>(p =>
                    {
                        card.ProgressPercent = p.Total <= 0
                            ? 0
                            : (int)((double)p.Done / p.Total * 100);

                        card.ProgressText = $"Step {p.Done:N0} / {p.Total:N0}";
                    });

                    // Waits for the current animal before starting the next one.
                    var result = await _simulationService.SimulateAsync(
                        snapshot,
                        Workload,
                        progress,
                        _currentCts.Token,
                        simulateError);

                    // Updates the card and the results table.
                    ProcessTaskResult(card, result);
                }
            }
            finally
            {
                // Stops the timer and saves the total run time.
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

                // Adds the final run time to the event log.
                LogEvent(
                    $"Simulation finished in {stopwatch.Elapsed.TotalSeconds:0.00} seconds.");

                // Always resets the run state and releases the cancellation source.
                FinalizeRun();
            }
        }

        // Runs all selected animal calculations at the same time.
        private async Task RunParallelAsync()
        {
            if (IsBusy) return;

            var selectedItems = GetSnapshotsForSelectedAnimals();

            if (selectedItems.Count == 0)
            {
                LogEvent("Please select at least one animal.");
                return;
            }

            PrepareForRun();

            // Keeps the test error setting unchanged during this run.
            bool simulateError = SimulateError;

            // Measures the total time of the parallel run.
            var stopwatch = Stopwatch.StartNew();

            try
            {
                LogEvent("Starting parallel simulation mode (Task.Run + Task.WhenAll)...");

                // Creates one task for each selected animal.
                var tasks = selectedItems.Select(item =>
                {
                    var card = item.Card;
                    var snapshot = item.Snapshot;

                    card.Status = "Running";

                    LogEvent($"Scheduled task for {snapshot.Name}.");

                    // Sends progress updates to the animal card.
                    var progress = new Progress<SimulationProgress>(p =>
                    {
                        card.ProgressPercent = p.Total <= 0
                            ? 0
                            : (int)((double)p.Done / p.Total * 100);

                        card.ProgressText = $"Step {p.Done:N0} / {p.Total:N0}";
                    });

                    return RunSingleAnimalTaskAsync(
                        card,
                        snapshot,
                        progress,
                        _currentCts!.Token,
                        simulateError);

                }).ToArray();

                // Waits until all animal tasks have finished.
                await Task.WhenAll(tasks);
            }
            finally
            {
                // Stops the timer and saves the total run time.
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

                // Adds the final run time to the event log.
                LogEvent(
                    $"Simulation finished in {stopwatch.Elapsed.TotalSeconds:0.00} seconds.");

                // Runs even if an unexpected exception occurs.
                FinalizeRun();
            }
        }

        // Runs one animal calculation as a separate task.
        private async Task RunSingleAnimalTaskAsync(
            SimulationCardViewModel card,
            AnimalSnapshot snapshot,
            IProgress<SimulationProgress> progress,
            CancellationToken token,
            bool simulateError)
        {
            var result = await _simulationService.SimulateAsync(
                snapshot,
                Workload,
                progress,
                token,
                simulateError);

            // Saves the result for this animal.
            ProcessTaskResult(card, result);
        }

        // Updates the animal card, counters and results table.
        private void ProcessTaskResult(
            SimulationCardViewModel card,
            SimulationResult result)
        {
            card.Status = result.Status;

            card.ProgressPercent = result.Status == "Success"
                ? 100
                : card.ProgressPercent;

            ResultsTable.Add(new SimulationResultRowViewModel(
                result,
                DateTime.Now,
                card.ImagePath));

            // Updates the correct counter and adds a message to the log.
            switch (result.Status)
            {
                case "Success":
                    SuccessfulCount++;

                    LogEvent(
                        $"{result.AnimalName}: Success. Duration: {result.ElapsedSeconds:0.00} s.");

                    break;

                case "Cancelled":
                    CancelledCount++;

                    LogEvent($"{result.AnimalName}: Cancelled by user.");

                    break;

                default:
                    ErrorCount++;

                    LogEvent($"{result.AnimalName}: Error - {result.Details}");

                    break;
            }
        }

        // Adds a report row for an animal that was not started.
        private void MarkAsCancelled(SimulationCardViewModel card)
        {
            card.Status = "Cancelled";
            card.ProgressText = "Cancelled before start";

            var result = new SimulationResultRowViewModel(
                card.Name,
                "Cancelled",
                "Simulation cancelled before it started.",
                0,
                DateTime.Now,
                card.ImagePath);

            ResultsTable.Add(result);

            CancelledCount++;

            LogEvent($"{card.Name}: Cancelled before simulation started.");
        }

        // Sends a cancellation request to the running calculations.
        private void CancelExecution()
        {
            if (_currentCts == null || _currentCts.IsCancellationRequested)
                return;

            LogEvent("Cancellation requested by user.");

            _currentCts.Cancel();
        }

        // Prepares the interface and cancellation source for a new run.
        private void PrepareForRun()
        {
            IsBusy = true;

            TotalTimeText = "00:00:00";

            SuccessfulCount = 0;
            ErrorCount = 0;
            CancelledCount = 0;

            ResultsTable.Clear();
            EventLog.Clear();

            _currentCts?.Dispose();
            _currentCts = new CancellationTokenSource();

            // Resets the progress and status of every animal card.
            foreach (var card in AnimalCards)
            {
                card.ProgressPercent = 0;
                card.ProgressText = "Idle";

                card.Status = card.IsSelected
                    ? "Waiting"
                    : "Not selected";
            }
        }

        // Finishes the run and releases resources.
        private void FinalizeRun()
        {
            IsBusy = false;

            // Releases the cancellation source after the run.
            _currentCts?.Dispose();
            _currentCts = null;
        }

        // Adds a message with the current time to the event log.
        private void LogEvent(string message)
        {
            EventLog.Insert(
                0,
                $"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class SimulationCardViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private int _progressPercent;
        private string _progressText = "Idle";
        private string _status = "Waiting";

        public string Name { get; }
        public string Id { get; }
        public string AnimalType { get; }
        public double Satiety { get; }
        public string ImagePath { get; }

        public string SimulationTitle { get; }
        public string SimulationDescription { get; }

        // Gets or sets whether this animal is selected for simulation.
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        // Shows the calculation progress as a percentage.
        public int ProgressPercent
        {
            get => _progressPercent;
            set
            {
                int newValue = Math.Clamp(value, 0, 100);

                if (_progressPercent == newValue) return;

                _progressPercent = newValue;
                OnPropertyChanged();
            }
        }

        // Shows the current calculation step.
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = string.IsNullOrWhiteSpace(value)
                    ? "Idle"
                    : value;

                OnPropertyChanged();
            }
        }

        // Shows the current status of this animal.
        public string Status
        {
            get => _status;
            set
            {
                _status = string.IsNullOrWhiteSpace(value)
                    ? "Waiting"
                    : value;

                OnPropertyChanged();
            }
        }

        public SimulationCardViewModel(
            string name,
            string id,
            string animalType,
            double satiety,
            string imagePath,
            string simulationTitle,
            string simulationDescription)
        {
            Name = name;
            Id = id;
            AnimalType = animalType;
            Satiety = satiety;
            ImagePath = imagePath;
            SimulationTitle = simulationTitle;
            SimulationDescription = simulationDescription;
        }

        // Creates a fixed copy of the animal data for a simulation run.
        public AnimalSnapshot CreateSnapshot()
        {
            return new AnimalSnapshot(
                Guid.NewGuid(),
                Name,
                AnimalType,
                Satiety);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class SimulationResultRowViewModel
    {
        public string TimeString { get; }
        public string AnimalName { get; }
        public string ImagePath { get; }
        public string Status { get; }
        public string Details { get; }
        public double ElapsedSeconds { get; }

        // Creates a result row from a completed simulation.
        public SimulationResultRowViewModel(
            SimulationResult result,
            DateTime completedAt,
            string imagePath)
        {
            TimeString = completedAt.ToString("HH:mm:ss");
            AnimalName = result.AnimalName;
            ImagePath = imagePath;
            Status = result.Status;
            Details = result.Details;
            ElapsedSeconds = result.ElapsedSeconds;
        }

        // Creates a result row for an animal cancelled before it started.
        public SimulationResultRowViewModel(
            string animalName,
            string status,
            string details,
            double elapsedSeconds,
            DateTime completedAt,
            string imagePath)
        {
            TimeString = completedAt.ToString("HH:mm:ss");
            AnimalName = animalName;
            ImagePath = imagePath;
            Status = status;
            Details = details;
            ElapsedSeconds = elapsedSeconds;
        }
    }
}
