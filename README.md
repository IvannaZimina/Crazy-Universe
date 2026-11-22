# Crazy Universe

Welcome to the Crazy Universe repository!

Crazy Universe is a humorous WPF application where different types of animals live in the same world, have unique behaviours, interact with each other, and react to events.

The project was developed across course assignment stages. Each version extends the previous one, adding new programming concepts and functionality while keeping the same solution and repository.

The application is built using separate Core and WPF projects and demonstrates object-oriented programming, generic repositories, domain events, LINQ, MVVM, asynchronous programming, parallel execution, dependency injection, and multi-provider persistence.

---

## Technologies Used

- Programming Language: C#
- Platform: .NET / WPF
- UI: XAML, data binding, commands
- Architecture: MVVM
- Asynchronous Programming: async/await, Task, Task.Run, Task.WhenAll
- Cancellation: CancellationToken, CancellationTokenSource
- Performance Measurement: Stopwatch
- Collections: ObservableCollection, generic collections
- Data Operations: LINQ
- Persistence & Serialization: System.Text.Json, XmlSerializer, ADO.NET (Microsoft.Data.SqlClient)
- Database: SQL Server Express LocalDB ((localdb)\MSSQLLocalDB)
- Dependency Injection & Configuration: Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Configuration.Json
- Version Control: Git and GitHub
- Development Environment: Visual Studio 2022 or newer

---

## Running Guide

1. Clone the repository: https://github.com/IvannaZimina/Crazy-Universe
2. Open the solution file (.slnx) in Visual Studio 2022 or newer.
3. Set CrazyUniverse.WpfApp as the Startup Project.
4. Configure persistence in CrazyUniverse.WpfApp/appsettings.json (Storage:Provider = Json, Xml, or LocalDb, plus Storage:ConnectionString for LocalDB).
5. Check SQL Server Express LocalDB when using LocalDb via sqllocaldb info and sqllocaldb start MSSQLLocalDB. Default connection string: `Server=(localdb)\MSSQLLocalDB;Database=CrazyUniverseDb;Trusted_Connection=True;TrustServerCertificate=True;`
6. Press F5 or click Start to run the application.

---

## Project Architecture

The solution consists of two main projects:

### 1. Core — CrazyUniverse.Core

Contains the domain models, business logic, world orchestration, repositories, interfaces, events, simulation services, and persistence/storage abstractions.
The Core project does not depend on WPF.

Main components:

- Animal.cs: Abstract base class for all animal types. Defines shared properties, identity, satiety state, feeding logic, neighbor reaction logic, and the abstract CrazyAction() method.
- Concrete Animal Classes: Monkey, Penguin, Lion, HoneyBadger, Kangaroo, Dolphin, and Frog implement their own behaviours and special actions.
- World (ZooWorld & AnimalCatalog): Encapsulates type-based repositories, animal type catalog entries, creation/restoration helpers, and forwarded domain events in Core.
- Interfaces: Define animal capabilities such as IJumpable, ISwimmable, IRunnable, IClimbable, and IVocalizable.
- Repositories: IRepository<T> and Repository<T> provide type-safe storage and management of animal objects.
- Domain Events: LogEventArgs and event-related logic handle animal interactions and reactions.
- Persistence (CrazyUniverse.Core/Persistence): WorldDto, EnclosureDto, and CharacterDto represent plain serializable world snapshots; WorldValidator verifies schema version, catalog consistency, GUIDs, string bounds, and satiety (0..100); WorldMapper maps between ZooWorld and WorldDto; InvalidWorldDataException wraps structural, semantic, and parse errors.
- Storage (CrazyUniverse.Core/Storage): IWorldStorage defines the async persistence contract (SaveAsync, LoadAsync) implemented by JsonWorldStorage, XmlWorldStorage, and SqlWorldStorage (LocalDB via parameterized ADO.NET transactions).
- Simulation Models: AnimalSnapshot, SimulationProgress, and SimulationResult provide data for simulation calculations, progress reporting, and results.
- AnimalSimulationService: Performs CPU-intensive calculations, reports progress, checks cancellation requests, and handles individual calculation results.
- AnimalActionsHelper: Provides utilities for checking animal capabilities using pattern matching and type checking.
- Messages.resx: Centralized resource file containing application messages, action descriptions, error messages, and log formats.

### 2. ViewModel — CrazyUniverse.WpfApp/ViewModels

