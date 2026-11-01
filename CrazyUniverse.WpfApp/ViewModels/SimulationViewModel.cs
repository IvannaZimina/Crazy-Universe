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

namespace CrazyUniverse.WpfApp.ViewModels
{
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

        public ObservableCollection<SimulationCardViewModel> AnimalCards { get; } = new();  // Holds the animal cards displayed in the window.

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

        public ICommand RunSequentialCommand { get; }                   // Command that starts simulations one after another.
        public ICommand RunParallelCommand { get; }                     // Command that starts simulations at the same time.
        public ICommand CancelCommand { get; }                          // Command that requests cancellation of the current run.

        public SimulationViewModel()                                    // Creates the ViewModel and prepares its commands and animal cards.
        {
            _simulationService = new AnimalSimulationService();         // Creates the service that performs the calculations.

            // Starts sequential simulation when the command is executed.
            RunSequentialCommand = new RelayCommand(
                async _ => await RunSequentialAsync(),                  // Calls the sequential method and waits for it to finish.
                _ => !IsBusy);                                          // Allows the command only when no simulation is running.

            // Starts parallel simulation when the command is executed.
            RunParallelCommand = new RelayCommand(
                async _ => await RunParallelAsync(),                    // Calls the parallel method and waits for it to finish.
                _ => !IsBusy);                                          // Allows the command only when no simulation is running.

            // Requests cancellation of the current simulation.
            CancelCommand = new RelayCommand(
                _ => CancelExecution(),                                 // Calls the method that sends the cancellation request.
                _ => IsBusy);                                           // Allows cancellation only while a simulation is running.

            // Creates the default animal cards and listens for selection changes.
            foreach (var card in AnimalCardFactory.CreateDefaultCards()) // Gets the default animal cards one by one.
            {
                card.PropertyChanged += AnimalCard_PropertyChanged;      // Subscribes to changes in each animal card.
                AnimalCards.Add(card);                                   // Adds the card to the collection shown in the UI.
            }
        }

        // Handles changes in an animal card.
        private void AnimalCard_PropertyChanged(
            object? sender,                                                     // Identifies the object that sent the change notification.
            PropertyChangedEventArgs e)                                         // Contains the name of the property that changed.
        {
            if (e.PropertyName == nameof(SimulationCardViewModel.IsSelected))   // Checks whether the selection state has changed.
            {
                // A new selection requires new snapshots.
                _cachedSnapshots = null;                                        // Clears old snapshots so they can be created again.

                OnPropertyChanged(nameof(SelectedAnimalsText));                 // Updates the selected animal count in the UI.
            }
        }

        // Creates snapshots for selected animals or returns the cached ones.
        private List<(SimulationCardViewModel Card, AnimalSnapshot Snapshot)>
            GetSnapshotsForSelectedAnimals()
        {
            if (_cachedSnapshots == null)                                       // Checks whether snapshots have already been created.
            {
                _cachedSnapshots = AnimalCards                                  // Starts with all available animal cards.
                    .Where(card => card.IsSelected)                             // Keeps only cards selected by the user.
                    .Select(card => (card, card.CreateSnapshot()))              // Pairs each selected card with a snapshot of its data.
                    .ToList();                                                  // Saves the pairs in a list.
            }

            return _cachedSnapshots;                                            // Returns the existing snapshots or the newly created ones.
        }

