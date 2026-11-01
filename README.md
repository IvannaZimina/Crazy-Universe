# Crazy Universe — Vol.1, Vol.2 & Vol.3

Welcome to the **Crazy Universe** repository!

Crazy Universe is a humorous WPF application where different types of animals live in the same world, have unique behaviours, interact with each other, and react to events.

The project was developed in three stages as part of the course assignments. Each version extends the previous one, adding new programming concepts and functionality while keeping the same solution and repository.

The application is built using separate **Core** and **WPF** projects and demonstrates object-oriented programming, generic repositories, domain events, LINQ, MVVM, asynchronous programming, and parallel execution.

---

## Technologies Used

- **Programming Language:** C#
- **Platform:** .NET / WPF
- **UI:** XAML, data binding, commands
- **Architecture:** MVVM
- **Asynchronous Programming:** async/await, Task, Task.Run, Task.WhenAll
- **Cancellation:** CancellationToken, CancellationTokenSource
- **Performance Measurement:** Stopwatch
- **Collections:** ObservableCollection, generic collections
- **Data Operations:** LINQ
- **Version Control:** Git and GitHub
- **Development Environment:** Visual Studio 2022 or newer

---

## Running Guide

1. Clone the repository:

   `https://github.com/IvannaZimina/Crazy-Universe`

2. Open the solution file (`.slnx`) in Visual Studio 2022 or newer.
3. Set `CrazyUniverse.WpfApp` as the Startup Project.
4. Press **F5** or click **Start** to run the application.

---

## Project Architecture

The solution consists of two main projects:

### 1. Core — `CrazyUniverse.Core`

Contains the domain models, business logic, repositories, interfaces, events, and simulation services.

The Core project does not depend on WPF.

**Main components:**

- **Animal.cs:** Abstract base class for all animal types. Defines shared properties, identity, satiety state, feeding logic, and the abstract `CrazyAction()` method.
- **Concrete Animal Classes:** `Monkey`, `Penguin`, `Lion`, `HoneyBadger`, and other animal types implement their own behaviours and special actions.
- **Interfaces:** Define animal capabilities such as `IJumpable`, `ISwimmable`, `IRunnable`, `IClimbable`, and `IVocalizable`.
- **Repositories:** `IRepository<T>` and `Repository<T>` provide type-safe storage and management of animal objects.
- **Domain Events:** `LogEventArgs` and event-related logic handle animal interactions and reactions.
- **Simulation Models:** `AnimalSnapshot`, `SimulationProgress`, and `SimulationResult` provide data for simulation calculations, progress reporting, and results.
- **AnimalSimulationService:** Performs CPU-intensive calculations, reports progress, checks cancellation requests, and handles individual calculation results.
- **AnimalActionsHelper:** Provides utilities for checking animal capabilities using pattern matching and type checking.
- **Messages.resx:** Centralized resource file containing application messages, action descriptions, error messages, and log formats.

### 2. ViewModel — `CrazyUniverse.WpfApp/ViewModels`

The ViewModels manage application state, commands, data binding, and communication between the Core and View layers.

- **MainViewModel:** Manages animal repositories, selected animals, enclosure collections, population statistics, LINQ operations, and user commands.
- **SimulationViewModel:** Manages selected animals, workload settings, sequential and parallel simulation modes, progress updates, cancellation, results, and execution statistics.
- **AddAnimalViewModel:** Manages the animal creation dialog and validates the required input fields.
- **RelayCommand:** Encapsulates user actions using the `ICommand` interface and supports `CanExecute` conditions.
- **SimulationCardViewModel:** Provides data and state for individual animal simulation cards.

### 3. View — `CrazyUniverse.WpfApp`

The WPF presentation layer contains the XAML windows, controls, styles, converters, and data templates.

Main windows:

- `MainWindow` — the main application interface with animal enclosures and interaction controls.
- `AddAnimal` — a modal dialog for creating new animals.
- `SimulationWindow` — the interface for running and monitoring animal simulations.