The ViewModels manage application state, commands, data binding, and communication between the Core and View layers.

- MainViewModel: Connects UI collections, enclosure views, population statistics, LINQ operations, and world save/load commands to ZooWorld and IWorldStorage.
- SimulationViewModel: Manages selected animals, workload settings, sequential and parallel simulation modes, mode comparison, progress updates, cancellation, results, and execution statistics.
- AddAnimalViewModel: Manages the animal creation dialog and validates the required input fields.
- RelayCommand & AsyncRelayCommand: Encapsulate synchronous and asynchronous user actions using ICommand and support CanExecute conditions.
- SimulationCardViewModel: Provides data and state for individual animal simulation cards.

### 3. View — CrazyUniverse.WpfApp

The WPF presentation layer contains the XAML windows, controls, styles, converters, and data templates.

Main windows:
- MainWindow — the main application interface with animal enclosures, world save/load buttons, and interaction controls.
- AddAnimal — a modal dialog for creating new animals.
- SimulationWindow — the interface for running and monitoring animal simulations.

The View layer uses data binding and commands to communicate with the ViewModels. Business logic remains in the Core project.

---

## Vol.1 — Object-Oriented Programming and WPF

The first version introduced the Crazy Zoo world, its animal hierarchy, individual behaviours, and the basic WPF interface.

### Object-Oriented Design

- Inheritance: An abstract Animal base class defines shared functionality for concrete animal types.
- Interfaces: Animal capabilities are represented by interfaces such as IJumpable, ISwimmable, IRunnable, IClimbable, and IVocalizable.
- Polymorphism: Concrete animal classes override abstract or virtual methods to provide their own behaviour.
- Encapsulation: Animal identity is assigned during construction, and mutable state is protected from direct external modification.
- Input Validation: Invalid arguments are rejected without changing the object's valid state.
- Capability Checking: The application uses is and as operators to check supported animal capabilities.

### Animal Behaviour

Each animal has its own characteristics and actions.

- Animals can perform common actions such as feeding.
- Capability interfaces define additional actions available to suitable animal types.
- Every concrete animal type implements its own CrazyAction().
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

- IRepository<T>: Defines the repository contract.
- Repository<T>: Implements the generic repository functionality.
- Type Constraints: Uses where T : Animal to ensure type safety.
- Separate Repositories: Each concrete animal type has its own repository, for example Repository<Monkey>, Repository<Lion>, Repository<Penguin>, and Repository<HoneyBadger>.
- Repository Operations: Supports adding, removing, reading, and searching for animals.
- Collection Protection: GetAll() provides a read-only view of the internal collection, preventing external modification of repository contents.

### 2. Domain Events and Neighbour Reactions

Animal interactions are implemented in the Core domain layer rather than in WPF button event handlers.

Three main event scenarios are supported:
- Addition — Welcome: When a new animal is added, existing animals in the same repository greet it. The newly added animal does not react to itself.
- Removal — Farewell: When an animal is removed, the remaining animals in the same repository say goodbye. The removed animal is removed from the repository list and no longer participates in subsequent repository reaction loops.
- CrazyAction: When an animal performs its special action, other animals in the same repository react according to their individual behaviour.

Additional rules:
- Animals in different repositories do not react to each other's events.
- The animal that caused an event does not react to itself.
- Reactions create log entries.
- Neighbour reactions to CrazyAction update the reacting animal's satiety within validated bounds (0..100).
- Typed event arguments (LogEventArgs) provide the event kind (Kind) and the animal that caused the event (Cause).

### 3. LINQ Operations

The application implements three LINQ operations and displays their results in the UI.

- Filtering — Where: Searches animals by name using case-insensitive comparison.
- Sorting — OrderBy / OrderByDescending: Sorts animals alphabetically in ascending or descending order.
- Grouping — GroupBy: Groups animals by satiety ranges and displays population statistics.

The satiety groups are:
- Low: 30 or less.
- Medium: More than 30 and up to 70.
- High: More than 70.

### 4. MVVM Implementation

The application follows the Model-View-ViewModel separation.

- Model/Core: Contains animal classes, world/catalog orchestration, repositories, domain events, and reaction rules.
- ViewModel: Manages selected animals, collections, LINQ results, application state, logs, and commands.
- View: Displays data and sends user actions through data bindings and commands.

