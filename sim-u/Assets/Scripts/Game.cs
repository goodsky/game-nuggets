using Campus;
using Common;
using Faculty;
using GameData;
using Simulation;
using System;
using System.IO;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    private static readonly int GameWidth = 1600;
    private static readonly int GameHeight = 900;

    private static readonly string ConfigFolderName = "GameData";
    private static readonly string ConfigFileExtension = ".xml";

    [Header("Game Configuration")]
    public string UIConfig;
    public string CampusConfig;
    public string FacultyConfig;
    public string SimulationConfig;

    [Space(10)]
    [Header("Debugging Flags")]
    public bool VisualizeConnections;

    [Space(10)]
    [Header("Admin Overrides")]
    public bool AdminMode;
    public OverrideInt OverrideMoney;
    public OverrideSimulationDate OverrideDate;
    public OverrideInt OverrideAcademicPrestige;
    public OverrideInt OverrideResearchPrestige;
    public OverrideInt OverridePopularity;
    public OverrideInt OverrideAcademicScore;
    public OverrideInt OverrideResearchScore;
    public OverrideStudentBody[] OverrideStudents;

    [Header("If no save game is specified use this")]
    public string DefaultSaveGame;

    private static object _singletonLock = new object();
    private static Game _singleton = null;

    /// <summary>
    /// This static metadata will survive scene transitions.
    /// Set it before loading the game scene to request a particular save file to be loaded.
    /// </summary>
    private static GameSaveStateMetadata _saveStateMetadata { get; set; } = null;
    private GameSaveState _saveState = null;

    /// <summary>
    /// Sets the metadata for the save game that will be used to populate the game
    /// when the main scene is loaded. Only takes effect if set before the 
    /// <see cref="InitGameObjects"/> call.
    /// </summary>
    /// <param name="path">The save game path on disk (within the StreamingAssets/ directory)</param>
    public static void SetGameSaveStateForReload(string path)
    {
        _saveStateMetadata = new GameSaveStateMetadata(path);
    }

    /// <summary>
    /// Save the game state.
    /// </summary>
    /// <returns>The saved game state.</returns>
    public GameSaveState SaveGame()
    {
        var accessor = new GameAccessor();
        return new GameSaveState
        {
            Version = GameSaveState.CurrentVersion,
            Campus = accessor.CampusManager.SaveGameState(),
            Faculty = accessor.Faculty.SaveGameState(),
            Simulation = accessor.Simulation.SaveGameState(),
        };
    }

    /// <summary>
    /// Load the saved game if one exists.
    /// Uses <see cref="GameSaveStateMetadata"/> to select the game file.
    /// </summary>
    /// <param name="saveState">The saved game state if it could be loaded.</param>
    /// <returns>True if the saved game exists and was loaded. False otherwise.</returns>
    public bool TryLoadSavedGame(out GameSaveState saveState)
    {
        if (_saveState != null)
        {
            saveState = _saveState;
            return true;
        }

        GameSaveStateMetadata info = Game._saveStateMetadata;
        if (info != null)
        {
            if (SavedGameLoader.TryReadFromDisk(info.Path, out saveState))
            {
                _saveState = saveState;
                return true;
            }
        }

        saveState = null;
        return false;
    }

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        Screen.SetResolution(GameWidth, GameHeight, fullscreen: false);

        GameLogger.EnsureSingletonExists();
        GameLogger.Info("Game started.");

        lock (_singletonLock)
        {
            if (_singleton != null)
            {
                GameLogger.FatalError("It appears there are multiple root Game objects.");
            }

            _singleton = this;
        }

        if (_saveStateMetadata == null && !string.IsNullOrEmpty(DefaultSaveGame))
        {
            GameLogger.Info("Using default save game '{0}'", DefaultSaveGame);
            SetGameSaveStateForReload(DefaultSaveGame);
        }

        GameLogger.Info("Creating game objects.");
        InitGameObjects();
    }

    /// <summary>
    /// Cleanup game state and data.
    /// </summary>
    protected void OnDestroy()
    {
        GameLogger.Info("Game exiting.");

        lock (_singletonLock)
        {
            _singleton = null;
        }
    }

    /// <summary>
    /// Initialize the game objects.
    /// </summary>
    private void InitGameObjects()
    {
        UIFactory.LoadEventSystem(gameObject);

        gameObject.AddComponent<GameDataStore>();
        gameObject.AddComponent<GameStateMachine>();

        GameObject ui = UIFactory.LoadUICanvas(gameObject);
        GameDataLoader<UIData>.SetGameData<UIManager>(ui, GetConfigPath(UIConfig));

        GameObject campus = UIFactory.GenerateEmpty("Campus", transform);
        GameDataLoader<CampusData>.SetGameData<CampusManager>(campus, GetConfigPath(CampusConfig), OverrideCampusValues);

        GameObject faculty = UIFactory.GenerateEmpty("Faculty", transform);
        GameDataLoader<FacultyData>.SetGameData<FacultyManager>(faculty, GetConfigPath(FacultyConfig), OverrideFacultyValues);

        GameObject simulation = UIFactory.GenerateEmpty("Simulation", transform);
        GameDataLoader<SimulationData>.SetGameData<SimulationManager>(simulation, GetConfigPath(SimulationConfig), OverrideSimulationValues);

        TooltipManager.Initialize(ui.gameObject.transform);
        FloatingMoneyManager.Initialize(ui.gameObject.transform);
    }

    private void OverrideCampusValues(CampusData data)
    {
        if (AdminMode)
        {
        }
    }

    private void OverrideFacultyValues(FacultyData data)
    {
        if (AdminMode)
        {
        }
    }

    private void OverrideSimulationValues(SimulationData data)
    {
        if (AdminMode)
        {
            SimulationManager simulation = new GameAccessor().Simulation;
            SimulationSaveState saveData = data.SavedData?.Simulation;
            if (saveData == null)
            {
                GameLogger.FatalError("Tried to overwrite null save data!");
            }

            if (OverrideMoney.Override)
            {
                GameLogger.Info("[Override] Money = ${0:n0}", OverrideMoney.Value);
                saveData.Score.Money = OverrideMoney.Value;
            }

            if (OverrideAcademicPrestige.Override)
            {
                GameLogger.Info("[Override] Academic Prestige = {0}", OverrideAcademicPrestige.Value);
                saveData.Score.AcademicPrestige = OverrideAcademicPrestige.Value;
            }

            if (OverrideResearchPrestige.Override)
            {
                GameLogger.Info("[Override] Research Prestige = {0}", OverrideResearchPrestige.Value);
                saveData.Score.ResearchPrestige = OverrideResearchPrestige.Value;
            }

            if (OverridePopularity.Override)
            {
                GameLogger.Info("[Override] Popularity = ${0:n0}", OverridePopularity.Value);
                saveData.Score.Popularity = OverridePopularity.Value;
            }

            if (OverrideAcademicScore.Override)
            {
                GameLogger.Info("[Override] Academic Score = ${0:n0}", OverrideAcademicScore.Value);
                saveData.Score.AcademicScore = OverrideAcademicScore.Value;
            }

            if (OverrideResearchScore.Override)
            {
                GameLogger.Info("[Override] Research Score = ${0:n0}", OverrideResearchScore.Value);
                saveData.Score.ResearchScore = OverrideResearchScore.Value;
            }

            if (OverrideDate.Override)
            {
                GameLogger.Info("[Override] Date = {0}", OverrideDate.Value);
                saveData.SavedDate = OverrideDate.Value;
            }

            for (int i = 0; i < OverrideStudents.Length; ++i)
            {
                if (OverrideStudents[i].Override)
                {
                    var students = simulation.GenerateStudentPopulation(
                        OverrideStudents[i].StudentCount,
                        OverrideStudents[i].MeanAcademicScore,
                        OverrideStudents[i].StudentCount);

                    GameLogger.Info("[Override] Student Year {0} = {2}", i, students);
                    saveData.StudentBody.ActiveStudents[i] = students;
                }
            }
        }
    }

    private string GetConfigPath(string configName) => Path.Combine(Application.streamingAssetsPath, ConfigFolderName, configName + ConfigFileExtension);

    [Serializable]
    public class OverrideInt
    {
        public bool Override;

        public int Value;
    }

    [Serializable]
    public class OverrideSimulationDate
    {
        public bool Override;

        [Range(0, 9999)]
        public int Year;

        public SimulationQuarter Quarter;

        [Range(1, SimulationDate.WeeksPerQuarter)]
        public int Week;

        public SimulationDate Value => new SimulationDate(Year, Quarter, Week);
    }

    [Serializable]
    public class OverrideStudentBody
    {
        public bool Override;

        public int StudentCount;

        [Range(60, 110)]
        public int MeanAcademicScore;
    }
}
