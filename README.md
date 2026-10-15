# Crazy Universe (Crazy Zoo + WPF Design)

Welcome to the **Crazy Universe** repository!  
This is a humorous WPF application where different types of objects live in the same world but behave, interact, and react to each other dynamically.

---

## Application Description

**Crazy Zoo** is a humorous simulator where every animal has a unique behavior, a satiety state, and specific traits.  
Users can interact with the animals (such as feeding them, making them perform actions, or adding new characters via a modal dialog) and observe their unique crazy actions and neighborhood reactions in real-time.  

The solution consists of separate Core and WPF projects and uses inheritance, generics, interfaces, polymorphism, events, and collections, fully adhering to advanced course assignment requirements.

---

## Technologies Used

- **Programming Language**: C#
- **Platform**: .NET / WPF
- **Version Control**: Git
- **Development Environment**: Visual Studio 2022+

---

## Running Guide

1. Clone the repository to your local machine:  
   `https://github.com/IvannaZimina/Crazy-Universe`
2. Open the solution file (`.sln`) in **Visual Studio** (version 2022 or newer is recommended).
3. Ensure that the `CrazyUniverse.WpfApp` project is set as the **Startup Project**.
4. Press **F5** (or click the **Start** button) to run the application.

---

## Core Assignment Requirements & Implementation Status

### 1. Type-Based Generic Repositories (`IRepository<T>` & `Repository<T>`)
- **Generic Architecture**: Built using a general `IRepository<T>` interface and concrete `Repository<T>` implementation supporting methods: `Add(T item)`, `Remove(T item)`, `IEnumerable<T> GetAll()`, and `T? Find(Func<T, bool> predicate)`.
- **Type Segregation**: Uses dedicated repositories for each concrete animal type (e.g., `Repository<Monkey>`, `Repository<Lion>`, `Repository<Penguin>`, `Repository<HoneyBadger>`).
- **Compile-Time Safety**: Implements strict type constraints (`where T : Animal`), preventing improper types at compile time without relying on UI name-matching.

### 2. Domain Events & Neighbor Reactions (`Core/Model`)
Events and reactions are fully managed within the domain layer (`Core/Model`), entirely decoupled from WPF button click handlers:
- **Addition (Welcome)**: When a new character is added to a repository, existing objects in the same enclosure greet the newcomer. The new object does not greet itself.
- **Removal (Farewell)**: When a character is removed, remaining objects bid farewell. The removed object unsubscribes from events and ceases to respond to subsequent triggers.
- **CrazyAction**: The selected character performs its unique special action. Other objects in the same enclosure react according to world logic. The initiator does not react to itself.
- **Isolation**: Objects in different repositories (enclosures) do not answer or react. Every response creates an event log entry, and specific reactions affect the responding object's state (e.g., energy/satiety changes).

---

## Architectural & Design Principles

- **Architectural Pattern**: MVVM (Model-View-ViewModel) with strict separation of concerns (Core & UI split).
- **Design Patterns**: Factory Pattern (`AnimalFactory`) for decoupled object creation.
- **Object-Oriented Programming**: Encapsulation, abstraction, polymorphism, generics, and inheritance.

---

## Project Architecture & MVVM Implementation

The solution is split into independent projects, fully satisfying the MVVM pattern and clean architecture:

### 1. Model / Core (`CrazyUniverse.Core`)
- **Independent Business Logic**: Contains characters (animals), generic repositories, interfaces, domain events, and reaction rules completely separated from UI buttons, windows, or WPF dependencies.
- **Animal.cs** (Abstract base class): Manages shared state (`Satiety` from 0 to 100 with `protected set`), generates unique identifiers via `Guid`, handles base feeding logic (`Feed`), and defines the abstract `CrazyAction()` contract. Implements `INotifyPropertyChanged`.
- **Concrete Models**: `Monkey`, `Penguin`, `Lion`, and `HoneyBadger` implement unique polymorphic behaviors and reaction handlers.
- **Interfaces**: Contracts for capabilities (`IJumpable`, `ISwimmable`, `IRunnable`, `IClimbable`, `IVocalizable`) and generic repositories (`IRepository<T>`).
- **Helpers & Resources**:
  - **AnimalActionsHelper.cs**: Helper utilities for checking animal capabilities dynamically using pattern matching (`is`) and type casting (`as`).
  - **Messages.resx (Resource File)**: Centralized string resource file storing all localized messages, action descriptions, error strings, and event log formats.

### 2. ViewModel (`CrazyUniverse.WpfApp/ViewModels`)
- **State & Binding Management**: `MainViewModel` manages selected animals, enclosure collections, global zoo population tracking (`ZooPopulation`), LINQ-derived state, and user commands.
- **Commands (`ICommand`)**: User actions (Feed, Swim, Jump, Make Sound, Crazy Action, Add, Remove) are encapsulated via `RelayCommand`. 
- **Command Guards (`CanExecute`)**: Dynamically disables commands and UI elements if no suitable animal is selected or if energy is depleted.
- **Reactive Updates**: Utilizes `ObservableCollection<Animal>` and `INotifyPropertyChanged` to automatically synchronize lists, progress bars, counters, and event logs.

### 3. View (`CrazyUniverse.WpfApp`)
- **WPF Presentation Layer**: XAML windows (`MainWindow`, `AddAnimal`) bound directly to ViewModels via data bindings, converters, and styles. Features accordion-style enclosures with empty-state placeholders ("Пока здесь никто не живет 🐾").

---

## Design Patterns