The implementation uses:
- ICommand, RelayCommand, and AsyncRelayCommand for user actions.
- CanExecute to control command availability.
- INotifyPropertyChanged for property change notifications.
- ObservableCollection<T> for automatic collection updates.
- Data binding to keep the UI synchronized with application state.

### 5. Partner Contribution

A fellow student contributed three new animal types to the existing Crazy Zoo project.

The contribution was developed in a separate branch and submitted through a pull request. The changes were reviewed and merged into the main branch. Details and links are documented in the Peer Review & Collaboration section below.

---

## Vol.3 — Asynchronous and Parallel Animal Simulation

Vol.3 extends the existing project with CPU-intensive calculations for selected animals, enhanced UI grouping, filtering, mode comparison, and asynchronous execution monitoring.

The user can select animals via enclosure groups, configure the workload, run calculations in two modes, compare execution times and calculated values, and inspect individual results.

### 1. Animal Snapshots and Calculations

- AnimalSnapshot: Stores a fixed copy of the selected animal's data (Id, Name, AnimalType, Satiety) before the simulation starts.
- CPU-Intensive Calculations: Each selected animal performs a deterministic calculation based on its type, snapshot properties, and workload.
- Same Input Data & Mode Comparison: Sequential and parallel modes use the same snapshots and workload; when both modes have run on the same input, the UI displays sequential time, parallel time, speed-up, and the count of matching values within relative tolerance (|a - b| <= 1e-9 * max(1, |a|)). Stored comparison data resets when selection, workload, or error simulation settings change.

### 2. Execution Modes

The application provides two execution modes:

- Sequential Mode: Calculations run one after another. The next animal's calculation starts only after the previous one has finished.
- Parallel Mode: Each animal calculation is started using Task.Run, and all tasks are awaited using Task.WhenAll.

### 3. Progress Reporting and UI Filtering

- IProgress<SimulationProgress> is used to report calculation progress.
- Enclosure Grouping & Selection: Animals in the selection panel are grouped by enclosures (EnclosureSimulationGroup) with localized enclosure headers, allowing users to toggle individual animal selection with checkboxes.
- Dynamic Filtering: The central simulation grid automatically filters and displays only the animals where IsSelected == true.
- Individual Asset Images: Each animal type features its own custom profile image asset (Monkey, Lion, Penguin, HoneyBadger, Kangaroo, Dolphin, Frog).

### 4. Cancellation

- A shared CancellationTokenSource is used for each simulation run.
- The cancellation token is passed to all calculations.
- CPU-intensive loops periodically check the token and stop when cancellation is requested.
- Animals that have not started when cancellation is requested are also handled in the final report.

### 5. Error Handling and Results

Each animal calculation produces an individual result.

Possible statuses:
- Success: The calculation completed successfully.
- Error: The calculation failed and the error message is included in the result.
- Cancelled: The calculation was cancelled.

An error in one animal's calculation does not prevent other calculations from completing.

The simulation interface displays:
- Animal name and custom image.
- Final calculation status.
- Result details (including calculated value) or error message.
- Individual execution time.
- Total simulation time (including milliseconds mm:ss.fff).
- Counters for successful, failed, and cancelled calculations.
- Mode comparison summary when both modes were run on identical input.

Stopwatch is used to measure the total duration of each simulation run.
A test error option is available to demonstrate error handling with the Lion simulation.
The simulation uses try/finally to restore the application state and release the cancellation source after a run.

---

## Vol.4 (Task 6-1) — Multi-Provider World Persistence & Dependency Injection

Task 6-1 introduces full world state persistence and silent atomic restoration across file and relational providers without breaking domain encapsulation, repository isolation, or neighbour reaction subscriptions.

### 1. Persisted vs. Non-Persisted State

- Persisted: Schema Version, enclosure metadata (Id, Name, AnimalType), and each character's Id (GUID), concrete Type key (Monkey, Lion, Penguin, HoneyBadger, Kangaroo, Dolphin, Frog), Name, Description, Satiety (0..100), and EnclosureId.
- Not Persisted: WPF windows, ICommand instances, UI selection state, event log history, database connections, and domain event subscriptions (LogMessageGenerated / neighbour reaction handlers are preserved on the live Repository<T> instances).

### 2. Validation & Silent Atomic Restore

