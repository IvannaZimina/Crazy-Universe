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
* **Programming Language**: C#
* **Platform**: .NET / WPF
* **Architectural Principles**: Object-Oriented Programming (encapsulation, abstraction, polymorphism, inheritance)
* **Version Control**: Git

---

## Project Architecture (CrazyUniverse.Core)

The core of the project contains the domain logic, completely separated from the user interface:

### 1. Models
* **Animal.cs** (Abstract base class):
  * Manages the shared state of all animals (encapsulated `Satiety` ranging from 0 to 100).
  * Automatically generates a unique identifier `Id` using `Guid`.
  * Contains the base feeding logic (`Feed`) and defines the contract through the abstract `CrazyAction()` method.
* **Monkey.cs, Penguin.cs, Lion.cs** (Concrete subclass models):
  * Inherit from `Animal` and use the base constructor (`: base(name)`).
  * Implement unique versions of the `CrazyAction()` method, defining individual behaviors for each animal.

### 2. Interfaces
* **IClimbable.cs** - Interface for animals that can climb (for example, monkeys).
* **IFlyable.cs** - Interface for animals that can fly or glide.
* **ISwimmable.cs** - Interface for animals that can swim (for example, penguins).