The View layer uses data binding and commands to communicate with the ViewModels. Business logic remains in the Core project.

---

## Vol.1 — Object-Oriented Programming and WPF

The first version introduced the Crazy Zoo world, its animal hierarchy, individual behaviours, and the basic WPF interface.

### Object-Oriented Design

- **Inheritance:** An abstract `Animal` base class defines shared functionality for concrete animal types.
- **Interfaces:** Animal capabilities are represented by interfaces such as `IJumpable`, `ISwimmable`, `IRunnable`, `IClimbable`, and `IVocalizable`.
- **Polymorphism:** Concrete animal classes override abstract or virtual methods to provide their own behaviour.
- **Encapsulation:** Animal identity is assigned during construction, and mutable state is protected from direct external modification.
- **Input Validation:** Invalid arguments are rejected without changing the object's valid state.
- **Capability Checking:** The application uses `is` and `as` operators to check supported animal capabilities.

### Animal Behaviour

Each animal has its own characteristics and actions.

- Animals can perform common actions such as feeding.
- Capability interfaces define additional actions available to suitable animal types.
- Every concrete animal type implements its own `CrazyAction()`.
- Crazy actions can use or change the animal's validated state and return a message for the UI.

### WPF Interface

The first version introduced the main application interface.

Users can:

- View animals and their details.
- Select an animal.
- Add new animals through a modal dialog.
- Remove animals.
- Perform available actions.
- View action messages in the application log.

---

## Vol.2 — Repositories, Domain Events, LINQ and MVVM

Vol.2 extends the original Crazy Zoo project with type-based repositories, domain events, neighbour reactions, LINQ operations, and a structured MVVM implementation.

### 1. Generic Repositories

The application uses a generic repository architecture.

- **IRepository<T>:** Defines the repository contract.
- **Repository<T>:** Implements the generic repository functionality.
- **Type Constraints:** Uses `where T : Animal` to ensure type safety.
- **Separate Repositories:** Each concrete animal type has its own repository, for example `Repository<Monkey>`, `Repository<Lion>`, `Repository<Penguin>`, and `Repository<HoneyBadger>`.
- **Repository Operations:** Supports adding, removing, reading, and searching for animals.
- **Collection Protection:** `GetAll()` provides a read-only view of the internal collection, preventing external modification of repository contents.

### 2. Domain Events and Neighbour Reactions

Animal interactions are implemented in the Core domain layer rather than in WPF button event handlers.

Three main event scenarios are supported:

- **Addition — Welcome:** When a new animal is added, existing animals in the same repository greet it. The newly added animal does not react to itself.
- **Removal — Farewell:** When an animal is removed, the remaining animals in the same repository say goodbye. The removed animal unsubscribes from events and no longer reacts to future events.
- **CrazyAction:** When an animal performs its special action, other animals in the same repository react according to their individual behaviour.

Additional rules:

- Animals in different repositories do not react to each other's events.
- The animal that caused an event does not react to itself.
- Reactions create log entries.
- At least one type of reaction changes the reacting animal's state.
- Typed event arguments (`LogEventArgs`) provide the event kind (`Kind`) and the animal that caused the event (`Cause`).

### 3. LINQ Operations

The application implements three LINQ operations and displays their results in the UI.

- **Filtering — `Where`:** Searches animals by name using case-insensitive comparison.
- **Sorting — `OrderBy` / `OrderByDescending`:** Sorts animals alphabetically in ascending or descending order.
- **Grouping — `GroupBy`:** Groups animals by satiety ranges and displays population statistics.

The satiety groups are:

- **Low:** 30 or less.
- **Medium:** More than 30 and up to 70.
- **High:** More than 70.

### 4. MVVM Implementation

The application follows the Model-View-ViewModel separation.

