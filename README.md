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
- **Architectural Principles**: Object-Oriented Programming (encapsulation, abstraction, polymorphism, inheritance)
- **Version Control**: Git

---

## Running Guide
1. Clone the repository to your local machine.
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

### 3. Helpers
- **AnimalActionsHelper.cs**:
  - Centralized component for checking animal capabilities dynamically.
  - Explicitly demonstrates the use of both pattern matching (`is`) and traditional type casting (`as`) operators to invoke specific interface methods safely.

  ---

## Crazy Actions & State Rules
- **Encapsulated State (`Satiety`)**: Every animal maintains a satiety level ranging from 0 to 100, protected from external modification via `protected set`.
- **Defensive Input Validation**: Numerical parameters (such as swimming distance) are strictly validated. Negative or zero values do not alter the object's state.
- **Polymorphic `CrazyAction()`**: Defined as an abstract method in the `Animal` base class and overridden in each subclass (`Penguin`, `Monkey`, `Lion`). It dynamically decreases satiety and changes the animal's reaction text depending on its exhaustion level (e.g., when satiety drops below 20).

---

## TO DO: Verified Use Cases
1. **Animal Initialization**: Creating an instance of a penguin named "Skipper" with an automatically generated `Guid` identifier and a starting satiety of 100.
2. **Feeding an Animal**: Calling the `Feed()` method, which increases the animal's satiety level (capped at a maximum of 100).
3. **Performing a Crazy Action**: Triggering `CrazyAction()` for a lion named "Simba", which reduces its satiety and outputs a majestic roar text reflecting its current energy state.
4. **Capability Checking via Helper (`is`/`as`)**: Using `AnimalActionsHelper` to dynamically and safely verify whether a specific object implements `ISwimmable` or `IJumpable`.
5. **Invalid Input Protection**: Passing a negative distance (`-10.0`) into the penguin's swim method — the system correctly handles the error, leaving the satiety state completely untouched.

---

## TO DO: WPF UI Element Justification
- **Selected Element**: [e.g., ListView with custom DataTemplates]
- **Justification**: [Explain why this self-studied WPF component was chosen to display and manage the collection of animals, allowing flexible data binding and custom visual layouts for each animal type.]

---

## TO DO: Peer Review & Collaboration
- **Peer Name**: [Insert classmate's name]
- **Issue Link**: [Insert link to the GitHub issue]
- **Pull Request (PR) Link**: [Insert link to the pull request and code review]
---

## Artificial Intelligence (AI) Usage Declaration
While developing this project, I used artificial intelligence (Gemini) as an architectural and technical support assistant. It was used for:
- Task analysis & action planning: Breaking down course assignment requirements into structured steps and defining the Core/WPF project architecture.
- OOP principles & code refinement: Assisting in structuring abstract classes and applying polymorphism correctly.
- Validation & state management: Discussing logic for defensive input validation to protect object states from corruption.
- Documentation: Structuring and refining the README.md file and drafting professional code comments in English.
