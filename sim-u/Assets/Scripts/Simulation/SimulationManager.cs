using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Simulation
{
    /// <summary>
    /// Represents possible simulation speed modes.
    /// </summary>
    public enum SimulationSpeed
    {
        Paused,
        Normal,
        Fast,
    }

    /// <summary>
    /// Unity GameObject that manages Simulation State.
    /// </summary>
    public class SimulationManager : GameDataLoader<SimulationData>, IGameStateSaver<SimulationSaveState>
    {
        private float _tickRateInSeconds;
        private int _normalTicksPerWeek;
        private int _fastTicksPerWeek;

        private volatile int _ticksCounter = 0;
        private volatile int _ticksInProgress = 0;

        private Dictionary<string, Action> _updateActions = new Dictionary<string, Action>();

        /// <summary>
        /// Gets the current speed of the simulation.
        /// </summary>
        public SimulationSpeed Speed { get; private set; }

        /// <summary>
        /// Gets the current year/quarter/week time in the simulation.
        /// </summary>
        public SimulationDate Date { get; private set; }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            // NOTE: Start is invoked after LoadData is called.
            InvokeRepeating(nameof(SimulationTick), 0f, _tickRateInSeconds);

            base.Start();
        }

        /// <summary>
        /// Register an action to be run each simulation tick.
        /// Be careful with this! If you put something expensive here it'll get nasty fast.
        /// </summary>
        /// <param name="name">Name of the action. For accounting.</param>
        /// <param name="action">The action to run.</param>
        public void RegisterSimulationUpdateCallback(string name, Action action)
        {
            GameLogger.Info("Registering action '{0}' to run each simulation tick...", name);
            _updateActions.Add(name, action);
        }

        /// <summary>
        /// Sets the speed of the game simualation.
        /// </summary>
        /// <param name="speed">The simulation speed.</param>
        public void SetSimulationSpeed(SimulationSpeed speed)
        {
            GameLogger.Info("SimulationSpeed set to '{0}'", speed.ToString());
            Speed = speed;

            TriggerSimulationUpdates();
        }

        public SimulationSaveState SaveGameState()
        {
            return new SimulationSaveState()
            {
                SavedSpeed = Speed,
                SavedDate = Date,
            };
        }

        public void LoadGameState(SimulationSaveState state)
        {
            if (state != null)
            {
                SetSimulationSpeed(state.SavedSpeed);
                Date = state.SavedDate;
            }
            else
            {
                GameLogger.Warning("No simulation state was loaded. Setting to default.");
                SetSimulationSpeed(SimulationSpeed.Normal);
                Date = new SimulationDate(year: 1, quarter: SimulationQuarter.Fall, week: 1);
            }
        }

        protected override void LoadData(SimulationData gameData)
        {
            _tickRateInSeconds = gameData.TickRateInSeconds;
            _normalTicksPerWeek = gameData.NormalTicksPerWeek;
            _fastTicksPerWeek = gameData.FastSpeedTicksPerWeek;

            GameLogger.Info("Simulation started with parameters: TickRate={0}; NormalSpeed={1}; FastSpeed={2};",
                _tickRateInSeconds,
                _normalTicksPerWeek,
                _fastTicksPerWeek);

            if (_tickRateInSeconds < 0.1f || _tickRateInSeconds > 10f)
            {
                GameLogger.FatalError("Tick rate is out of valid bounds! Value = '{0}'", _tickRateInSeconds);
            }

            if (_normalTicksPerWeek < 0 || _normalTicksPerWeek > 1000)
            {
                GameLogger.FatalError("Normal Ticks per week is out of valid bounds! Value = '{0}'", _normalTicksPerWeek);
            }

            if (_fastTicksPerWeek < 0 || _fastTicksPerWeek > 1000)
            {
                GameLogger.FatalError("Fast Speed Ticks per week is out of valid bounds! Value = '{0}'", _fastTicksPerWeek);
            }
        }

        protected override void LinkData(SimulationData gameData)
        {
            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            SimulationSaveState savedGame = gameData.SavedData?.Simulation;
            LoadGameState(savedGame);
        }

        /// <summary>
        /// Heartbeat of the simulation.
        /// </summary>
        private void SimulationTick()
        {
            if (Interlocked.Exchange(ref _ticksInProgress, 1) != 0)
            {
                GameLogger.Error("Overlapping simulation ticks! Aborting.");
                return;
            }

            try
            {
                ++_ticksCounter;
                if ((Speed == SimulationSpeed.Normal &&
                    _ticksCounter >= _normalTicksPerWeek) ||
                    (Speed == SimulationSpeed.Fast &&
                    _ticksCounter >= _fastTicksPerWeek))
                {
                    _ticksCounter = 0;
                    Date = Date.NextWeek();
                }

                TriggerSimulationUpdates();
            }
            finally
            {
                _ticksInProgress = 0;
            }
        }

        private void TriggerSimulationUpdates()
        {
            foreach (Action tickAction in _updateActions.Values)
            {
                tickAction();
            }
        }
    }
}