- **Model/Core:** Contains animal classes, repositories, domain events, and reaction rules.
- **ViewModel:** Manages selected animals, collections, LINQ results, application state, logs, and commands.
- **View:** Displays data and sends user actions through data bindings and commands.

The implementation uses:

- `ICommand` and `RelayCommand` for user actions.
- `CanExecute` to control command availability.
- `INotifyPropertyChanged` for property change notifications.
- `ObservableCollection<T>` for automatic collection updates.
- Data binding to keep the UI synchronized with application state.

### 5. Partner Contribution

A fellow student contributed three new animal types to the existing Crazy Zoo project.

The contribution was developed in a separate branch and submitted through a pull request. The changes were reviewed and merged into the main branch.

- **Partner:** [Partner name]
- **Issue:** [Issue link]
- **Pull Request:** [Pull request link]

---

## Vol.3 — Asynchronous and Parallel Animal Simulation

Vol.3 extends the existing project with CPU-intensive calculations for selected animals.

The user can select animals, configure the workload, run calculations in two modes, compare execution times, and inspect individual results.

### 1. Animal Snapshots and Calculations

- **AnimalSnapshot:** Stores a fixed copy of the selected animal's data before the simulation starts.
- **CPU-Intensive Calculations:** Each selected animal performs a calculation based on its type and properties.
- **Same Input Data:** Sequential and parallel modes use the same animal snapshots and workload, allowing a fair comparison.

### 2. Execution Modes

The application provides two execution modes:

- **Sequential Mode:** Calculations run one after another. The next animal's calculation starts only after the previous one has finished.
- **Parallel Mode:** Each animal calculation is started using `Task.Run`, and all tasks are awaited using `Task.WhenAll`.

### 3. Progress Reporting

- `IProgress<SimulationProgress>` is used to report calculation progress.
- The UI displays progress information on individual animal cards.
- The simulation interface shows the current calculation state and progress.

### 4. Cancellation

- A shared `CancellationTokenSource` is used for each simulation run.
- The cancellation token is passed to all calculations.
- CPU-intensive loops periodically check the token and stop when cancellation is requested.
- Animals that have not started when cancellation is requested are also handled in the final report.

### 5. Error Handling and Results

Each animal calculation produces an individual result.

Possible statuses:

- **Success:** The calculation completed successfully.
- **Error:** The calculation failed and the error message is included in the result.
- **Cancelled:** The calculation was cancelled.

An error in one animal's calculation does not prevent other calculations from completing.

The simulation interface displays:

- Animal name.
- Final calculation status.
- Result details or error message.
- Individual execution time.
- Total simulation time.
- Counters for successful, failed, and cancelled calculations.

`Stopwatch` is used to measure the total duration of each simulation run.

A test error option is available to demonstrate error handling with the Lion simulation.

The simulation uses `try/finally` to restore the application state and release the cancellation source after a run.

---

## Design Patterns and Factories

### AnimalFactory

Located in `CrazyUniverse.WpfApp/Factories/AnimalFactory.cs`.

- Encapsulates the creation of concrete animal objects.
- Supports the animal creation dialog.
- Keeps animal instantiation logic separate from the UI.

### AnimalCardFactory

Located in `CrazyUniverse.WpfApp/Services/AnimalCardFactory.cs`.

- Creates the default simulation animal cards.
- Initializes card data such as animal names, IDs, images, titles, and descriptions.
- Keeps simulation card creation separate from the simulation ViewModel.

---

## WPF UI Elements

The application uses several WPF elements to display animal information and simulation progress.

- **Expander:** Organizes animals into separate enclosures and allows users to expand or collapse their contents.
- **ProgressBar:** Displays the progress of individual simulation calculations.
- **Data Binding and Data Templates:** Connect ViewModel data to the UI and define how animal and simulation card information is displayed.

These elements help organize the interface and provide visual feedback during user interactions and long-running calculations.

---

## Verified Use Cases

