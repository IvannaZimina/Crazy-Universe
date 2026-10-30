# Crazy Universe (Vol.1, Vol.2 & Vol.3)

Welcome to the **Crazy Universe** repository!  
This is a humorous WPF application where different types of objects live in the same world but behave, interact, and react to each other dynamically.

---

## Application Description

**Crazy Zoo** is a humorous simulator where every animal has a unique behavior, a satiety state, and specific traits.  
Users can interact with the animals (such as feeding them, making them perform actions, or adding new characters via a modal dialog) and observe their unique crazy actions and neighborhood reactions in real-time.

The solution consists of separate Core and WPF projects and uses inheritance, generics, interfaces, polymorphism, events, collections, asynchronous programming, and parallel task execution, fully adhering to the course assignment requirements.

---

## Technologies Used

- **Programming Language**: C#
- **Platform**: .NET / WPF
- **Asynchronous Programming**: async/await, Task, Task.Run, Task.WhenAll
- **Cancellation**: CancellationToken and CancellationTokenSource
- **Performance Measurement**: Stopwatch
- **Version Control**: Git
- **Development Environment**: Visual Studio 2022+

---

## Running Guide

1. Clone the repository to your local machine:  
   `https://github.com/IvannaZimina/Crazy-Universe`
2. Open the solution file (`.slnx`) in **Visual Studio** (version 2022 or newer is recommended).
3. Ensure that the `CrazyUniverse.WpfApp` project is set as the **Startup Project**.
4. Press **F5** (or click the **Start** button) to run the application.

---

## Core Assignment Requirements & Implementation Status

### 1. Type-Based Generic Repositories (`IRepository<T>` & `Repository<T>`)

- **Generic Architecture**: Built using a general `IRepository<T>` interface and concrete `Repository<T>` implementation supporting methods: `Add(T item)`, `Remove(T item)`, `IEnumerable<T> GetAll()`, and `T? Find(Func<T, bool> predicate)`.
- **Type Segregation**: Uses dedicated repositories for each concrete animal type (e.g., `Repository<Monkey>`, `Repository<Lion>`, `Repository<Penguin>`, `Repository<HoneyBadger>`).
- **Compile-Time Safety**: Implements strict type constraints (`where T : Animal`), preventing improper types at compile time without relying on UI name-matching.
- **Encapsulation Protection**: `GetAll()` returns an `IReadOnlyCollection<T>` (via `_items.AsReadOnly()`), protecting the internal list from unauthorized external clearing or modification.

### 2. Domain Events & Neighbor Reactions (`Core/Model`)

Events and reactions are fully managed within the domain layer (`Core/Model`), entirely decoupled from WPF button click handlers:

- **Addition (Welcome)**: When a new character is added to a repository, existing objects in the same enclosure greet the newcomer. The new object does not greet itself.
- **Removal (Farewell)**: When a character is removed, remaining objects bid farewell. The removed object unsubscribes from events and ceases to respond to subsequent triggers.
- **CrazyAction**: The selected character performs its unique special action. Other objects in the same enclosure react according to world logic. The initiator does not react to itself.
- **Isolation**: Objects in different repositories (enclosures) do not answer or react. Every response creates an event log entry, and specific reactions affect the responding object's state (e.g., energy/satiety changes).
- **Typed Event Arguments (`LogEventArgs`)**: Domain events transmit structured `LogEventArgs` containing an event kind (`Kind`) and the triggering animal (`Cause`), ensuring clean logging and separation of concerns.

### 3. LINQ: Search, Sorting & Grouping

The application implements three primary LINQ queries integrated into the UI:

- **Search / Filtering (`Where`)**: Filters characters by name across enclosures in real-time using case-insensitive comparison (`StringComparison.OrdinalIgnoreCase`).
- **Sorting (`OrderBy` / `OrderByDescending`)**: Toggles enclosure sorting direction by animal name (`A-Z` / `Z-A`) via interface command bindings.
- **Grouping (`GroupBy`)**: Aggregates statistics (`SatietyGroupsSummary`) grouping all zoo animals by satiety ranges (`Low` ≤ 30, `Medium` ≤ 70, `High` > 70) utilizing centralized resource keys for stable category display.

### 4. Asynchronous and Parallel Animal Simulation (Vol.3)

Vol.3 extends the application with CPU-intensive calculations for selected animals. Users can run the simulation in two different modes and compare their execution time.