### Factory Pattern (`AnimalFactory`)
To adhere to the Single Responsibility Principle and decouple object creation from UI and business logic:
- Located in `CrazyUniverse.WpfApp/Factories/AnimalFactory.cs`.
- Encapsulates instantiation logic for all concrete animal types (`Monkey`, `Lion`, `Penguin`, `HoneyBadger`).
- Dynamically creates objects based on parameters passed from the modal dialog (`AddAnimal`), ensuring clean architecture.

---

## Verified Use Cases

1. **Animal Initialization & Generics**: Creating typed repositories and populating default zoo animals with unique identifiers and starting satiety levels.
2. **Repository Addition & Welcome Events**: Adding a new character triggers greeting logs from existing enclosure neighbors without self-greeting.
3. **Repository Removal & Farewell Events**: Removing a character triggers farewell logs from remaining neighbors; the deleted instance safely unsubscribes and stops responding.
4. **Crazy Action Execution**: Triggering `CrazyAction()` causes neighbor reactions and state updates within the isolated enclosure while keeping other enclosures unaffected.
5. **Capability Checking (`is`/`as`)**: Using `AnimalActionsHelper` to verify interface implementations safely.
6. **Global Population Tracking**: Real-time synchronization of the `ZooPopulation` counter across all enclosure repositories.

---

## Peer Review & Collaboration

### 1. **Peer Name**: Maksym Kotkov
- **Repository Link**: [https://github.com/IvannaZimina/Crazy-Universe](https://github.com/IvannaZimina/Crazy-Universe)
- **Status**: **Completed & Merged**
- **Task for Reviewer / Classmate**: 
  - Create a new concrete animal subclass inheriting from `Animal` (e.g., `Elephant`, `Giraffe`, or `Kangaroo`).
  - Implement the abstract `CrazyAction()` method with unique behavior based on `Satiety`.
  - Implement at least two relevant interfaces available in the project (such as `IJumpable`, `ISwimmable`, or `IVocalizable`) to utilize the existing action architecture and capability-checking helpers.

**Guidelines & Recommendations for Maksym**:
- **Clean Code**: Follow consistent naming conventions, keep methods short and focused, and avoid code duplication.
- **Comments**: Add XML documentation comments (`///`) to public classes and methods.
- **Git Commits**: Write clear, descriptive commit messages with prefixes (e.g., `feat: add Elephant model`, `fix: validate satiety range`).

---

### 2. **Peer Name**: Arina Shishkova
- **Repository Link**: [https://github.com/IvannaZimina/Crazy-Universe](https://github.com/IvannaZimina/Crazy-Universe)
- **Status**: **In Progress / Invited for Collaboration**
- **Task for Reviewer / Classmate**: 
  - Add three new concrete animal subclasses inheriting from `Animal` (e.g., `Elephant`, `Kangaroo`, `Dolphin`, etc.), each featuring unique behavior.
  - Implement the abstract `CrazyAction()` method with unique behavior based on `Satiety`.
  - Implement at least two relevant capability interfaces available in the project (`IJumpable`, `ISwimmable`, `IVocalizable`, `IFlyable`, `IRunnable`, `IClimbable`).
  - Register the new animal types in `AnimalFactory.cs` so they can be instantiated dynamically via the `AddAnimal` dialog and ensure initial instances are created in `AnimalFactory.CreateDefaultAnimals()`.

**Guidelines & Recommendations for collaborators**:
- **Clean Code**: Follow consistent naming conventions, keep methods short and focused, and avoid code duplication.
- **Comments**: Add XML documentation comments (`///`) to public classes and methods.
- **Git Commits**: Write clear, descriptive commit messages using Conventional Commit prefixes:
  - `feat:` — for adding new features or models (e.g., `feat: add 3 new animal models`).
  - `fix:` — for fixing bugs or UI glitches (e.g., `fix: update factory mapping and messages`).
  - `docs:` — for updating documentation or comments (for README).
  - `refactor:` — for code restructuring without changing functionality.
  - `style:` — for formatting and visual adjustments.
  - `chore:` — for configuration or project setting updates.
- **PR & Merge Workflow**: Work in a separate feature branch (`feature/arina-new-animals`), create a Pull Request to `main`, and wait for code review and merge by the primary author.

---

## Artificial Intelligence (AI) Usage Declaration

While developing this project, I used artificial intelligence (Gemini) as an architectural and technical support assistant. It was used for:
- Task analysis & action planning: Breaking down course assignment requirements into structured steps and defining the Core/WpfApp project architecture.
- OOP principles & code refinement: Assisting in structuring abstract classes and applying polymorphism correctly.
- Validation & state management: Discussing logic for defensive input validation to protect object states from corruption.
- Documentation: Structuring and refining the README.md file and drafting professional code comments.
---

## View
<img width="1069" height="700" alt="image" src="https://github.com/user-attachments/assets/b6fecf06-71ee-485b-b70e-c53997757f4b" />
<img width="1076" height="701" alt="image" src="https://github.com/user-attachments/assets/727371fc-c095-4eb8-96d9-650266e8750f" />
<img width="1074" height="701" alt="image" src="https://github.com/user-attachments/assets/ba06f3a8-8f39-4f20-94da-6e24679981c4" />
<img width="1073" height="707" alt="image" src="https://github.com/user-attachments/assets/34d61f97-35bf-49a9-9d8f-1ada7eddac95" />
<img width="1072" height="706" alt="image" src="https://github.com/user-attachments/assets/fa931c13-3119-428c-af0e-6273fae7ef7f" />
<img width="1074" height="698" alt="image" src="https://github.com/user-attachments/assets/414842c4-af28-4aec-bf4d-68d46a20a882" />
<img width="1075" height="704" alt="image" src="https://github.com/user-attachments/assets/6a4b66af-5072-4503-813b-59b305a32ffb" />