        // Runs animal calculations one by one.
        private async Task RunSequentialAsync()
        {
            if (IsBusy) return;                                         // Prevents another run from starting while a simulation is active.

            var selectedItems = GetSnapshotsForSelectedAnimals();       // Gets the selected cards and their snapshots.

            if (selectedItems.Count == 0)                               // Checks whether the user selected any animals.
            {
                LogEvent(Messages.Simulation_Log_NoAnimalsSelected);    // Adds a message explaining that selection is required.
                return;                                                 // Stops because there is nothing to simulate.
            }

            PrepareForRun();                                            // Resets the UI and creates a new cancellation source.

            // Keeps the test error setting unchanged during this run.
            bool simulateError = SimulateError;                         // Copies the current test error setting for this run.

            // Measures the total time of the sequential run.
            var stopwatch = Stopwatch.StartNew();                       // Starts measuring the total run duration.

            try // Ensures that the run is finalized even if an unexpected error occurs.
            {
                LogEvent(Messages.Simulation_Log_StartingSequential);   // Records that sequential mode has started.

                for (int i = 0; i < selectedItems.Count; i++)           // Goes through the selected animals in order.
                {
                    var item = selectedItems[i];                        // Gets the current card and its snapshot.
                    var card = item.Card;                               // Stores the current animal card.
                    var snapshot = item.Snapshot;                       // Stores the animal data used for this calculation.

                    // Marks all remaining animals as cancelled if needed.
                    if (_currentCts!.IsCancellationRequested)           // Checks whether the user requested cancellation.
                    {
                        for (int j = i; j < selectedItems.Count; j++)   // Goes through the current and all remaining animals.
                        {
                            MarkAsCancelled(selectedItems[j].Card);     // Adds a cancelled result for an animal that was not started.
                        }

                        break;                                          // Stops starting new animal calculations.
                    }

                    card.Status = Messages.Simulation_Status_Running;   // Shows that the current animal is being processed.

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_StartingAnimal,     // Gets the message format from the resource file.
                            snapshot.Name));                            // Supplies the current animal name.

                    // Sends progress updates to the animal card.
                    var progress = new Progress<SimulationProgress>(p => // Creates a handler for calculation progress updates.
                    {
                        card.ProgressPercent = p.Total <= 0             // Checks whether the total number of steps is valid.
                            ? 0                                         // Uses zero progress if there are no steps.
                            : (int)((double)p.Done / p.Total * 100);    // Calculates the completed percentage.

                        card.ProgressText = string.Format(              // Updates the text showing the current calculation step.
                            Messages.Simulation_Progress_Steps,         // Gets the progress message format from the resource file.
                            p.Done,                                     // Supplies the number of completed steps.
                            p.Total);                                   // Supplies the total number of steps.
                    });

                    // Waits for the current animal before starting the next one.
                    var result = await _simulationService.SimulateAsync( // Starts the current animal calculation and waits for its result.
                        snapshot,                                       // Passes the animal data snapshot to the service.
                        Workload,                                       // Passes the selected number of calculation steps.
                        progress,                                       // Passes the progress reporter.
                        _currentCts.Token,                              // Passes the token used to cancel the calculation.
                        simulateError);                                 // Passes the test error setting.

                    // Updates the card and the results table.
                    ProcessTaskResult(card, result);                    // Saves the result and updates the UI for this animal.
                }
            }
            finally // Runs this block whether the simulation finishes normally or an exception occurs.
            {
                // Stops the timer and saves the total run time.
                stopwatch.Stop();                                           // Stops measuring the total run duration.
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");  // Converts the elapsed time into hours, minutes and seconds.

                // Adds the final run time to the event log.
                LogEvent(
                    string.Format(
                        Messages.Simulation_Log_Finished,                   // Gets the final log message format from the resource file.
                        stopwatch.Elapsed.TotalSeconds));                   // Supplies the total duration in seconds.

                // Always resets the run state and releases the cancellation source.
                FinalizeRun();                                              // Marks the simulation as finished and releases its cancellation source.
            }
        }

        // Runs all selected animal calculations at the same time.
        private async Task RunParallelAsync()
        {
            if (IsBusy) return;                                             // Prevents another run from starting while a simulation is active.

            var selectedItems = GetSnapshotsForSelectedAnimals();           // Gets the selected cards and their snapshots.

            if (selectedItems.Count == 0)                                   // Checks whether the user selected any animals.
            {
                LogEvent(Messages.Simulation_Log_NoAnimalsSelected);        // Adds a message explaining that selection is required.
                return;                                                     // Stops because there is nothing to simulate.
            }

            PrepareForRun();                                                // Resets the UI and creates a new cancellation source.

            // Keeps the test error setting unchanged during this run.
            bool simulateError = SimulateError;                             // Copies the current test error setting for this run.

            // Measures the total time of the parallel run.
            var stopwatch = Stopwatch.StartNew();                           // Starts measuring the total run duration.

            try                                                             // Ensures that the run is finalized even if an unexpected error occurs.
            {
                LogEvent(Messages.Simulation_Log_StartingParallel);         // Records that parallel mode has started.

                // Creates one task for each selected animal.
                var tasks = selectedItems.Select(item =>                    // Creates a task for every selected animal.
                {
                    var card = item.Card;                                   // Gets the card that will display this animal's progress.
                    var snapshot = item.Snapshot;                           // Gets the fixed animal data for the calculation.

                    card.Status = Messages.Simulation_Status_Running;       // Shows that this animal's calculation has been scheduled.

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ScheduledTask,          // Gets the scheduled task message from the resource file.
                            snapshot.Name));                                // Supplies the current animal name.

                    // Sends progress updates to the animal card.
                    var progress = new Progress<SimulationProgress>(p =>    // Creates a handler for calculation progress updates.
                    {
                        card.ProgressPercent = p.Total <= 0                 // Checks whether the total number of steps is valid.
                            ? 0                                             // Uses zero progress if there are no steps.
                            : (int)((double)p.Done / p.Total * 100);        // Calculates the completed percentage.

                        card.ProgressText = string.Format(                  // Updates the text showing the current calculation step.
                            Messages.Simulation_Progress_Steps,             // Gets the progress message format from the resource file.
                            p.Done,                                         // Supplies the number of completed steps.
                            p.Total);                                       // Supplies the total number of steps.
                    });

                    return RunSingleAnimalTaskAsync(    // Starts the calculation task for this animal.
                        card,                           // Passes the card that will display the result.
                        snapshot,                       // Passes the animal data snapshot.
                        progress,                       // Passes the progress reporter.
                        _currentCts!.Token,             // Passes the shared cancellation token.
                        simulateError);                 // Passes the test error setting.

                }).ToArray();                           // Converts all created tasks into an array.

                // Waits until all animal tasks have finished.
                await Task.WhenAll(tasks);              // Waits for every task in the array to complete.
            }
            finally                                     // Runs this block whether all tasks finish normally or an exception occurs.
            {
                // Stops the timer and saves the total run time.
                stopwatch.Stop();                       // Stops measuring the total run duration.
                TotalTimeText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss"); // Converts the elapsed time into hours, minutes and seconds.

                // Adds the final run time to the event log.
                LogEvent(                                   // Adds the total duration to the event log.
                    string.Format(                          // Inserts the measured duration into the log message.
                        Messages.Simulation_Log_Finished,   // Gets the final log message format from the resource file.
                        stopwatch.Elapsed.TotalSeconds));   // Supplies the total duration in seconds.

                // Runs even if an unexpected exception occurs.
                FinalizeRun();                              // Marks the simulation as finished and releases its cancellation source.
            }
        }

        // Runs one animal calculation as a separate task.
        private async Task RunSingleAnimalTaskAsync(
            SimulationCardViewModel card,                   // Card that displays this animal's progress and result.
            AnimalSnapshot snapshot,                        // Fixed copy of the animal data used for this calculation.
            IProgress<SimulationProgress> progress,         // Reports progress while the calculation is running.
            CancellationToken token,                        // Allows this calculation to be cancelled.
            bool simulateError)                             // Controls whether the test error should be triggered.
        {
            var result = await _simulationService.SimulateAsync( // Starts the calculation and waits for its result.
                snapshot,                                        // Passes the animal data snapshot.
                Workload,                                        // Passes the selected number of calculation steps.
                progress,                                        // Passes the progress reporter.
                token,                                           // Passes the cancellation token.
                simulateError);                                  // Passes the test error setting.

            // Saves the result for this animal.
            ProcessTaskResult(card, result);                     // Updates the card, counters, results table and event log.
        }

        // Updates the animal card, counters and results table.
        private void ProcessTaskResult(
            SimulationCardViewModel card,                       // Card connected to the completed calculation.
            SimulationResult result)                            // Contains the status, details and duration of the calculation.
        {
            card.Status = result.Status;                        // Displays the status returned by the simulation service.

            card.ProgressPercent = result.Status == "Success"   // Checks whether the calculation finished successfully.
                ? 100                                           // Sets progress to 100 percent after success.
                : card.ProgressPercent;                         // Keeps the current progress for errors or cancellation.

            ResultsTable.Add(new SimulationResultRowViewModel(  // Creates and adds a new row to the results table.
                result,                                         // Passes the calculation result.
                DateTime.Now,                                   // Records the time when the result was processed.
                card.ImagePath));                               // Uses the animal image in the result row.

            // Updates the correct counter and adds a message to the log.
            switch (result.Status)                              // Checks the final status to decide which counter and log message to update.
            {
                case "Success":                                 // Handles a calculation that finished successfully.
                    SuccessfulCount++;                          // Increases the successful simulation counter.

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultSuccess,
                            result.AnimalName,                      // Supplies the animal name.
                            result.ElapsedSeconds));                // Supplies the calculation duration.

                    break;                                          // Ends the Success case.

                case "Cancelled":                                   // Handles a calculation that was cancelled.
                    CancelledCount++;                               // Increases the cancelled simulation counter.

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultCancelled,
                            result.AnimalName));                     // Supplies the animal name.

                    break;                                           // Ends the Cancelled case.

                default:                                             // Handles any result status that is not Success or Cancelled.
                    ErrorCount++;                                    // Increases the error counter.

                    LogEvent(
                        string.Format(
                            Messages.Simulation_Log_ResultError,
                            result.AnimalName,                       // Supplies the animal name.
                            result.Details));                        // Supplies the error details.

                    break;                                           // Ends the default case.
            }
        }

        // Adds a report row for an animal that was not started.
        private void MarkAsCancelled(SimulationCardViewModel card)  // Marks an animal as cancelled before its calculation starts.
        {
            card.Status = Messages.Simulation_Status_Cancelled;     // Updates the status shown on the animal card.
            card.ProgressText = Messages.Simulation_Progress_CancelledBeforeStart; // Shows that the calculation did not start.

            var result = new SimulationResultRowViewModel(          // Creates a result row for this animal.
                card.Name,                                          // Supplies the animal name.
                Messages.Simulation_Status_Cancelled,               // Sets the result status to Cancelled.
                Messages.Simulation_Message_CancelledBeforeStart,   // Supplies the message explaining why it was cancelled.
                0,                                                  // Sets the calculation duration to zero because it never started.
                DateTime.Now,                                       // Records the time when the cancellation was added.
                card.ImagePath);                                    // Uses the animal image in the result row.

            ResultsTable.Add(result);                               // Adds the cancelled result to the results table.

            CancelledCount++;                                       // Increases the cancelled simulation counter.

            LogEvent(
                string.Format(
                    Messages.Simulation_Log_CancelledBeforeStart,
                    card.Name));                                    // Supplies the animal name.
        }

        // Sends a cancellation request to the running calculations.
        private void CancelExecution()
        {
            if (_currentCts == null || _currentCts.IsCancellationRequested) // Checks whether there is a cancellation source and whether it was already cancelled.
                return;                                                     // Stops if there is nothing to cancel or cancellation was already requested.

            LogEvent(Messages.Simulation_Log_CancellationRequested);        // Records that the user requested cancellation.

            _currentCts.Cancel();                                           // Sends the cancellation request to all calculations using this token.
        }

        // Prepares the interface and cancellation source for a new run.
        private void PrepareForRun()
        {
            IsBusy = true;                                          // Marks the application as busy while calculations are running.

            TotalTimeText = "00:00:00";                             // Resets the displayed total time.

            SuccessfulCount = 0;                                    // Resets the successful simulation counter.
            ErrorCount = 0;                                         // Resets the error counter.
            CancelledCount = 0;                                     // Resets the cancelled simulation counter.

            ResultsTable.Clear();                                   // Removes results from the previous run.
            EventLog.Clear();                                       // Removes log messages from the previous run.

            _currentCts?.Dispose();                                 // Releases the previous cancellation source if one exists.
            _currentCts = new CancellationTokenSource();            // Creates a new source for cancellation requests.

            // Resets the progress and status of every animal card.
            foreach (var card in AnimalCards)                       // Goes through all animal cards in the window.
            {
                card.ProgressPercent = 0;                           // Resets the progress bar to zero.
                card.ProgressText = Messages.Simulation_Status_Idle; // Shows that no calculation is currently running.

                card.Status = card.IsSelected                       // Checks whether this animal is selected.
                    ? Messages.Simulation_Status_Waiting            // Sets the status to Waiting for selected animals.
                    : Messages.Simulation_Status_NotSelected;       // Sets the status to Not selected for other animals.
            }
        }

        // Finishes the run and releases resources.
        private void FinalizeRun()
        {
            IsBusy = false;                                         // Marks the application as ready for another simulation.

            // Releases the cancellation source after the run.
            _currentCts?.Dispose();                                 // Releases the cancellation source if it exists.
            _currentCts = null;                                     // Removes the reference to the finished cancellation source.
        }

        // Adds a message with the current time to the event log.
        private void LogEvent(string message)                       // Receives the message that should be added to the log.
        {
            EventLog.Insert(                                        // Adds the new message to the collection.
                0,                                                  // Inserts it at the first position so the newest message appears at the top.
                $"[{DateTime.Now:HH:mm:ss}] {message}");            // Adds the current time before the message.
        }

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)         // Uses the caller's property name when no name is passed.
        {
            PropertyChanged?.Invoke(                                // Sends a change notification if there are listeners.
                this,                                               // Identifies this ViewModel as the object that changed.
                new PropertyChangedEventArgs(propertyName));        // Provides the name of the changed property.
        }
    }

    // Holds the data and state displayed on one animal card.
    public sealed class SimulationCardViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = true;                                // Selects the animal by default.
        private int _progressPercent;                                   // Stores the current progress percentage.
        private string _progressText = Messages.Simulation_Status_Idle; // Stores the text shown near the progress bar.
        private string _status = Messages.Simulation_Status_Waiting;    // Stores the current animal status.

        public string Name { get; }                                     // Gets the animal's display name.
        public string Id { get; }                                       // Gets the animal's unique card identifier.
        public string AnimalType { get; }                               // Gets the animal type used by the simulation service.
        public double Satiety { get; }                                  // Gets the animal's satiety value.
        public string ImagePath { get; }                                // Gets the path to the animal image.

        public string SimulationTitle { get; }                          // Gets the title of the simulation shown on the card.
        public string SimulationDescription { get; }                    // Gets the description shown on the card.

        // Gets or sets whether this animal is selected for simulation.
        public bool IsSelected
        {
            get => _isSelected;                                         // Returns whether the animal is selected.
            set                                                         // Updates the selection state.
            {
                if (_isSelected == value) return;                       // Stops if the selection state has not changed.

                _isSelected = value;                                    // Saves the new selection state.
                OnPropertyChanged();                                    // Notifies the UI and the parent ViewModel about the change.
            }
        }

        // Shows the calculation progress as a percentage.
        public int ProgressPercent
        {
            get => _progressPercent;                                    // Returns the current progress percentage.
            set                                                         // Updates the progress percentage.
            {
                int newValue = Math.Clamp(value, 0, 100);               // Keeps the progress value between zero and one hundred.

                if (_progressPercent == newValue) return;               // Stops if the progress value has not changed.

                _progressPercent = newValue;                            // Saves the new progress percentage.
                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Shows the current calculation step.
        public string ProgressText
        {
            get => _progressText;                                       // Returns the text shown near the progress bar.
            set                                                         // Updates the progress text.
            {
                _progressText = string.IsNullOrWhiteSpace(value)        // Checks whether the new text is empty or contains only spaces.
                    ? Messages.Simulation_Status_Idle                   // Uses the default Idle text when no value was provided.
                    : value;                                            // Otherwise, saves the provided text.

                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        // Shows the current status of this animal.
        public string Status
        {
            get => _status;                                             // Returns the current animal status.
            set                                                         // Updates the animal status.
            {
                _status = string.IsNullOrWhiteSpace(value)              // Checks whether the new status is empty or contains only spaces.
                    ? Messages.Simulation_Status_Waiting                // Uses Waiting as the default status.
                    : value;                                            // Otherwise, saves the provided status.

                OnPropertyChanged();                                    // Notifies the UI about the changed property.
            }
        }

        public SimulationCardViewModel(
            string name,                                // Animal name displayed on the card.
            string id,                                  // Identifier used to distinguish this card.
            string animalType,                          // Animal type used to choose the calculation.
            double satiety,                             // Animal satiety value used by the simulation.
            string imagePath,                           // Path to the image displayed on the card.
            string simulationTitle,                     // Title describing the animal's simulation.
            string simulationDescription)               // Short explanation of the simulation.
        {
            Name = name;                                // Saves the animal name.
            Id = id;                                    // Saves the card identifier.
            AnimalType = animalType;                    // Saves the animal type.
            Satiety = satiety;                          // Saves the satiety value.
            ImagePath = imagePath;                      // Saves the image path.
            SimulationTitle = simulationTitle;          // Saves the simulation title.
            SimulationDescription = simulationDescription; // Saves the simulation description.
        }

        // Creates a fixed copy of the animal data for a simulation run.
        public AnimalSnapshot CreateSnapshot()
        {
            return new AnimalSnapshot(                  // Creates a new snapshot with the current animal data.
                Guid.NewGuid(),                         // Gives this snapshot a new unique identifier.
                Name,                                   // Copies the animal name.
                AnimalType,                             // Copies the animal type.
                Satiety);                               // Copies the satiety value.
        }

        public event PropertyChangedEventHandler? PropertyChanged;  // Notifies the UI when a card property changes.

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)         // Uses the caller's property name when no name is passed.
        {
            PropertyChanged?.Invoke(                                // Sends a notification if there are listeners.
                this,                                               // Identifies this card as the object that changed.
                new PropertyChangedEventArgs(propertyName));        // Provides the name of the changed property.
        }
    }

    // Holds one row of information about a completed or cancelled simulation.
    public sealed class SimulationResultRowViewModel
    {
        public string TimeString { get; }       // Gets the time when the result was recorded.
        public string AnimalName { get; }       // Gets the name of the animal in this result.
        public string ImagePath { get; }        // Gets the path to the animal image.
        public string Status { get; }           // Gets the final status of the simulation.
        public string Details { get; }          // Gets the result message or error details.
        public double ElapsedSeconds { get; }   // Gets the time spent on the calculation in seconds.

        // Creates a result row from a completed simulation.
        public SimulationResultRowViewModel(
            SimulationResult result,            // Contains the result returned by the simulation service.
            DateTime completedAt,               // Time when the simulation result was recorded.
            string imagePath)                   // Path to the animal image shown in the table.
        {
            TimeString = completedAt.ToString("HH:mm:ss");  // Converts the completion time into hours, minutes and seconds.
            AnimalName = result.AnimalName;                 // Copies the animal name from the result.
            ImagePath = imagePath;                          // Saves the image path.
            Status = result.Status;                         // Copies the final simulation status.
            Details = result.Details;                       // Copies the result message or error details.
            ElapsedSeconds = result.ElapsedSeconds;         // Copies the calculation duration.
        }

        // Creates a result row for an animal cancelled before it started.
        public SimulationResultRowViewModel(
            string animalName,                              // Name of the animal that was not started.
            string status,                                  // Final status assigned to this animal.
            string details,                                 // Message explaining why the simulation did not start.
            double elapsedSeconds,                          // Time spent on the calculation, which is zero if it never started.
            DateTime completedAt,                           // Time when the cancellation result was recorded.
            string imagePath)                               // Path to the animal image shown in the table.
        {
            TimeString = completedAt.ToString("HH:mm:ss");  // Converts the completion time into hours, minutes and seconds.
            AnimalName = animalName;                        // Saves the animal name.
            ImagePath = imagePath;                          // Saves the image path.
            Status = status;                                // Saves the final status.
            Details = details;                              // Saves the explanation message.
            ElapsedSeconds = elapsedSeconds;                // Saves the calculation duration.
        }
    }
}