- **Animal Snapshots**: Uses `AnimalSnapshot` objects to store a fixed copy of the selected animals' data before starting calculations.
- **CPU-Intensive Calculations**: Each animal performs a calculation based on its type and properties, simulating different workloads.
- **Sequential Mode**: Runs calculations one by one using `async`/`await`. The next animal starts only after the previous calculation has finished.
- **Parallel Mode**: Starts separate tasks for selected animals using `Task.Run` and waits for all tasks with `Task.WhenAll`.
- **Same Input Data**: Both modes use the same selected animal snapshots and workload, allowing execution times to be compared.
- **Progress Reporting**: Uses `IProgress<SimulationProgress>` to report calculation progress and update the animal cards in the UI.
- **Performance Measurement**: Uses `Stopwatch` to measure the total execution time of each simulation run.
- **Cancellation Support**: Uses a shared `CancellationTokenSource` and `CancellationToken` to request cancellation. Calculations periodically check the token and stop when cancellation is requested.
- **Individual Result Handling**: Each animal receives its own result with a status (`Success`, `Error`, or `Cancelled`), details, and execution time.
- **Error Isolation**: An error in one animal's calculation is handled separately and does not prevent other animal tasks from completing.
- **Test Error Option**: Includes an optional test error switch for the Lion simulation to demonstrate error handling.
- **Results and Statistics**: Displays individual results, total execution time, and counters for successful, failed, and cancelled calculations.
- **Run Protection**: Uses `try/finally` to ensure that the application resets the busy state and releases the cancellation source after a simulation run, including when an unexpected exception occurs.

---

## Architectural & Design Principles

- **Architectural Pattern**: MVVM (Model-View-ViewModel) with strict separation of concerns (Core & UI split).
- **Design Patterns**: Factory Pattern (`AnimalFactory`) for decoupled object creation.
- **Object-Oriented Programming**: Encapsulation, abstraction, polymorphism, generics, and inheritance.
- **Asynchronous Programming**: Uses tasks and asynchronous methods to keep the WPF interface responsive during long-running calculations.
- **Separation of Concerns**: Calculation logic is separated from the ViewModel and WPF presentation layer.

---

## Project Architecture & MVVM Implementation

The solution is split into independent projects, fully satisfying the MVVM pattern and clean architecture:

### 1. Model / Core (`CrazyUniverse.Core`)

- **Independent Business Logic**: Contains characters (animals), generic repositories, interfaces, domain events, and reaction rules completely separated from UI buttons, windows, or WPF dependencies.
- **Animal.cs** (Abstract base class): Manages shared state (`Satiety` from 0 to 100 with `protected set` for strict encapsulation), generates unique identifiers via `Guid`, handles base feeding logic (`Feed`), and defines the abstract `CrazyAction()` contract. Implements `INotifyPropertyChanged`.
- **Concrete Models**: `Monkey`, `Penguin`, `Lion`, `HoneyBadger`, etc., implement unique polymorphic behaviors, reaction handlers, and **defensive input validation** (negative inputs like `Climb(-200)` are safely caught and rejected without altering the valid `Satiety` state).
- **Interfaces**: Contracts for capabilities (`IJumpable`, `ISwimmable`, `IRunnable`, `IClimbable`, `IVocalizable`) and generic repositories (`IRepository<T>`).
- **Simulation Models**: `AnimalSnapshot`, `SimulationProgress`, and `SimulationResult` provide data structures for animal calculations, progress reporting, and simulation results.
- **Simulation Service**: `AnimalSimulationService` contains the CPU-intensive calculation logic, periodically checks cancellation requests, reports progress, measures individual calculation time, and handles success, cancellation, and error results.
- **Helpers & Resources**:
  - **AnimalActionsHelper.cs**: Helper utilities for checking animal capabilities dynamically using pattern matching (`is`) and type casting (`as`).
  - **Messages.resx (Resource File)**: Centralized string resource file storing all localized messages, action descriptions, error strings, and event log formats.

### 2. ViewModel (`CrazyUniverse.WpfApp/ViewModels`)

- **State & Binding Management**: `MainViewModel` manages selected animals, enclosure collections, global zoo population tracking (`ZooPopulation`), LINQ-derived state, and user commands.
- **SimulationViewModel**: Manages selected animals, simulation workload, sequential and parallel execution, progress updates, cancellation, results, and execution statistics.
- **Commands (`ICommand`)**: User actions (Feed, Swim, Jump, Make Sound, Crazy Action, Add, Remove) are encapsulated via `RelayCommand`.
- **Command Guards (`CanExecute`)**: Dynamically disables commands and UI elements if no suitable animal is selected, if energy is depleted, or while a simulation is running. The modal dialog (`AddAnimalViewModel`) also checks required name/description fields before enabling submission commands.
- **Reactive Updates**: Utilizes `ObservableCollection<T>` and `INotifyPropertyChanged` to automatically synchronize lists, progress bars, counters, and event logs.

### 3. View (`CrazyUniverse.WpfApp`)

- **WPF Presentation Layer**: XAML windows (`MainWindow`, `AddAnimal`, `SimulationWindow`) are bound to ViewModels via data bindings, converters, and styles. Features accordion-style enclosures with empty-state placeholders and a simulation interface for sequential and parallel calculations.

---

## Design Patterns

### Factory Pattern (`AnimalFactory`)

To adhere to the Single Responsibility Principle and decouple object creation from UI and business logic:

- Located in `CrazyUniverse.WpfApp/Factories/AnimalFactory.cs`.
- Encapsulates instantiation logic for all concrete animal types (`Monkey`, `Lion`, `Penguin`, `HoneyBadger`, etc.).
- Dynamically creates objects based on parameters passed from the modal dialog (`AddAnimal`), ensuring clean architecture.

---

## Verified Use Cases

