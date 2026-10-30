
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyUniverse.Core.Simulation
{
    /// ==================== DESCRIPTION ====================
    ///  This service handles heavy CPU calculations for animals in separate background threads.
    ///  It provides asynchronous simulation of animal behavior based on their type and workload.
    /// =====================================================
    public sealed class AnimalSimulationService
    {
        // Main asynchronous method that runs simulation for a single animal snapshot.
        public async Task<SimulationResult> SimulateAsync(
            AnimalSnapshot animal,                          // Snapshot containing animal data at start
            int workLoad,                                   // Total number of iterations/steps to compute
            IProgress<SimulationProgress>? progress = null, // Optional reporter for UI progress updates
            CancellationToken token = default,              // Optional token to listen for cancellation requests
            bool simulateError = false)                     // Optional switch for testing calculation errors
        {
            ArgumentNullException.ThrowIfNull(animal);      // Check if the animal object is null; throw if it is

            var stopwatch = Stopwatch.StartNew();           // Start measuring simulation duration
            string animalName = animal.Name;                 // Extract and store the animal's name locally
            string animalType = animal.AnimalType;           // Extract and store the animal's type locally

            try
            {
                // Run heavy CPU calculations on a background worker thread so UI doesn't freeze.
                await Task.Run(() =>
                {
                    long totalSteps = workLoad;
                    double calculationValue = 0;

                    // Heavy loop representing intensive CPU work.
                    for (long i = 1; i <= totalSteps; i++)
                    {
                        // Periodically check if the user clicked "Cancel".
                        if ((i & 0x3FFF) == 0)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        // Perform different calculations depending on the animal type.
                        if (animalType.Contains("Monkey", StringComparison.OrdinalIgnoreCase))
                        {
                            calculationValue += Math.Sin(i) * Math.Cos(i) * 42.0;
                        }
                        else if (animalType.Contains("Lion", StringComparison.OrdinalIgnoreCase))
                        {
                            calculationValue += Math.Sqrt(i) * Math.Sin(animal.Satiety + i);
                        }
                        else
                        {
                            calculationValue += Math.Cos(i / 10.0) * Math.Tan(1.0 + (i % 5));
                        }

                        // Send progress updates every 100,000 steps or at the final step.
                        if (i % 100_000 == 0 || i == totalSteps)
                        {
                            progress?.Report(
                                new SimulationProgress(i, totalSteps, animalName));
                        }
                    }

                    // TEST ONLY: simulate an error for Lion when the test switch is enabled.
                    if (simulateError &&
                        animalType.Contains("Lion", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Test error: Lion population simulation failed.");
                    }

                }, token);

                stopwatch.Stop();

                Debug.WriteLine(
                    $"[SUCCESS] {animalName}: calculation completed in {stopwatch.Elapsed.TotalSeconds:F3} sec.");

                // Return success result.
                return new SimulationResult(
                    animalName,
                    "Success",
                    "Calculation completed successfully.",
                    stopwatch.Elapsed.TotalSeconds);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();

                Debug.WriteLine(
                    $"[CANCELLED] {animalName}: simulation was cancelled after {stopwatch.Elapsed.TotalSeconds:F3} sec.");

                // Return cancelled result.
                return new SimulationResult(
                    animalName,
                    "Cancelled",
                    "Simulation was cancelled by user.",
                    stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Write full exception information to Visual Studio Output window.
                Debug.WriteLine($"[ERROR] {animalName}: {ex}");

                // Return error result.
                return new SimulationResult(
                    animalName,
                    "Error",
                    ex.Message,
                    stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}
