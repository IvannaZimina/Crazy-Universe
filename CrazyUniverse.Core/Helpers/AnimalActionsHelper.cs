using System;
using CrazyUniverse.Core.Interfaces;
using CrazyUniverse.Core.Models;

namespace CrazyUniverse.Core.Helpers
{
    // Demonstrates the use of 'is' and 'as' operators to check object capabilities.
    public static class AnimalActionsHelper
    {
        // Method that checks what additional interface roles an animal supports 
        // using pattern matching ('is') and type casting ('as').
        public static string ExecuteSpecialAbility(Animal animal)
        {
            // 1. Using the 'is' operator with pattern matching to check if the animal can jump
            // jumpableAnimal - pattern variable
            if (animal is IJumpable jumpableAnimal)
            {
                return jumpableAnimal.Jump();
            }

            // the old method of syntax
            // if (animal is IJumpable)
            // {
            //     IJumpable j = (IJumpable)animal;
            //     return j.Jump();
            // }

            // 2. Using the 'as' operator to check if the animal can swim (returns null if it can't)
            var swimmableAnimal = animal as ISwimmable;
            if (swimmableAnimal != null)
            {
                return swimmableAnimal.Swim(10.0); // Передаем требуемую дистанцию
            }

            // 3. Using the 'is' operator to check if the animal can run
            if (animal is IRunnable runnableAnimal)
            {
                return runnableAnimal.Run();
            }

            // 4. Using the 'as' operator to check if the animal can climb
            var climbableAnimal = animal as IClimbable;
            if (climbableAnimal != null)
            {
                return climbableAnimal.Climb(5.0); // Передаем требуемую высоту
            }

            // 5. Using the 'is' operator to check if the animal can make a sound
            if (animal is IVocalizable vocalAnimal)
            {
                return vocalAnimal.MakeSound();
            }

            return $"{animal.Name} has no extra interface abilities registered.";
        }
    }
}