# Crazy Universe (Crazy Zoo + WPF Design)

Welcome to the **Crazy Universe** repository!  
This is a humorous WPF application where different types of objects live in the same world but behave differently.

---

## Application Description
**Crazy Zoo** is a humorous simulator where every animal has a unique behavior, a satiety state, and characteristics.  
Users can interact with the animals (such as feeding them) and observe their unique crazy actions in real-time.  
The solution consists of separate Core and WPF projects and uses inheritance, interfaces, polymorphism, and collections, following the course assignment requirements.

---

## Technologies Used
- **Programming Language**: C#
- **Platform**: .NET / WPF
- **Architectural Principles**: Object-Oriented Programming (encapsulation, abstraction, polymorphism, inheritance), MVVM-like separation (Core & UI split)
- **Version Control**: Git

---

## Running Guide
1. Clone the repository to your local machine: `https://github.com/IvannaZimina/Crazy-Universe`
2. Open the solution file (`.sln`) in **Visual Studio** (version 2022 or newer is recommended).
3. Ensure that the `CrazyUniverse.WpfApp` project is set as the **Startup Project**.
4. Press **F5** (or click the **Start** button) to run the application.

---

## Project Architecture (CrazyUniverse.Core)

The core of the project contains the domain logic, completely separated from the user interface:

### 1. Models
- **Animal.cs** (Abstract base class):
  - Manages the shared state of all animals (encapsulated `Satiety` ranging from 0 to 100 with `protected set`).
  - Automatically generates a unique identifier `Id` using `Guid`.
  - Contains the base feeding logic (`Feed`) and defines the contract through the abstract `CrazyAction()` method.
  - Implements `INotifyPropertyChanged` to support reactive UI updates.
- **Monkey.cs, Penguin.cs, Lion.cs** (Concrete subclass models):
  - Inherit from `Animal` and use the base constructor (`: base(name)`).
  - Implement unique versions of the `CrazyAction()` method, which dynamically impact and depend on the current `Satiety` state.
  - Include defensive input validation ensuring that invalid inputs do not alter the object's state.

### 2. Interfaces
- **IJumpable.cs**: Interface for animals that can jump.
- **ISwimmable.cs**: Interface for animals that can swim (with distance parameter).
- **IRunnable.cs**: Interface for animals that can run.
- **IClimbable.cs**: Interface for animals that can climb (with height parameter).
- **IVocalizable.cs**: Interface for animals that can make sounds.
- **IFlyable.cs**: Interface for animals that can fly or glide.

### 3. Helpers & Resources
- **AnimalActionsHelper.cs**:
  - Centralized component for checking animal capabilities dynamically.
  - Explicitly demonstrates the use of both pattern matching (`is`) and traditional type casting (`as`) operators to invoke specific interface methods safely.
- **Localization / Resource Files**:
  - Centralized string resources (`Messages.resx`) used for multi-language support and maintaining clean separation of hardcoded UI strings.

---

## Collection & WPF User Interface

- **Dynamic Collection**: Objects are stored in an `ObservableCollection<Animal>`, allowing automatic synchronization of list modifications with the visual UI.
- **Data Display**: The main application window displays the full list of animals alongside detailed information regarding the currently selected object.
- **Object Management**: Users can interactively add new instances, select them from the list to inspect details, and remove unwanted items.
- **Dynamic Actions**: Action buttons (Feed, Swim, Jump, Make Sound, Crazy Action) dynamically adapt to the selected animal and invoke the corresponding methods of the object or its implemented interfaces.
- **Event Log**: Core and action methods return meaningful timestamped messages that are recorded in a dedicated user interface event log with automatic scrolling.
- **Fault Tolerance & Validation**: Text and numerical inputs are strictly validated; invalid parameters do not cause application crashes.
- **Clean Architecture**: Domain business rules are isolated within the `CrazyUniverse.Core` project, while the WPF application project is strictly responsible for presentation management and user input.

