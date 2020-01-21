using Common;
using Faculty;
using GameData;
using System;
using System.Collections.Generic;
using System.Threading;
using UI;

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
        private StudentHistogramGenerator _generator;
        private UniversityScore _score;
        private UniversityVariables _variables;

        private SimulationData _config;

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

        public UniversityScore Score
        {
            get
            {
                // TODO: this should be a readonly copy
                return _score;
            }
        }

        public UniversityVariables Variables
        {
            get
            {
                // TODO: this should be a readonly copy
                return _variables;
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
        /// Check if the university can afford this purchase.
        /// </summary>
        public bool CanPurchase(int cost)
        {
            return cost <= 0 || // can always purchase free things!
                _score.Money >= cost ||
                Accessor.Game.AdminMode; // admin mode allows us to go into mega debt
        }

        /// <summary>
        /// Lower the university money score by the cost.
        /// Pops up the floating money effect by the mouse to indicate money spent.
        /// </summary>
        /// <param name="cost">The number in dollars to purchase.</param>
        /// <param name="required">If the purchase is required, then it will happen even
        /// if the university does not have enough dollars to pay for it.</param>
        public bool Purchase(int cost, bool required = false)
        {
            if (CanPurchase(cost) ||
                required)
            {
                _score.Money -= cost;
                TriggerUpdates(UpdateType.Tick);

                FloatingMoneyManager.Spawn(cost);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Update money without checking if it is a valid update.
        /// </summary>
        public void UpdateMoney(int deltaMoney)
        {
            _score.Money += deltaMoney;
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

            // Don't forget to get the freshmen's money
            int tuition = _variables.TuitionPerQuarter * students.TotalStudentCount;
            UpdateMoney(tuition);

            // Update the teacher's assignments
            Accessor.Faculty.AssignFacultyToStudents(_studentBody);
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
        /// Generates a hypothetical student population with the given variables.
        /// </summary>
        /// <param name="studentCount">Number of students to return. Takes Top N.</param>
        /// <param name="medianAcademicScore">Average grade of the population.</param>
        /// <param name="populationSize">Number of students to generate.</param>
        /// <returns>The generated student population represented as a histogram.</returns>
        public StudentHistogram GenerateStudentPopulation(int studentCount, int medianAcademicScore, int populationSize)
        {
            return _generator.GenerateStudentPopulation(studentCount, medianAcademicScore, populationSize);
        }

        /// <summary>
        /// Generates an empty student histogram.
        /// </summary>
        /// <returns>The empty student population represented as a histogram.</returns>
        public StudentHistogram GenerateStudentPopulation()
        {
            return _generator.GenerateEmpty();
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
                Score = _score,
                Variables = _variables,
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
            _config = gameData;

            // Link the Academic Year calculation and pop-up into the Simulation Manager
            RegisterSimulationUpdateCallback(nameof(AcademicYearWrapUp),
                AcademicYearWrapUp,
                UpdateType.AcademicYearly);

            // Link the Accounting calculations that happen every quarter / every week
            RegisterSimulationUpdateCallback(nameof(QuarterlyAccounting),
               QuarterlyAccounting,
               UpdateType.Quarterly);

            RegisterSimulationUpdateCallback(nameof(WeeklyAccounting),
                WeeklyAccounting,
                UpdateType.Weekly);

            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            SimulationSaveState savedGame = gameData.SavedData?.Simulation;
            LoadGameState(savedGame);
        }

        /// <summary>
        /// Update the simulation for once a year events.
        /// </summary>
        private void AcademicYearWrapUp()
        {
            GraduationResults graduationResult = _studentBody.GraduateStudents(Date);
            Accessor.UiManager.OpenAlertWindow(nameof(UI.AcademicYearPopUp), graduationResult);
        }

        /// <summary>
        /// Update the simulation for once a quarter events.
        /// </summary>
        private void QuarterlyAccounting()
        {
            // Tuition Money
            int tuition = _variables.TuitionPerQuarter * _studentBody.TotalStudentCount;

            GameLogger.Debug("[Quarterly {0}] Tuition: ${1:n0}", Date, tuition);
            UpdateMoney(tuition);
        }

        /// <summary>
        /// Update the simulation for once a week events.
        /// </summary>
        private void WeeklyAccounting()
        {
            // Step 1: Update scores
            // NB: Drop outs all count as StudentAcademicScore.MinValue - 10
            int currentStudentBodyMean = _studentBody.GetCurrentAcademicPrestige(
                _config.AcademicPrestigeLookBackYears,
                _config.AcademicPrestigeLookBackStudentCount,
                _config.StudentAcademicScore.DefaultValue,
                _config.StudentAcademicScore.MinValue - 10);

            int newAcademicPrestige = (int)Math.Round(
                SimulationUtils.LinearMapping(
                    value: currentStudentBodyMean,
                    minInput: _config.StudentAcademicScore.MinValue,
                    maxInput: _config.StudentAcademicScore.MaxValue,
                    minOutput: _config.AcademicPrestige.MinValue,
                    maxOutput: _config.AcademicPrestige.MaxValue));

            newAcademicPrestige = Utils.Clamp(
                newAcademicPrestige,
                _config.AcademicPrestige.MinValue,
                _config.AcademicPrestige.MaxValue);

            _score.AcademicPrestige = newAcademicPrestige;

            // Step 2: Update student education
            StudentHistogram[] educationDelta = Accessor.Faculty.ExecuteTeachingStep();
            _studentBody.UpdateStudents(educationDelta);

            // Step 3: Pay employees
            int paymentsDue = 0;
            foreach (var faculty in Accessor.Faculty.HiredFaculty)
            {
                // These poor teachers are getting underpaid. Do you think anyone will notice?
                int truncatedWeeklySalary =
                    faculty.SalaryPerYear / (SimulationDate.WeeksPerQuarter * SimulationDate.QuartersPerYear);

                paymentsDue += truncatedWeeklySalary;
            }

            GameLogger.Debug("[Weekly {0}] AP = {1}; Faculty Salary ${2:n0}", Date, newAcademicPrestige, paymentsDue);
            UpdateMoney(-paymentsDue);
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
