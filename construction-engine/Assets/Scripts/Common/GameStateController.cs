﻿namespace Common
{
    /// <summary>
    /// Base class for game state controllers.
    /// </summary>
    public abstract class GameStateController
    {
        /// <summary>
        /// Called each game step while this state is active.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Called once as this state is newly started.
        /// </summary>
        /// <param name="context">Optionally arguments to give context to the new state.</param>
        public abstract void TransitionIn(object context);

        /// <summary>
        /// Called once as this state is ending.
        /// </summary>
        public abstract void TransitionOut();
    }
}
