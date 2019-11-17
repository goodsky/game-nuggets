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
    /// Simulation trigger levels.
    /// </summary>
    public enum UpdateType
    {
        Tick = 0,
        Weekly = 1,
        Quarterly = 2,
        AcademicYearly = 3,
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

        private SimulationSpeed _speed;
        private bool _isFrozen;

        private Dictionary<string, (UpdateType type, Action action)> _updateActions = new Dictionary<string, (UpdateType type, Action action)>();

        private StudentBody _studentBody;
        private UniversityScore _score;
        private UniversityVariables _variables;
        private StudentHistogramGenerator _generator;

        /// <summary>
        /// Gets the current speed of the simulation.
        /// Freezing the simulation overrides this value to 'Paused'.
        /// </summary>
        public SimulationSpeed Speed {
            get
            {
                return _isFrozen ?
                    SimulationSpeed.Paused :
                    _speed;
            }
        }

        /// <summary>
        /// Gets the current year/quarter/week time in the simulation.
        /// </summary>
        public SimulationDate Date { get; private set; }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            InvokeRepeating(nameof(SimulationTick), 0f, _tickRateInSeconds);

            base.Start();
        }

        /// <summary>
        /// Register an action to be run each simulation tick.
        /// Be careful with this! If you put something expensive here it'll get nasty fast.
        /// </summary>
        /// <param name="name">Name of the action. For accounting.</param>
        /// <param name="action">The action to run.</param>
        /// <param name="updateType">How often the callback should be invoked in the simulation.</param>
        public void RegisterSimulationUpdateCallback(string name, Action action, UpdateType updateType)
        {
            GameLogger.Info("Registering action '{0}' to run {1}...", name, updateType.ToString());
            _updateActions.Add(name, (updateType, action));
        }

        /// <summary>
        /// Enrolls a new class of students to the university.
        /// </summary>
        /// <param name="students">The students to enroll.</param>
        public void EnrollStudents(StudentHistogram students)
        {
            _studentBody.EnrollClass(students);
        }

        /// <summary>
        /// Gets the current student body enrolled in your university.
        /// </summary>
        /// <returns>he student body.</returns>
        public StudentBody CurrentStudentBody()
        {
            return _studentBody;
        }

        /// <summary>
        /// Calculate the possible tuition range for the university at its current score.
        /// </summary>
        /// <returns>The tuition range.</returns>
        public (int minTuition, int maxTuition) GenerateTuitionRange()
        {
            return (_generator.CalculateTargetTuition(_score, bonus: -10),
                    _generator.CalculateTargetTuition(_score, bonus: 10));
        }

        /// <summary>
        /// Generates a hypothetical student population with the given variables.
        /// </summary>
        /// <param name="tuition">The requested tuition value for the college.</param>
        /// <returns>The generated student population represented as a histogram.</returns>
        public StudentHistogram GenerateStudentPopulation(int tuition)
        {
            return _generator.GenerateStudentPopulation(tuition, _score);
        }

        /// <summary>
        /// Map an academic score to an SAT score. Used for displaying academic score (sometimes).
        /// </summary>
        /// <param name="academicScore">A student's academic score.</param>
        /// <returns>A student's SAT score.</returns>
        public int ConvertAcademicScoreToSATScore(int academicScore)
        {
            return _generator.ConvertAcademicScoreToSATScore(academicScore);
        }

        /// <summary>
        /// Map a SAT score to an academic score. Used for filtering SAT scores.
        /// </summary>
        /// <param name="satScore">A student's SAT score.</param>
        /// <returns>A student's academic score.</returns>
        public int ConvertSATScoreToAcademicScore(int satScore)
        {
            return _generator.ConvertSATScoreToAcademicScore(satScore);
        }

        /// <summary>
        /// Sets the speed of the game simualation.
        /// </summary>
        /// <param name="speed">The simulation speed.</param>
        public void SetSimulationSpeed(SimulationSpeed speed)
        {
            GameLogger.Info("SimulationSpeed set to '{0}'", speed.ToString());
            _speed = speed;

            TriggerUpdates(UpdateType.Tick);
        }

        /// <summary>
        /// Allows a super-pop-up to freeze the entire simulation.
        /// Must be unfrozen before any normal simulation will continue.
        /// </summary>
        /// <param name="freeze">True to freeze the simulation. False to unfreeze.</param>
        public void SetSimulationFreeze(bool freeze)
        {
            GameLogger.Info("Simulation freeze state: {0}", freeze);
            _isFrozen = freeze;
        }

        public SimulationSaveState SaveGameState()
        {
            return new SimulationSaveState()
            {
                SavedSpeed = _speed,
                SavedIsFrozen = _isFrozen,
                SavedDate = Date,
                StudentBody = _studentBody.SaveGameState(),
            };
        }

        public void LoadGameState(SimulationSaveState state)
        {
            if (state != null)
            {
                SetSimulationSpeed(state.SavedSpeed);
                Date = state.SavedDate;

                _score = state.Score;
                _variables = state.Variables;
                _studentBody.LoadGameState(state.StudentBody);
            }
            else
            {
                GameLogger.Warning("No simulation state was loaded. Setting to default.");
                SetSimulationSpeed(SimulationSpeed.Normal);
                Date = new SimulationDate(year: 1, quarter: SimulationQuarter.Summer, week: 13);
            }
        }

        protected override void LoadData(SimulationData gameData)
        {
            _score = new UniversityScore(gameData);
            _variables = new UniversityVariables();
            _generator = new StudentHistogramGenerator(gameData);
            _studentBody = new StudentBody(gameData, _generator);
            

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
            // Link the Academic Year calculation and pop-up into the Simulation Manager
            RegisterSimulationUpdateCallback(nameof(AcademicYearWrapUp),
                AcademicYearWrapUp,
                UpdateType.AcademicYearly);


            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            SimulationSaveState savedGame = gameData.SavedData?.Simulation;
            LoadGameState(savedGame);
        }

        private void AcademicYearWrapUp()
        {
            GraduationResults graduationResult = _studentBody.GraduateStudents();
            Accessor.UiManager.OpenWindow(nameof(UI.AcademicYearPopUp), graduationResult);
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
                SimulationDate oldDate = Date;

                ++_ticksCounter;
                if ((Speed == SimulationSpeed.Normal &&
                    _ticksCounter >= _normalTicksPerWeek) ||
                    (Speed == SimulationSpeed.Fast &&
                    _ticksCounter >= _fastTicksPerWeek))
                {
                    _ticksCounter = 0;
                    Date = Date.NextWeek();
                }

                if (oldDate.Year != Date.Year)
                {
                    TriggerUpdates(UpdateType.AcademicYearly);
                }
                else if (oldDate.Quarter != Date.Quarter)
                {
                    TriggerUpdates(UpdateType.Quarterly);
                }
                else if (oldDate.Week != Date.Week)
                {
                    TriggerUpdates(UpdateType.Weekly);
                }
                else
                {
                    TriggerUpdates(UpdateType.Tick);
                }
            }
            finally
            {
                _ticksInProgress = 0;
            }
        }

        private void TriggerUpdates(UpdateType updateType)
        {
            foreach (var actionKvp in _updateActions)
            {
                string actionName = actionKvp.Key;
                (UpdateType actionType, Action action) = actionKvp.Value;

                if (actionType <= updateType)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        GameLogger.Error("Error during simulation update '{0}'. Ex = {1}", actionName, ex);
                    }
                }
            }
        }
    }
}