The following scenarios cover the main functionality introduced across all three project versions.

### Vol.1 — Basic Animal Functionality

1. **Animal Initialization:** Create animals with unique identifiers and initial satiety values.
2. **Animal Actions:** Select an animal and perform available actions such as feeding, jumping, swimming, or climbing.
3. **CrazyAction:** Execute the special action of different animal types and observe their individual behaviour.
4. **Input Validation:** Pass invalid arguments, such as negative climbing or swimming values, and verify that the animal's valid state remains unchanged.
5. **Animal Management:** Add, select, and remove animals through the WPF interface.

### Vol.2 — Repositories and Interactions

6. **Repository Addition:** Add an animal and verify that existing animals in the same repository react to it.
7. **Repository Removal:** Remove an animal and verify that the remaining animals react, while the removed animal no longer receives events.
8. **Event Isolation:** Verify that animals in other repositories do not react to events from a different repository.
9. **State Changes:** Verify that the required neighbour reactions create log entries and update the reacting animal's state.
10. **LINQ Operations:** Test filtering, sorting, and grouping in the UI.
11. **Command Validation:** Verify that commands are enabled or disabled according to the selected animal and required input.

### Vol.3 — Simulation

12. **Sequential Execution:** Select animals, set a workload, and run the sequential mode. Each calculation must finish before the next one starts.
13. **Parallel Execution:** Run the same selection and workload in parallel mode and compare the total execution time.
14. **Progress Updates:** Observe progress information on the selected animal cards.
15. **Error Handling:** Enable the test error option and run the simulation with Lion selected. The Lion result should show `Error`, while other calculations continue.
16. **Cancellation:** Start a simulation and press Cancel. Running calculations should stop after checking the cancellation token, and animals that have not started should be marked as cancelled.
17. **Result Summary:** Check individual statuses, result details, execution times, total time, and the success/error/cancellation counters.

---

## Peer Review & Collaboration

Tasks, collaboration guidelines, and review status are tracked through GitHub Issues and Pull Requests.

- **[Issue #5: Add new animal subclass and integrate capabilities](https://github.com/IvannaZimina/Crazy-Universe/issues/5)** — Maksym Kotkov
  - **Pull Request:** [PR #1: feat: add HoneyBadger model](https://github.com/IvannaZimina/Crazy-Universe/pull/1)
  - **Code Review & Verification:** Reviewed and tested by the project owner. The `HoneyBadger` subclass, its `CrazyAction()` behavior, and capability interfaces (`IClimbable`, `ISwimmable`, `IRunnable`, `IVocalizable`) were checked. The PR was successfully merged.

- **[Issue #6: Add and integrate three new animal types](https://github.com/IvannaZimina/Crazy-Universe/issues/6)** — Arina Shishkova
  - **Pull Request:** [PR #4: feat: add and integrate three new animal types](https://github.com/IvannaZimina/Crazy-Universe/pull/4)
  - **Code Review & Verification:** Reviewed, tested, and merged by the project owner. Added Kangaroo, Dolphin, and Frog, integrating them into `AnimalFactory`, repositories, enclosures, the AddAnimal dialog, and UI actions.

- **[Issue #9: Implement asynchronous and parallel animal simulation (Vol.3)](https://github.com/IvannaZimina/Crazy-Universe/issues/9)**
  - **Pull Request:** [PR #8: Feature/crazy zoo vol3](https://github.com/IvannaZimina/Crazy-Universe/pull/8)
  - **Status:** Open — submitted for review. Implements CPU-intensive simulations, sequential and parallel execution, cancellation, error handling, and execution-time reporting.

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
<img width="1157" height="753" alt="image" src="https://github.com/user-attachments/assets/8626e0fc-f18b-4bb8-a149-5d8fac1a376c" />
<img width="1357" height="892" alt="image" src="https://github.com/user-attachments/assets/778562fa-6506-4ce0-bfd2-240afe86a7eb" />


