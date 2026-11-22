using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;
using CrazyUniverse.Core.Simulation;
using CrazyUniverse.WpfApp.Commands;
using CrazyUniverse.WpfApp.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrazyUniverse.WpfApp.ViewModels
{
    // Helper class to group cards by enclosure/type in the left selection panel
    public class EnclosureSimulationGroup
    {
        public string EnclosureName { get; set; } = string.Empty;
        public ObservableCollection<SimulationCardViewModel> Cards { get; } = new();
    }

    // Stores the data and commands used by the simulation window.
    public sealed class SimulationViewModel : INotifyPropertyChanged
    {
        private const double RelativeTolerance = 1e-9;

        private readonly AnimalSimulationService _simulationService;    // Provides the calculation logic for animal simulations.
        private CancellationTokenSource? _currentCts;                   // Allows the current simulation to receive a cancellation request.
        private bool _isBusy;                                           // Shows whether a simulation is currently running.
        private bool _simulateError;                                    // Controls whether the test error should be triggered.
        private int _workload = 10_000_000;                             // Sets the default number of calculation steps.
        private string _totalTimeText = "00:00.000";                    // Stores the total simulation time as text.
        private int _successfulCount;                                   // Counts simulations that finished successfully.
        private int _errorCount;                                        // Counts simulations that finished with an error.
        private int _cancelledCount;                                   // Counts simulations that were cancelled.

        private bool _hasComparison;
        private string _sequentialTimeComparisonText = "-";
        private string _parallelTimeComparisonText = "-";
        private string _speedupComparisonText = "-";
        private string _valuesMatchComparisonText = "-";

        // Keeps the same snapshots for sequential and parallel runs.
        private List<(SimulationCardViewModel Card, AnimalSnapshot Snapshot)>? _cachedSnapshots;

        // Stores the latest results from each simulation mode.
        private SimulationRunData? _sequentialRun;
        private SimulationRunData? _parallelRun;

        public event PropertyChangedEventHandler? PropertyChanged;                          // Notifies the UI when a property value changes.

        // Master collection of all available animal cards
        public ObservableCollection<SimulationCardViewModel> AnimalCards { get; } = new();

        // Grouped view for the left selection panel (maintains enclosures)
        public ObservableCollection<EnclosureSimulationGroup> EnclosureGroups { get; } = new();

        // Filtered collection for the central grid: contains ONLY animals with IsSelected == true
        public ObservableCollection<SimulationCardViewModel> SelectedAnimalCards { get; } = new();

        public ObservableCollection<SimulationResultRowViewModel> ResultsTable { get; } = new(); // Holds the results displayed in the results table.

        public ObservableCollection<string> EventLog { get; } = new();                      // Stores messages about simulation events.

        // Shows how many animals are currently selected.
        public string SelectedAnimalsText =>
            string.Format(
                Messages.Simulation_SelectedAnimals,
                AnimalCards.Count(card => card.IsSelected));

        // Enables or disables the test error for the Lion simulation.
        public bool SimulateError
        {
            get => _simulateError;
            set
            {
                if (_simulateError == value) return;

                _simulateError = value;
                InvalidateStoredRuns();
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
                InvalidateStoredRuns();
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
                    ? "00:00.000"
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

        public bool HasComparison
        {
            get => _hasComparison;
            private set
            {
                if (_hasComparison == value) return;
                _hasComparison = value;
                OnPropertyChanged();
            }
        }

        public string SequentialTimeComparisonText
        {
            get => _sequentialTimeComparisonText;
            private set
            {
                if (_sequentialTimeComparisonText == value) return;
                _sequentialTimeComparisonText = value;
                OnPropertyChanged();
            }
        }

        public string ParallelTimeComparisonText
        {
            get => _parallelTimeComparisonText;
            private set
            {
                if (_parallelTimeComparisonText == value) return;
                _parallelTimeComparisonText = value;
                OnPropertyChanged();
            }
        }

        public string SpeedupComparisonText
        {
            get => _speedupComparisonText;
            private set
            {
                if (_speedupComparisonText == value) return;
                _speedupComparisonText = value;
                OnPropertyChanged();
            }
        }

        public string ValuesMatchComparisonText
        {
            get => _valuesMatchComparisonText;
            private set
            {
                if (_valuesMatchComparisonText == value) return;
                _valuesMatchComparisonText = value;
                OnPropertyChanged();
            }
        }

        public ICommand RunSequentialCommand { get; private set; } = null!;
        public ICommand RunParallelCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        // Constructor accepting real zoo animals to build dynamic cards from actual app instances.
        public SimulationViewModel(IEnumerable<Animal> animals)
        {
            _simulationService = new AnimalSimulationService();
            InitializeCommands();

            PopulateCards(AnimalCardFactory.CreateCardsFromAnimals(animals));
        }

        // Helper to populate master collection, set up property listeners, group by enclosures, and refresh central view
        private void PopulateCards(IEnumerable<SimulationCardViewModel> cards)
        {
            AnimalCards.Clear();
            EnclosureGroups.Clear();

            var groups = cards.GroupBy(c => c.AnimalType);

            foreach (var g in groups)
            {
                var groupObj = new EnclosureSimulationGroup
                {
                    EnclosureName = GetEnclosureDisplayName(g.Key)
                };

                foreach (var card in g)
                {
                    card.PropertyChanged += AnimalCard_PropertyChanged;
                    AnimalCards.Add(card);
                    groupObj.Cards.Add(card);
                }

                EnclosureGroups.Add(groupObj);
            }

            RefreshSelectedCards();
        }

        private static string GetEnclosureDisplayName(string animalType)
        {
            return animalType switch
            {
                "Monkey" => Messages.UI_MonkeyEnclosure,
                "Lion" => Messages.UI_LionEnclosure,
                "Penguin" => Messages.UI_PenguinEnclosure,
                "HoneyBadger" => Messages.UI_HoneyBadgerEnclosure,
                "Kangaroo" => Messages.UI_KangarooEnclosure,
                "Dolphin" => Messages.UI_DolphinEnclosure,
                "Frog" => Messages.UI_FrogEnclosure,
                _ => animalType
            };
        }

        // Updates the central panel to contain ONLY cards where IsSelected == true
        private void RefreshSelectedCards()
        {
            SelectedAnimalCards.Clear();
            foreach (var card in AnimalCards.Where(c => c.IsSelected))
            {
                SelectedAnimalCards.Add(card);
            }
        }

        // Initializes all UI commands.
        private void InitializeCommands()
        {
            RunSequentialCommand = new AsyncRelayCommand(
                _ => RunSequentialAsync(),
                _ => !IsBusy,
                ex => LogEvent($"Unexpected error in sequential run: {ex.Message}"));

            RunParallelCommand = new AsyncRelayCommand(
                _ => RunParallelAsync(),
                _ => !IsBusy,
                ex => LogEvent($"Unexpected error in parallel run: {ex.Message}"));

            CancelCommand = new RelayCommand(
                _ => CancelExecution(),
                _ => IsBusy);
        }

        // Handles changes in an animal card (such as toggling the checkbox)
        private void AnimalCard_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SimulationCardViewModel.IsSelected))
            {
                _cachedSnapshots = null;
                InvalidateStoredRuns();
                RefreshSelectedCards();
                OnPropertyChanged(nameof(SelectedAnimalsText));
            }
        }

        private void InvalidateStoredRuns()
        {
            _sequentialRun = null;
            _parallelRun = null;
            ResetComparison();
        }

        private void ResetComparison()
        {
            HasComparison = false;
            SequentialTimeComparisonText = "-";
            ParallelTimeComparisonText = "-";
            SpeedupComparisonText = "-";
            ValuesMatchComparisonText = "-";
        }

        private void UpdateComparisonIfPossible()
        {
            if (_sequentialRun == null || _parallelRun == null)
            {
                ResetComparison();
                return;
            }

            if (_sequentialRun.Workload != _parallelRun.Workload ||
                _sequentialRun.SimulateError != _parallelRun.SimulateError)
            {
                ResetComparison();
                return;
            }

            var seqIds = _sequentialRun.Snapshots.Select(s => s.Id).ToList();
            var parIds = _parallelRun.Snapshots.Select(s => s.Id).ToList();

            if (!seqIds.SequenceEqual(parIds))
            {
                ResetComparison();
                return;
            }

            int matchedCount = 0;
            int comparableCount = 0;

            foreach (string id in seqIds)
            {
                if (!_sequentialRun.Results.TryGetValue(id, out var seqRes) ||
                    !_parallelRun.Results.TryGetValue(id, out var parRes))
                {
                    continue;
                }

                if (!seqRes.CalculationValue.HasValue || !parRes.CalculationValue.HasValue)
                {
                    continue;
                }

                comparableCount++;
                double a = seqRes.CalculationValue.Value;
                double b = parRes.CalculationValue.Value;
                double tolerance = RelativeTolerance * Math.Max(1.0, Math.Abs(a));

                if (Math.Abs(a - b) <= tolerance)
                {
                    matchedCount++;
                }
            }

            double seqMs = _sequentialRun.TotalTime.TotalMilliseconds;
            double parMs = _parallelRun.TotalTime.TotalMilliseconds;

            SequentialTimeComparisonText = _sequentialRun.TotalTime.ToString(@"mm\:ss\.fff");
            ParallelTimeComparisonText = _parallelRun.TotalTime.ToString(@"mm\:ss\.fff");

            if (parMs > 0)
            {
                double speedup = seqMs / parMs;
                SpeedupComparisonText = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:F2}x",
                    speedup);
            }
            else
            {
                SpeedupComparisonText = "N/A";
            }

            ValuesMatchComparisonText = string.Format(
                CultureInfo.InvariantCulture,
                "{0} of {1}",
                matchedCount,
                comparableCount);

            HasComparison = true;
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
                LogEvent(Messages.Simulation_Log_NoAnimalsSelected);
                return;
            }

            PrepareForRun();

            int runWorkload = Workload;
            bool simulateError = SimulateError;
            var runSnapshots = selectedItems.Select(item => item.Snapshot).ToList();
            var runResults = new ConcurrentDictionary<string, SimulationAnimalResult>();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                LogEvent(Messages.Simulation_Log_StartingSequential);

                for (int i = 0; i < selectedItems.Count; i++)
                {
                    var item = selectedItems[i];
                    var card = item.Card;
                    var snapshot = item.Snapshot;

                    if (_currentCts!.IsCancellationRequested)
                    {
                        for (int j = i; j < selectedItems.Count; j++)
                        {
                            MarkAsCancelled(selectedItems[j].Card, runResults);
                        }

                        break;
                    }

                    card.Status = Messages.Simulation_Status_Running;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_StartingAnimal,
                            snapshot.Name));

                    var progress = new Progress<SimulationProgress>(p =>
                    {
                        card.ProgressPercent = p.Total <= 0
                            ? 0
                            : (int)((double)p.Done / p.Total * 100);

                        card.ProgressText = string.Format(
                            Messages.Simulation_Progress_Steps,
                            p.Done,
                            p.Total);
                    });

                    var result = await _simulationService.SimulateAsync(
                        snapshot,
                        runWorkload,
                        progress,
                        _currentCts.Token,
                        simulateError);

                    ProcessTaskResult(card, result, runResults);
                }
            }
            finally
            {
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"mm\:ss\.fff");

                _sequentialRun = new SimulationRunData(
                    runResults,
                    stopwatch.Elapsed,
                    runWorkload,
                    runSnapshots,
                    simulateError);

                UpdateComparisonIfPossible();

                LogEvent(
                    string.Format(
                        Messages.Simulation_Log_Finished,
                        stopwatch.Elapsed.TotalSeconds));

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
                LogEvent(Messages.Simulation_Log_NoAnimalsSelected);
                return;
            }

            PrepareForRun();

            int runWorkload = Workload;
            bool simulateError = SimulateError;
            var runSnapshots = selectedItems.Select(item => item.Snapshot).ToList();
            var runResults = new ConcurrentDictionary<string, SimulationAnimalResult>();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                LogEvent(Messages.Simulation_Log_StartingParallel);

                var tasks = selectedItems.Select(item =>
                {
                    var card = item.Card;
                    var snapshot = item.Snapshot;

                    card.Status = Messages.Simulation_Status_Running;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ScheduledTask,
                            snapshot.Name));

                    var progress = new Progress<SimulationProgress>(p =>
                    {
                        card.ProgressPercent = p.Total <= 0
                            ? 0
                            : (int)((double)p.Done / p.Total * 100);

                        card.ProgressText = string.Format(
                            Messages.Simulation_Progress_Steps,
                            p.Done,
                            p.Total);
                    });

                    return RunSingleAnimalTaskAsync(
                        card,
                        snapshot,
                        progress,
                        _currentCts!.Token,
                        simulateError,
                        runWorkload,
                        runResults);

                }).ToArray();

                await Task.WhenAll(tasks);
            }
            finally
            {
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"mm\:ss\.fff");

                _parallelRun = new SimulationRunData(
                    runResults,
                    stopwatch.Elapsed,
                    runWorkload,
                    runSnapshots,
                    simulateError);

                UpdateComparisonIfPossible();

                LogEvent(
                    string.Format(
                        Messages.Simulation_Log_Finished,
                        stopwatch.Elapsed.TotalSeconds));

                FinalizeRun();
            }
        }

        // Runs one animal calculation as a separate task.
        private async Task RunSingleAnimalTaskAsync(
            SimulationCardViewModel card,
            AnimalSnapshot snapshot,
            IProgress<SimulationProgress> progress,
            CancellationToken token,
            bool simulateError,
            int runWorkload,
            ConcurrentDictionary<string, SimulationAnimalResult> runResults)
        {
            var result = await _simulationService.SimulateAsync(
                snapshot,
                runWorkload,
                progress,
                token,
                simulateError);

            ProcessTaskResult(card, result, runResults);
        }

        // Updates the animal card, counters and results table.
        private void ProcessTaskResult(
            SimulationCardViewModel card,
            SimulationResult result,
            ConcurrentDictionary<string, SimulationAnimalResult> runResults)
        {
            runResults[card.Id] = new SimulationAnimalResult(
                result.Status,
                result.Status == "Success" ? result.CalculationValue : null,
                result.Details);

            card.Status = result.Status;

            card.ProgressPercent = result.Status == "Success"
                ? 100
                : card.ProgressPercent;

            ResultsTable.Add(new SimulationResultRowViewModel(
                result,
                DateTime.Now,
                card.ImagePath));

            switch (result.Status)
            {
                case "Success":
                    SuccessfulCount++;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultSuccess,
                            result.AnimalName,
                            result.ElapsedSeconds));

                    break;

                case "Cancelled":
                    CancelledCount++;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultCancelled,
                            result.AnimalName));

                    break;

                default:
                    ErrorCount++;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultError,
                            result.AnimalName,
                            result.Details));

                    break;
            }
        }

        // Adds a report row for an animal that was not started.
        private void MarkAsCancelled(
            SimulationCardViewModel card,
            ConcurrentDictionary<string, SimulationAnimalResult> runResults)
        {
            runResults[card.Id] = new SimulationAnimalResult(
                "Cancelled",
                null,
                Messages.Simulation_Message_CancelledBeforeStart);

            card.Status = Messages.Simulation_Status_Cancelled;
            card.ProgressText = Messages.Simulation_Progress_CancelledBeforeStart;

            var result = new SimulationResultRowViewModel(
                card.Name,
                Messages.Simulation_Status_Cancelled,
                Messages.Simulation_Message_CancelledBeforeStart,
                0,
                DateTime.Now,
                card.ImagePath);

            ResultsTable.Add(result);
            CancelledCount++;

            LogEvent(
                string.Format(
                    Messages.Simulation_Log_CancelledBeforeStart,
                    card.Name));
        }

        // Sends a cancellation request to the running calculations.
        private void CancelExecution()
        {
            if (_currentCts == null || _currentCts.IsCancellationRequested)
                return;

            LogEvent(Messages.Simulation_Log_CancellationRequested);
            _currentCts.Cancel();
        }

        // Prepares the interface and cancellation source for a new run.
        private void PrepareForRun()
        {
            IsBusy = true;
            TotalTimeText = "00:00.000";

            SuccessfulCount = 0;
            ErrorCount = 0;
            CancelledCount = 0;

            ResultsTable.Clear();
            EventLog.Clear();

            _currentCts?.Dispose();
            _currentCts = new CancellationTokenSource();

            foreach (var card in AnimalCards)
            {
                card.ProgressPercent = 0;
                card.ProgressText = Messages.Simulation_Status_Idle;

                card.Status = card.IsSelected
                    ? Messages.Simulation_Status_Waiting
                    : Messages.Simulation_Status_NotSelected;
            }
        }

        // Finishes the run and releases resources.
        private void FinalizeRun()
        {
            IsBusy = false;
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

        // Stores the outcome of one animal calculation.
        private sealed class SimulationAnimalResult
        {
            public string Status { get; }
            public double? CalculationValue { get; }
            public string Details { get; }

            public SimulationAnimalResult(
                string status,
                double? calculationValue,
                string details)
            {
                Status = status;
                CalculationValue = calculationValue;
                Details = details;
            }
        }

        // Stores the results and input parameters of one simulation run.
        private sealed class SimulationRunData
        {
            public IReadOnlyDictionary<string, SimulationAnimalResult> Results { get; }
            public TimeSpan TotalTime { get; }
            public int Workload { get; }
            public IReadOnlyList<AnimalSnapshot> Snapshots { get; }
            public bool SimulateError { get; }

            public SimulationRunData(
                ConcurrentDictionary<string, SimulationAnimalResult> results,
                TimeSpan totalTime,
                int workload,
                IReadOnlyList<AnimalSnapshot> snapshots,
                bool simulateError)
            {
                Results = new Dictionary<string, SimulationAnimalResult>(results);
                TotalTime = totalTime;
                Workload = workload;
                Snapshots = snapshots.ToList();
                SimulateError = simulateError;
            }
        }
    }

    // Holds the data and state displayed on one animal card.
    public sealed class SimulationCardViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private int _progressPercent;
        private string _progressText = Messages.Simulation_Status_Idle;
        private string _status = Messages.Simulation_Status_Waiting;

        public string Name { get; }
        public string Id { get; }
        public string AnimalType { get; }
        public double Satiety { get; }
        public string ImagePath { get; }

        public string SimulationTitle { get; }
        public string SimulationDescription { get; }

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

        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = string.IsNullOrWhiteSpace(value)
                    ? Messages.Simulation_Status_Idle
                    : value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = string.IsNullOrWhiteSpace(value)
                    ? Messages.Simulation_Status_Waiting
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

        public AnimalSnapshot CreateSnapshot()
        {
            return new AnimalSnapshot(
                Id,
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

    // Holds one row of information about a completed or cancelled simulation.
    public sealed class SimulationResultRowViewModel
    {
        public string TimeString { get; }
        public string AnimalName { get; }
        public string ImagePath { get; }
        public string Status { get; }
        public string Details { get; }
        public double ElapsedSeconds { get; }
        public double CalculationValue { get; }

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
            CalculationValue = result.CalculationValue;
        }

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
            CalculationValue = 0;
        }
    }
}