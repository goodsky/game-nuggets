using Campus.GridTerrain;
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

        /// <summary>Demolishing anchored features in the campus.</summary>
        Demolishing,

        /// <summary>Selecting campus terrain to modify.</summary>
        SelectingTerrain,

        /// <summary>Modifying the campus terrain.</summary>
        EditingTerrain,

        /// <summary>Selecting the start position of the path.</summary>
        SelectingPath,

        /// <summary>Creating the path.</summary>
        PlacingPath,

        /// <summary>Selecting the start position of the road.</summary>
        SelectingRoad,

        /// <summary>Creating the road.</summary>
        PlacingRoad,

        /// <summary>Constructing a new entity on the campus.</summary>
        PlacingConstruction,
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

        private TerrainGridUpdateArgs _lastTerrainGridSelection = null;
        private TerrainVertexUpdateArgs _lastTerrainVertexSelection = null;

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
            switch (newState)
            {
                case GameState.SelectingTerrain:
                case GameState.PlacingConstruction:
                case GameState.SelectingPath:
                case GameState.SelectingRoad:
                case GameState.Demolishing:
                    Transition(newState, context);
                    break;

                default:
                    GameLogger.FatalError("Cannot start doing state! {0}", newState.ToString());
                    break;
            }
        }

        /// <summary>
        /// Stop doing what you're doing.
        /// </summary>
        public void StopDoing()
        {
            switch (Current)
            {
                case GameState.EditingTerrain:
                    Transition(GameState.SelectingTerrain);
                    break;

                default:
                    Transition(GameState.Selecting);
                    break;
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
        /// The grid selection on the terrain was updated.
        /// </summary>
        /// <param name="args"></param>
        public void UpdateTerrainGridSelection(TerrainGridUpdateArgs args)
        {
            _lastTerrainGridSelection = args;

            foreach (var controller in _currentStateControllers)
            {
                controller.TerrainGridSelectionUpdate(args);
            }
        }

        /// <summary>
        /// The grid selection on the terrain was updated.
        /// </summary>
        /// <param name="args"></param>
        public void UpdateTerrainVertexSelection(TerrainVertexUpdateArgs args)
        {
            _lastTerrainVertexSelection = args;

            foreach (var controller in _currentStateControllers)
            {
                controller.TerrainVertexSelectionUpdate(args);
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

                    // Prime new states with the last relevant event arguments
                    if (_lastTerrainGridSelection != null)
                    {
                        controller.TerrainGridSelectionUpdate(_lastTerrainGridSelection);
                    }

                    if (_lastTerrainVertexSelection != null)
                    {
                        controller.TerrainVertexSelectionUpdate(_lastTerrainVertexSelection);
                    }
                }
            }
        }
    }
}
