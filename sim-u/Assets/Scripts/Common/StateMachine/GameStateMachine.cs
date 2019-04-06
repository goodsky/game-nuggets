﻿using Campus.GridTerrain;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// Enumeration of possible game states.
    /// </summary>
    public enum GameState
    {
        /// <summary>The default game state. You are selecting your next state.</summary>
        Selecting,

        /// <summary>Constructing a new entity on the campus.</summary>
        PlacingConstruction,

        /// <summary>Selecting the start position of the path.</summary>
        SelectingPath,

        /// <summary>Creating the path.</summary>
        PlacingPath,

        /// <summary>Selecting campus terrain to modify.</summary>
        SelectingTerrain,

        /// <summary>Modifying the campus terrain.</summary>
        EditingTerrain,
    }

    /// <summary>
    /// State machine of possible campus interaction modes.
    /// Methods represent edges between states in the machine.
    /// States can register controller actions to happen during a state.
    /// </summary>
    public partial class GameStateMachine : MonoBehaviour
    {
        /// <summary>The current campus game state.</summary>
        public GameState Current { get; private set; }

        // Mapping of possible game states with controllers to execute during their turn.
        private static readonly List<Controller> EmptyControllers = new List<Controller>(0);
        private readonly Dictionary<GameState, List<Controller>> _stateControllers = new Dictionary<GameState, List<Controller>>();
        private List<Controller> _currentStateControllers = EmptyControllers;

        private readonly object _setLock = new object();

        private TerrainSelectionUpdateArgs _lastTerrainLocation = null;

        /// <summary>
        /// Unity start method.
        /// </summary>
        protected void Start()
        {
            Current = GameState.Selecting;
        }

        /// <summary>
        /// Unity update method.
        /// </summary>
        protected void Update()
        {
            foreach (var controller in _currentStateControllers)
            {
                controller.Update();
            }
        }

        /// <summary>
        /// Register a new state controller with the state machine.
        /// </summary>
        /// <param name="state">The state to activate the controller for.</param>
        /// <param name="controller">The controller to register.</param>
        public void RegisterController(GameState state, Controller controller)
        {
            if (!_stateControllers.ContainsKey(state))
            {
                _stateControllers[state] = new List<Controller>();
            }

            _stateControllers[state].Add(controller);
        }

        /// <summary>
        /// Transition into a new initial state.
        /// If you pass me a non-initial state I'll scream.
        /// </summary>
        public void StartDoing(GameState newState, object context = null)
        {
            if (newState != GameState.SelectingTerrain &&
                newState != GameState.PlacingConstruction &&
                newState != GameState.SelectingPath)
            {
                throw new InvalidOperationException(string.Format("Cannot start doing state! {0}", newState.ToString()));
            }

            Transition(newState, context);
        }

        /// <summary>
        /// Stop doing what you're doing.
        /// </summary>
        public void StopDoing()
        {
            if (Current == GameState.EditingTerrain)
            {
                Transition(GameState.SelectingTerrain);
            }
            else
            {
                Transition(GameState.Selecting);
            }
        }

        /// <summary>
        /// The terrain was clicked.
        /// </summary>
        /// <param name="button">The mouse button.</param>
        /// <param name="clickLocation">Location on the grid that was clicked.</param>
        public void ClickedTerrain(TerrainClickedArgs args)
        {
            foreach (var controller in _currentStateControllers)
            {
                controller.TerrainClicked(args);
            }
        }

        /// <summary>
        /// The selection on the terrain was updated.
        /// </summary>
        /// <param name="args"></param>
        public void SelectionUpdateTerrain(TerrainSelectionUpdateArgs args)
        {
            _lastTerrainLocation = args;

            foreach (var controller in _currentStateControllers)
            {
                controller.TerrainSelectionUpdate(args);
            }
        }

        /// <summary>
        /// Set the current state of the campus editor.
        /// </summary>
        /// <param name="next">The next state for the game to transition to.</param>
        /// <param name="context">Optional context object to pass to controllers upon transition.</param>
        private void Transition(GameState next, object context = null)
        {
            lock (_setLock)
            {
                if (Current == next)
                    return;

                GameState old = Current;
                Current = next;

                foreach (var controller in _currentStateControllers)
                {
                    controller.TransitionOut();
                }

                if (!_stateControllers.TryGetValue(next, out _currentStateControllers))
                {
                    _currentStateControllers = EmptyControllers;
                }

                foreach (var controller in _currentStateControllers)
                {
                    controller.TransitionIn(context);

                    // Primer events that the new state needs.
                    if (_lastTerrainLocation != null)
                    {
                        controller.TerrainSelectionUpdate(_lastTerrainLocation);
                    }
                }
            }
        }
    }
}