- WorldValidator: Validates schema version, enclosure-to-catalog mapping, GUID format and uniqueness, non-blank Name (<= 100), Description (<= 500), Satiety (0..100 — invalid values are rejected, never clamped), and valid EnclosureId before mutating the world.
- ZooWorld.Restore: Reconstructs all domain animals via AnimalCatalog.RestoreAnimal(...) first and atomically replaces items inside the existing 7 Repository<T> instances without triggering welcome/farewell neighbour reactions or log noise. If validation or deserialization fails, the active world remains untouched.

### 3. Storage Providers & Configuration (appsettings.json)

The active IWorldStorage implementation is selected once at startup via DI in App.xaml.cs from CrazyUniverse.WpfApp/appsettings.json:

- Json (JsonWorldStorage): Indented UTF-8 JSON using System.Text.Json and async file streams.
- Xml (XmlWorldStorage): XML serialization using XmlSerializer (XmlRoot, Version attribute, XmlArray / XmlArrayItem) and async file I/O.
- LocalDb (SqlWorldStorage): Relational persistence in SQL Server LocalDB using Microsoft.Data.SqlClient, auto-creating CrazyUniverseDb and tables (Enclosures, Characters with primary keys, foreign key, and CHECK constraints) if missing, and saving inside a single SqlTransaction with parameterized queries.

See docs/DATABASE.md for the database schema, ER diagram, and LocalDB setup details.

---

## Design Patterns and Factories

### AnimalFactory

Located in CrazyUniverse.WpfApp/Services/AnimalFactory.cs (namespace CrazyUniverse.WpfApp.Factories).

- Delegates animal creation, default animals, and available type keys to AnimalCatalog in CrazyUniverse.Core.
- Supports the animal creation dialog.
- Keeps animal instantiation logic separate from the UI.

### AnimalCardFactory

Located in CrazyUniverse.WpfApp/Services/AnimalCardFactory.cs.

- Creates simulation animal cards dynamically from active zoo animals (CreateCardsFromAnimals).
- Assigns individual image assets, simulation titles, and specific descriptions based on the animal type (Monkey, Lion, Penguin, HoneyBadger, Kangaroo, Dolphin, Frog).
- Keeps simulation card creation separate from the simulation ViewModel.

---

## WPF UI Elements

The application uses several WPF elements to display animal information and simulation progress cleanly and efficiently.

- Expander & ItemsControl: Organizes animals into separate enclosure groups with interactive checkboxes in the simulation panel.
- ScrollViewer: Provides vertical scrolling for both enclosure groups and the central simulation card grid to support large numbers of animals.
- ProgressBar: Displays real-time progress percentages and step counts for individual simulation calculations.
- UniformGrid: Formats the central simulation cards into a structured multi-column layout.
- Balanced Results & Logs Panel: Splits the lower section evenly between the execution results DataGrid and the real-time EventLog console.
- Data Binding and Data Templates: Connect ViewModel data to the UI and define how animal and simulation card information is displayed.

These elements help organize the interface and provide visual feedback during user interactions and long-running calculations.

---

## Verified Use Cases

The following scenarios cover the main functionality introduced across all project versions.

### Vol.1 — Basic Animal Functionality

1. Animal Initialization: Create animals with unique identifiers and initial satiety values.
2. Animal Actions: Select an animal and perform available actions such as feeding, jumping, swimming, or climbing.
3. CrazyAction: Execute the special action of different animal types and observe their individual behaviour.
4. Input Validation: Pass invalid arguments, such as negative climbing or swimming values, and verify that the animal's valid state remains unchanged.
5. Animal Management: Add, select, and remove animals through the WPF interface.

### Vol.2 — Repositories and Interactions

6. Repository Addition: Add an animal and verify that existing animals in the same repository react to it.
7. Repository Removal: Remove an animal and verify that the remaining animals react, while the removed animal no longer participates in subsequent events.
8. Event Isolation: Verify that animals in other repositories do not react to events from a different repository.
9. State Changes: Verify that the required neighbour reactions create log entries and update the reacting animal's state.
10. LINQ Operations: Test filtering, sorting, and grouping in the UI.
11. Command Validation: Verify that commands are enabled or disabled according to the selected animal and required input.

### Vol.3 — Simulation & UI Enhancements

