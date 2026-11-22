using CrazyUniverse.Core.Models;
using CrazyUniverse.Core.Resources;
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
using System.Windows.Data;

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
        private readonly AnimalSimulationService _simulationService;    // Provides the calculation logic for animal simulations.
        private CancellationTokenSource? _currentCts;                   // Allows the current simulation to receive a cancellation request.
        private bool _isBusy;                                           // Shows whether a simulation is currently running.
        private bool _simulateError;                                    // Controls whether the test error should be triggered.
        private int _workload = 10_000_000;                             // Sets the default number of calculation steps.
        private string _totalTimeText = "00:00:00";                     // Stores the total simulation time as text.
        private int _successfulCount;                                   // Counts simulations that finished successfully.
        private int _errorCount;                                        // Counts simulations that finished with an error.
        private int _cancelledCount;                                    // Counts simulations that were cancelled.

        // Keeps the same snapshots for sequential and parallel runs.
        private List<(SimulationCardViewModel Card, AnimalSnapshot Snapshot)>? _cachedSnapshots;

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
        public string SelectedAnimalsText =>                    // Provides the selected animal count as a text value.
            string.Format(                                      // Inserts the selected count into the resource message.
                Messages.Simulation_SelectedAnimals,            // Gets the message format from the resource file.
                AnimalCards.Count(card => card.IsSelected));    // Counts only cards whose IsSelected value is true.

        // Enables or disables the test error for the Lion simulation.
        public bool SimulateError
        {
            get => _simulateError;                              // Returns the current test error setting.
            set                                                 // Updates the test error setting.
            {
                if (_simulateError == value) return;            // Stops if the new value is the same as the current one.

                _simulateError = value;                         // Saves the new test error setting.
                OnPropertyChanged();                            // Notifies the UI about the changed property.
            }
        }

        // Sets the number of calculation steps.
        public int Workload
        {
            get => _workload;                                           // Returns the current number of calculation steps.
            set                                                         // Updates the number of calculation steps.
            {
                int newValue = Math.Clamp(value, 100_000, 100_000_000); // Keeps the workload between the allowed minimum and maximum.

                if (_workload == newValue) return;                      // Stops if the workload has not changed.

                _workload = newValue;                                   // Saves the new workload value.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Shows if a simulation is currently running.
        public bool IsBusy
        {
            get => _isBusy;                                             // Returns whether a simulation is running.
            set                                                         // Updates the running state.
            {
                if (_isBusy == value) return;                           // Stops if the running state has not changed.

                _isBusy = value;                                        // Saves the new running state.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
                CommandManager.InvalidateRequerySuggested();            // Makes WPF check again which commands can be executed.
            }
        }

        // Displays the total simulation time.
        public string TotalTimeText
        {
            get => _totalTimeText;                                      // Returns the displayed total simulation time.
            set                                                         // Updates the displayed total simulation time.
            {
                _totalTimeText = string.IsNullOrWhiteSpace(value)       // Checks whether the new value is empty or contains only spaces.
                    ? "00:00:00"                                        // Uses the default time when no value was provided.
                    : value;                                            // Otherwise, saves the provided time.

                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Stores the number of successful simulations.
        public int SuccessfulCount
        {
            get => _successfulCount;                                    // Returns the number of successful simulations.
            set                                                         // Updates the successful simulation count.
            {
                _successfulCount = Math.Max(0, value);                  // Prevents the counter from becoming negative.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Stores the number of simulations with errors.
        public int ErrorCount
        {
            get => _errorCount;                                         // Returns the number of simulations with errors.
            set                                                         // Updates the error count.
            {
                _errorCount = Math.Max(0, value);                       // Prevents the counter from becoming negative.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Stores the number of cancelled simulations.
        public int CancelledCount
        {
            get => _cancelledCount;                                     // Returns the number of cancelled simulations.
            set                                                         // Updates the cancelled simulation count.
            {
                _cancelledCount = Math.Max(0, value);                   // Prevents the counter from becoming negative.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        public ICommand RunSequentialCommand { get; private set; } = null!;   // Command that starts simulations one after another.
        public ICommand RunParallelCommand { get; private set; } = null!;     // Command that starts simulations at the same time.
        public ICommand CancelCommand { get; private set; } = null!;          // Command that requests cancellation of the current run.

        // Constructor accepting real zoo animals to build dynamic cards from actual app instances.
        public SimulationViewModel(IEnumerable<Animal> animals)         // Initializes ViewModel with real active animals from repositories.
        {
            _simulationService = new AnimalSimulationService();         // Creates the service that performs the calculations.
            InitializeCommands();                                       // Sets up user commands.

            PopulateCards(AnimalCardFactory.CreateCardsFromAnimals(animals)); // Loads real cards into master collection and groups.
        }

        // Helper to populate master collection, set up property listeners, group by enclosures, and refresh central view
        private void PopulateCards(IEnumerable<SimulationCardViewModel> cards)
        {
            AnimalCards.Clear();
            EnclosureGroups.Clear();

            var groups = cards.GroupBy(c => c.AnimalType);

            foreach (var g in groups)
            {
                var groupObj = new EnclosureSimulationGroup { EnclosureName = g.Key + "s" };
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

        // Updates the central panel to contain ONLY cards where IsSelected is true
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
            RunSequentialCommand = new RelayCommand(
                async _ => await RunSequentialAsync(),
                _ => !IsBusy);

            RunParallelCommand = new RelayCommand(
                async _ => await RunParallelAsync(),
                _ => !IsBusy);

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
                RefreshSelectedCards();                                         // Обновляем центральную сетку при изменении галочки
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
                LogEvent(Messages.Simulation_Log_NoAnimalsSelected);
                return;
            }

            PrepareForRun();
            bool simulateError = SimulateError;
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
                            MarkAsCancelled(selectedItems[j].Card);
                        }

                        break;
                    }

                    card.Status = Messages.Simulation_Status_Running;

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_StartingAnimal,
                            snapshot.Name));

                    // Sends live progress updates to the individual animal card progress bar.
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
                        Workload,
                        progress,
                        _currentCts.Token,
                        simulateError);

                    ProcessTaskResult(card, result);
                }
            }
            finally
            {
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

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
            bool simulateError = SimulateError;
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

                    // Sends live progress updates to the individual animal card progress bar in parallel mode.
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
                        simulateError);

                }).ToArray();

                await Task.WhenAll(tasks);
            }
            finally
            {
                stopwatch.Stop();
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

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
            bool simulateError)
        {
            var result = await _simulationService.SimulateAsync(
                snapshot,
                Workload,
                progress,
                token,
                simulateError);

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
        private void MarkAsCancelled(SimulationCardViewModel card)
        {
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
            TotalTimeText = "00:00:00";

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