1. **Animal Initialization & Generics**: Creating typed repositories and populating default zoo animals with unique identifiers and starting satiety levels, and suppressed startup logs.
2. **Repository Addition & Welcome Events**: Adding a new character triggers greeting logs from existing enclosure neighbors without self-greeting.
3. **Repository Removal & Farewell Events**: Removing a character triggers farewell logs from remaining neighbors; the deleted instance safely unsubscribes and stops responding.
4. **Crazy Action Execution**: Triggering `CrazyAction()` causes neighbor reactions and state updates within the isolated enclosure while keeping other enclosures unaffected.
5. **Capability Checking (`is`/`as`)**: Using `AnimalActionsHelper` to verify interface implementations safely.
6. **Global Population Tracking**: Real-time synchronization of the `ZooPopulation` counter across all enclosure repositories.
7. **LINQ Data Operations**: Live filtering by name, alphabetical sorting toggles, and grouped satiety range statistics.
8. **Robust Input Validation**: Passing negative arguments (e.g., `Climb(-200)` or `Swim(-200)`) successfully triggers localized error messages while preserving the object's valid `Satiety` state, and dialog forms enforce filled inputs before enabling submission commands.

### Vol.3 Simulation Use Cases

The following scenarios are intended for checking the simulation functionality:

1. **Sequential Execution**: Select animals, set a workload, and run the sequential mode. Each calculation should finish before the next one starts.
2. **Parallel Execution**: Use the same animal selection and workload to run the parallel mode and compare the total execution time.
3. **Progress Updates**: Observe the progress percentage and current calculation step on each selected animal card.
4. **Error Handling**: Enable the test error option and run the simulation with Lion selected. The Lion result should show `Error`, while other animal tasks should continue.
5. **Cancellation**: Start a simulation and press the Cancel button. Running calculations should stop after checking the cancellation token, and animals that have not started should be marked as cancelled.
6. **Result Summary**: Check individual statuses, result details, execution times, total time, and the success/error/cancellation counters.

---

## Peer Review & Collaboration

Tasks, guidelines, and review status for project contributors and peer reviewers are tracked directly via GitHub Issues and Pull Requests:

- **[Issue #5: Add new animal subclass and integrate capabilities](https://github.com/IvannaZimina/Crazy-Universe/issues/5)** (Maksym Kotkov)
  - *Pull Request*: **[PR #1](https://github.com/IvannaZimina/Crazy-Universe/pull/1)** (HoneyBadger integration)
  - *Code Review & Verification*: Checked by project owner for inheritance (`Animal` base class, `protected set` encapsulation), interface implementation (`IClimbable`, `ISwimmable`), clean compilation, and robust input validation (negative arguments like `-200` are safely rejected without altering `Satiety`).
- **[Issue #6: Add and integrate three new animal types](https://github.com/IvannaZimina/Crazy-Universe/issues/6)** (Arina Shishkova)
  - *Pull Request*: **[PR #4](https://github.com/IvannaZimina/Crazy-Universe/pull/4)** (Adding three new animal types, integrating them into repositories, events, and UI)
  - *Code Review & Verification*: Successfully merged and verified. Added three new animal types, integrated them into generic repositories, domain events (`LogEventArgs`), and MVVM data bindings with proper owner review.
- **[Issue #7: Guidelines & Recommendations for collaborators](https://github.com/IvannaZimina/Crazy-Universe/issues/7)** — General collaboration rules, clean code practices, and PR workflows managed via GitHub.

---

## Artificial Intelligence (AI) Usage Declaration

While developing this project, I used artificial intelligence (Gemini) as an architectural and technical support assistant. It was used for:

- Task analysis & action planning: Breaking down course assignment requirements into structured steps and defining the Core/WpfApp project architecture.
- OOP principles & code refinement: Assisting in structuring abstract classes and applying polymorphism correctly.
- Validation & state management: Discussing logic for defensive input validation to protect object states from corruption.
- Asynchronous programming: Discussing task execution, cancellation, progress reporting, and error handling.
- Documentation: Structuring and refining the README.md file and drafting professional code comments.

---

## View

<img width="1081" height="712" alt="image" src="https://github.com/user-attachments/assets/c2b67cb6-f867-40d3-a54d-bb88bb36f86b" />
<img width="1079" height="702" alt="image" src="https://github.com/user-attachments/assets/a450660a-3cb1-45c8-92fa-00985ce1932f" />
<img width="1078" height="700" alt="image" src="https://github.com/user-attachments/assets/ae74e0a5-787a-4735-b426-a6f90b37a95d" />
<img width="1078" height="700" alt="image" src="https://github.com/user-attachments/assets/b7db0ce7-18ba-44c3-8cd7-2cf486220252" />
<img width="1079" height="712" alt="image" src="https://github.com/user-attachments/assets/f8b656ad-0601-41f8-ba0d-27e8d3d8d896" />
<img width="1078" height="702" alt="image" src="https://github.com/user-attachments/assets/7d04d6af-bb24-40ef-a41b-3e3200ca2bcc" />
<img width="1076" height="703" alt="image" src="https://github.com/user-attachments/assets/a82ae57f-a21e-4e2e-be68-9632f437ae28" />
