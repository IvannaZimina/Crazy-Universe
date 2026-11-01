using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrazyUniverse.Core.Resources;

namespace CrazyUniverse.Core.Simulation
{
    /// ==================== DESCRIPTION ====================
    ///  This service handles heavy CPU calculations for animals in separate background threads.
    ///  It provides asynchronous simulation of animal behavior based on their type and workload.
    /// =====================================================
    public sealed class AnimalSimulationService // Defines a service that runs animal simulations.
    {
        // Main asynchronous method that runs simulation for a single animal snapshot.
        public async Task<SimulationResult> SimulateAsync( // Runs one simulation and returns its result asynchronously.
            AnimalSnapshot animal,                          // Snapshot containing animal data at start
            int workLoad,                                   // Total number of iterations/steps to compute
            IProgress<SimulationProgress>? progress = null, // Optional reporter for UI progress updates
            CancellationToken token = default,              // Optional token to listen for cancellation requests
            bool simulateError = false)                     // Optional switch for testing calculation errors
        {
            ArgumentNullException.ThrowIfNull(animal);      // Check if the animal object is null; throw if it is

            var stopwatch = Stopwatch.StartNew();           // Start measuring simulation duration
            string animalName = animal.Name;                // Extract and store the animal's name locally
            string animalType = animal.AnimalType;          // Extract and store the animal's type locally

            try // Starts a block where calculation errors and cancellation can be handled.
            {
                // Run heavy CPU calculations on a background worker thread so UI doesn't freeze.
                await Task.Run(() =>                // Starts the calculation on a background thread and waits for it to finish.
                {
                    long totalSteps = workLoad;     // Stores the total number of calculation steps.
                    double calculationValue = 0;    // Stores the result of repeated mathematical calculations.

                    // Heavy loop representing intensive CPU work.
                    for (long i = 1; i <= totalSteps; i++)          // Repeats the calculation from the first step to the last.
                    {
                        // Periodically check if the user clicked "Cancel".
                        if ((i & 0x3FFF) == 0)                      // Checks the cancellation token every 16,384 steps.
                        {
                            token.ThrowIfCancellationRequested();   // Stops the calculation if cancellation was requested.
                        }

                        // Perform different calculations depending on the animal type.
                        if (animalType.Contains("Monkey", StringComparison.OrdinalIgnoreCase))  // Checks if the animal type is Monkey, ignoring letter case.
                        {
                            calculationValue += Math.Sin(i) * Math.Cos(i) * 42.0;               // Adds the Monkey-specific calculation to the total.
                        }
                        else if (animalType.Contains("Lion", StringComparison.OrdinalIgnoreCase)) // Checks if the animal type is Lion, ignoring letter case.
                        {
                            calculationValue += Math.Sqrt(i) * Math.Sin(animal.Satiety + i);    // Adds a calculation that also uses the animal's satiety.
                        }
                        else // Uses this calculation for all other animal types.
                        {
                            calculationValue += Math.Cos(i / 10.0) * Math.Tan(1.0 + (i % 5));   // Adds the calculation for other animals to the total.
                        }

                        // Send progress updates every 100,000 steps or at the final step.
                        if (i % 100_000 == 0 || i == totalSteps)                    // Checks whether it is time to report progress.
                        {
                            progress?.Report(                                       // Sends an update only if a progress reporter was provided.
                                new SimulationProgress(i, totalSteps, animalName)); // Creates an update with the current step, total steps and animal name.
                        }
                    }

                    // TEST ONLY: simulate an error for Lion when the test switch is enabled.
                    if (simulateError && // Checks whether test error simulation is enabled.
                        animalType.Contains("Lion", StringComparison.OrdinalIgnoreCase)) // Checks whether the current animal is a Lion.
                    {
                        throw new InvalidOperationException( // Stops the calculation by throwing a test exception.
                            Messages.Simulation_Error_Lion); // Gets the error message from the resource file.
                    }

                }, token);                              // Passes the cancellation token to Task.Run.

                stopwatch.Stop();                       // Stops measuring the calculation time.

                Debug.WriteLine(string.Format(          // Writes a formatted message to the Visual Studio Output window.
                    Messages.Simulation_Log_Success,    // Gets the success log format from the resource file.
                    animalName,                         // Inserts the animal's name into the message.
                    stopwatch.Elapsed.TotalSeconds));   // Inserts the time spent on the calculation.

                // Return success result.
                return new SimulationResult(            // Creates the result object for a successful simulation.
                    animalName,                         // Stores the animal's name in the result.
                    Messages.Simulation_Status_Success, // Sets the result status to Success.
                    Messages.Simulation_Message_Success,// Sets the success message from the resource file.
                    stopwatch.Elapsed.TotalSeconds);    // Stores the total calculation time in seconds.
            }
            catch (OperationCanceledException)          // Handles a cancellation request during the simulation.
            {
                stopwatch.Stop();                       // Stops measuring the time after cancellation.

                Debug.WriteLine(string.Format(          // Writes the cancellation message to the Visual Studio Output window.
                    Messages.Simulation_Log_Cancelled,  // Gets the cancellation log format from the resource file.
                    animalName,                         // Inserts the animal's name into the message.
                    stopwatch.Elapsed.TotalSeconds));   // Inserts the time spent before cancellation.

                // Return cancelled result.
                return new SimulationResult(            // Creates the result object for a cancelled simulation.
                    animalName,                         // Stores the animal's name in the result.
                    Messages.Simulation_Status_Cancelled, // Sets the result status to Cancelled.
                    Messages.Simulation_Message_Cancelled, // Sets the cancellation message from the resource file.
                    stopwatch.Elapsed.TotalSeconds);    // Stores the time spent before cancellation.
            }
            catch (Exception ex)                        // Handles other errors that may occur during the calculation.
            {
                stopwatch.Stop();                       // Stops measuring the time after an error.

                // Write full exception information to Visual Studio Output window.
                Debug.WriteLine(string.Format(          // Writes the error details to the Visual Studio Output window.
                    Messages.Simulation_Log_Error,      // Gets the error log format from the resource file.
                    animalName,                         // Inserts the animal's name into the message.
                    ex));                               // Inserts the exception details.

                // Return error result.
                return new SimulationResult(            // Creates the result object for a failed simulation.
                    animalName,                         // Stores the animal's name in the result.
                    Messages.Simulation_Status_Error,   // Sets the result status to Error.
                    ex.Message,                         // Stores the error message.
                    stopwatch.Elapsed.TotalSeconds);    // Stores the time spent before the error.
            }
        }
    }
}