12. Enclosure Selection: Group animals by enclosures in the simulation panel and toggle selections using checkboxes.
13. Central Grid Filtering: Verify that the central simulation view dynamically displays only the selected animals.
14. Sequential Execution: Set a workload and run the sequential mode. Each calculation must finish before the next one starts.
15. Parallel Execution: Run the same selection and workload in parallel mode and compare the total execution time.
16. Progress Updates: Observe progress information and individual card progress bars.
17. Error Handling: Enable the test error option and run the simulation with Lion selected. The Lion result should show Error, while other calculations continue.
18. Cancellation: Start a simulation and press Cancel. Running calculations should stop after checking the cancellation token, and unstarted animals should be marked as cancelled.
19. Result Summary & Balanced Logs: Check individual statuses, result details, execution times, total time, success/error/cancellation counters, and review events in the balanced console log.
20. Compare Sequential and Parallel Results: Run both modes with the same selection, workload, and error setting, verify matching calculated values and speed-up in the comparison block, and verify reset when parameters change.

### Vol.4 (Task 6-1) — World Persistence & Providers

21. JSON Save & Restore: Add an animal, modify satiety via feeding/actions, save to .json, restart the app, and load the file — verify IDs, types, names, descriptions, satiety, and enclosure assignments match.
22. XML Save & Restore: Switch Storage:Provider to Xml, save to .xml, restart the app, and load the file (including repeated loads) — verify single XML declaration and full state restoration.
23. Post-Restore Neighbour Reactions: Trigger CrazyAction in a restored world and verify only neighbours in the same enclosure react, while loading itself produced zero reactions or extra log entries.
24. Corrupt & Invalid Data Handling: Load malformed JSON/XML or semantically invalid data (e.g., Satiety = 500, duplicate ID, unknown animal type) — verify a clear error appears in the Event Log, the app does not crash, and the active world remains unchanged.
25. File Dialog Cancellation: Cancel the Save or Open file dialog — verify the operation aborts cleanly without errors or state mutation.
26. LocalDB (ADO.NET) Persistence: Switch Storage:Provider to LocalDb, click Save World (no file dialog shown), restart the app, and click Load World — verify relational data persists and restores cleanly via SqlWorldStorage.

---

## Peer Review & Collaboration

Tasks and collaboration are tracked through GitHub Issues and Pull Requests.

- Issue #5: Add new animal subclass and integrate capabilities — Maksym Kotkov
  - Pull Request #1: feat: add HoneyBadger model (https://github.com/IvannaZimina/Crazy-Universe/pull/1)

- Issue #6: Add and integrate three new animal types — Arina Shishkova
  - Pull Request #4: feat: add and integrate three new animal types (https://github.com/IvannaZimina/Crazy-Universe/pull/4)

- Issue #9: Implement asynchronous and parallel animal simulation (Vol.3)
  - Pull Request #8: Feature/crazy zoo vol3 (https://github.com/IvannaZimina/Crazy-Universe/pull/8)

---

## Artificial Intelligence (AI) Usage Declaration

While developing this project, I used artificial intelligence (Gemini) as an architectural and technical support assistant. It was used for:

- Task analysis & action planning: Breaking down course assignment requirements into structured steps and defining the Core/WpfApp project architecture.
- OOP principles & code refinement: Assisting in structuring abstract classes and applying polymorphism correctly.
- Validation & state management: Discussing logic for defensive input validation to protect object states from corruption.
- Asynchronous programming: Discussing task execution, cancellation, progress reporting, and error handling.
- Persistence & DI architecture: Designing the IWorldStorage provider abstraction (Json, Xml, LocalDb), DTO validation pipeline, silent atomic restore, and startup DI configuration.
- Documentation: Structuring and refining the README.md file and drafting professional code comments.

---

## View

<img width="1081" height="712" alt="image" src="https://github.com/user-attachments/assets/c2b67cb6-f867-40d3-a54d-bb88bb36f86b" />
<img width="1079" height="702" alt="image" src="https://github.com/user-attachments/assets/a450660a-3cb1-45c8-92fa-00985ce1932f" />
<img width="1078" height="700" alt="image" src="https://github.com/user-attachments/assets/ae74e0a5-787a-4735-b426-a6f90b37a95d" />
<img width="1157" height="730" alt="image" src="https://github.com/user-attachments/assets/ad90d066-d80d-4f87-9e38-617671156b99" />
<img width="1157" height="753" alt="image" src="https://github.com/user-attachments/assets/9c857400-1415-4b30-aa04-474fecedd43a" />