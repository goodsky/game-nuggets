using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common
{
    /// <summary>
    /// Loads annotated state controller types for the state machine.
    /// </summary>
    public static class StateControllerLoader
    {
        public static IEnumerable<(GameState, GameStateMachine.Controller)> LoadControllers()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                StateControllerAttribute attribute = type.GetCustomAttribute<StateControllerAttribute>();
                if (attribute != null)
                {
                    if (!typeof(GameStateMachine.Controller).IsAssignableFrom(type))
                    {
                        GameLogger.Error("Attempted to load type '{0}' into the state machine, but it is not a State Machine Controller!", type.Name);
                        continue;
                    }

                    yield return (attribute.HandledState, (GameStateMachine.Controller)Activator.CreateInstance(type));
                }
            }
        }
    }

    public class StateControllerAttribute : Attribute
    {
        public GameState HandledState { get; set; }
    }
}
