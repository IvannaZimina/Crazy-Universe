# Crazy Universe (Crazy Zoo + WPF Design)

Welcome to the **Crazy Universe** repository!  
This is a humorous WPF application where different types of objects live in the same world but behave differently.

---

## Application Description

**Crazy Zoo** is a humorous simulator where every animal has a unique behavior, a satiety state, and characteristics.  
Users can interact with the animals (such as feeding them, making them perform actions via a modal dialog for adding new items) and observe their unique crazy actions in real-time.  
The solution consists of separate Core and WPF projects and uses inheritance, interfaces, polymorphism, and collections, following the course assignment requirements.

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

## Architectural & Design Principles

- **Architectural Pattern**: MVVM (Model-View-ViewModel) with strict separation of concerns (Core & UI split)
- **Design Patterns**: Factory Pattern (`AnimalFactory`) for decoupled object creation
- **Object-Oriented Programming**: Encapsulation, abstraction, polymorphism, and inheritance

---

## Project Architecture & MVVM Implementation

The solution is split into two independent projects, fully satisfying the requirements of the MVVM pattern and clean architecture:

### 1. Model / Core (`CrazyUniverse.Core`)
- **Independent Business Logic**: Contains characters (animals), interfaces, events, and reaction rules completely separated from UI buttons, windows, or WPF dependencies.
- **Animal.cs** (Abstract base class): Manages shared state (`Satiety` from 0 to 100 with `protected set`), generates unique identifiers via `Guid`, handles base feeding logic (`Feed`), and defines the abstract `CrazyAction()` contract. Implements `INotifyPropertyChanged`.
- **Concrete Models**: `Monkey`, `Penguin`, `Lion`, `HoneyBadger` implement unique polymorphic behaviors depending on their current satiety state.
- **Interfaces**: Contracts for capabilities (`IJumpable`, `ISwimmable`, `IRunnable`, `IClimbable`, `IVocalizable`).
- **Helpers & Resources**:
  - **AnimalActionsHelper.cs**: Helper utilities for checking animal capabilities dynamically using pattern matching (`is`) and type casting (`as`).
  - **Messages.resx (Resource File)**: Centralized string resource file storing all localized messages, action descriptions, error strings, and UI labels. Keeps core business logic completely free of hardcoded strings.

### 2. ViewModel (`CrazyUniverse.WpfApp/ViewModels`)
- **State & Binding Management**: `MainViewModel` and `AddAnimalViewModel` manage the selected animal storage, displayed lists, user inputs, LINQ-derived state, and user command execution.
- **Commands (`ICommand`)**: User actions (Feed, Swim, Jump, Make Sound, Crazy Action, Add, Remove) are encapsulated via `RelayCommand`. 
- **Command Guards (`CanExecute`)**: Dynamically disables commands and UI elements if no suitable animal/option is selected or if required inputs are missing (e.g., when energy is exhausted or fields are empty).
- **Reactive Updates**: Utilizes `ObservableCollection<Animal>` and `INotifyPropertyChanged` to automatically synchronize lists, progress bars, counters, and event logs without placing business logic in code-behind.

### 3. View (`CrazyUniverse.WpfApp`)
- **WPF Presentation Layer**: XAML windows (`MainWindow`, `AddAnimal`) bound directly to ViewModels via data bindings, converters, and styles. Users can select repositories/animals, manage items interactively, and view real-time logs.

---

## Design Patterns

### Factory Pattern (`AnimalFactory`)
To adhere to the Single Responsibility Principle and decouple object creation from UI and business logic, the application implements the **Factory Pattern**:
- Located in `CrazyUniverse.WpfApp/Services/AnimalFactory.cs`.
- Encapsulates the instantiation logic for all concrete animal types (`Monkey`, `Lion`, `Penguin`, `HoneyBadger`).
- Dynamically creates objects based on parameters passed from the modal dialog (`AddAnimal`), ensuring clean, extensible, and maintainable code architecture.

---

## Collection & WPF User Interface

- **Dynamic Collection**: Objects are stored in an `ObservableCollection<Animal>`, allowing automatic synchronization of list modifications with the visual UI.
- **Data Display**: The main application window displays the full list of animals alongside detailed information regarding the currently selected object.
- **Object Management & Modal Dialog**: Users can interactively add new instances via a dedicated modal dialog window (`AddAnimal`), select them from the list to inspect details, and remove unwanted items. The dialog dynamically binds available animal types supporting localization and mandatory field validation (Name & Description with red asterisk indicators).
- **Dynamic Actions**: Action buttons dynamically adapt to the selected animal, reflecting accurate energy costs and invoking corresponding methods.
- **Event Log**: Core and action methods return meaningful timestamped messages recorded in a dedicated user interface event log with automatic scrolling.
- **Fault Tolerance & Validation**: Text and numerical inputs are strictly validated; invalid parameters do not cause application crashes.

---

## Verified Use Cases

1. **Animal Initialization**: Creating default zoo animals with unique `Guid` identifiers and starting satiety levels through the factory service.
2. **Feeding an Animal**: Calling the `Feed()` method to increase satiety (capped at 100).
3. **Adding via Dialog**: Opening the `AddAnimal` modal window, selecting a localized animal type, filling mandatory fields, and instantiating via `AnimalFactory`.
4. **Performing a Crazy Action**: Triggering `CrazyAction()` to reduce energy and output exhausted or normal behavioral messages.
5. **Capability Checking via Helper (`is`/`as`)**: Using `AnimalActionsHelper` to dynamically verify interface implementations safely.
6. **Invalid Input Protection**: Guarding against negative distances or zero values to protect object states.

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