### Justification of Self-Studied WPF UI Elements (`DataTemplate` & `ProgressBar`)
To implement the user interface, the following WPF features were self-studied and integrated:
- **`DataTemplate` (within `ListBox` / content areas)**: Chosen to build custom visual layouts for each animal item in the collection. This allows clean, declarative data binding connecting model properties (`Name`, `TypeName`, `Satiety`) directly to UI elements without writing bloated procedural code-behind.
- **`ProgressBar`**: Utilized to visually represent the animal's current satiety level in real-time (both inside the list view on the left and the detailed center card), making the application state intuitive and lively.

---

## Crazy Actions & State Rules
- **Encapsulated State (`Satiety`)**: Every animal maintains a satiety level ranging from 0 to 100, protected from external modification via `protected set`.
- **Defensive Input Validation**: Numerical parameters (such as swimming distance) are strictly validated. Negative or zero values do not alter the object's state.
- **Polymorphic `CrazyAction()`**: Defined as an abstract method in the `Animal` base class and overridden in each subclass (`Penguin`, `Monkey`, `Lion`). It dynamically decreases satiety and changes the animal's reaction text depending on its exhaustion level (e.g., when satiety drops below 20).

---

## Verified Use Cases
1. **Animal Initialization**: Creating an instance of a penguin named "Skipper" with an automatically generated `Guid` identifier and a starting satiety of 100.
2. **Feeding an Animal**: Calling the `Feed()` method, which increases the animal's satiety level (capped at a maximum of 100).
3. **Performing a Crazy Action**: Triggering `CrazyAction()` for a lion named "Simba", which reduces its satiety and outputs a majestic roar text reflecting its current energy state.
4. **Capability Checking via Helper (`is`/`as`)**: Using `AnimalActionsHelper` to dynamically and safely verify whether a specific object implements `ISwimmable` or `IJumpable`.
5. **Invalid Input Protection**: Passing a negative distance (`-10.0`) into the penguin's swim method — the system correctly handles the error, leaving the satiety state completely untouched.

---

## TO DO: Peer Review & Collaboration
- **Peer Name**: Maksym Kotkov
- **Repository Link**: [https://github.com/IvannaZimina/Crazy-Universe](https://github.com/IvannaZimina/Crazy-Universe)
- **Task for Reviewer / Classmate**: 
  - Create a new concrete animal subclass inheriting from `Animal` (e.g., `Elephant`, `Giraffe`, or `Kangaroo`).
  - Implement the abstract `CrazyAction()` method with unique behavior based on `Satiety`.
  - Implement at least two relevant interfaces available in the project (such as `IJumpable`, `ISwimmable`, or `IVocalizable`) to utilize the existing action architecture and capability-checking helpers.
- **Guidelines & Recommendations for Maksym**:
  - **Clean Code**: Follow consistent naming conventions, keep methods short and focused, and avoid code duplication.
  - **Comments**: Add XML documentation comments (`///`) to public classes and methods.
  - **Git Commits**: Write clear, descriptive commit messages with prefixes (e.g., `feat: add Elephant model`, `fix: validate satiety range`).

---

## Artificial Intelligence (AI) Usage Declaration
While developing this project, I used artificial intelligence (Gemini) as an architectural and technical support assistant. It was used for:
- Task analysis & action planning: Breaking down course assignment requirements into structured steps and defining the Core/WpfApp project architecture.
- OOP principles & code refinement: Assisting in structuring abstract classes and applying polymorphism correctly.
- Validation & state management: Discussing logic for defensive input validation to protect object states from corruption.
- Documentation: Structuring and refining the README.md file and drafting professional code comments.

---

## View
<img width="1157" height="786" alt="image" src="https://github.com/user-attachments/assets/47fbdcfc-406e-4cb1-a3ae-294a2dfa4a03" />
<img width="1157" height="794" alt="image" src="https://github.com/user-attachments/assets/ed924d92-668f-489c-86ad-99219e4d7df6" />
<img width="1157" height="773" alt="image" src="https://github.com/user-attachments/assets/6532a6d7-d4ef-4ec2-984b-6a41aa9bb780" />
<img width="1157" height="789" alt="image" src="https://github.com/user-attachments/assets/e6eaa3b7-4309-4479-b83a-08def38ad165" />