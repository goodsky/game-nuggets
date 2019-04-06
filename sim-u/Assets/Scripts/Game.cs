using Campus;
using Common;
using UI;
using UnityEngine;

/// <summary>
/// Main entry point for the game state and data initialization.
/// </summary>
public class Game : MonoBehaviour
{
    [Header("UI Configuration")]
    public TextAsset UIConfig;

    [Header("Campus Configuration")]
    public TextAsset CampusConfig;

    /// <summary>The global game state machine</summary>
    public static GameStateMachine State { get; private set; }

    /// <summary>The User Interface Manager</summary>
    public static UIManager UI { get; private set; }

    /// <summary>The Campus Manager</summary>
    public static CampusManager Campus { get; private set; }

    private static object _singletonLock = new object();
    private static Game _singleton = null;

    /// <summary>
    /// Bootstrap the game state and data.
    /// </summary>
    protected void Awake()
    {
        InitLogging();
        GameLogger.Info("Game started.");

        lock (_singletonLock)
        {
            if (_singleton != null)
            {
                GameLogger.FatalError("It appears there are multiple root Game objects.");
            }

            _singleton = this;
        }

        GameLogger.Info("Creating game objects.");
        InitGameObjects();

        Test();
    }

    /// <summary>
    /// Cleanup game state and data.
    /// </summary>
    protected void OnDestroy()
    {
        GameLogger.Info("Game exiting.");
        GameLogger.Close();
    }

    /// <summary>
    /// Set up the game session loggers.
    /// </summary>
    private void InitLogging()
    {
        if (Application.isEditor)
        {
            GameLogger.CreateUnityLogger(LogLevel.Info);
        }

        GameLogger.CreateMyDocumentsStream("debug", LogLevel.Info);
    }

    /// <summary>
    /// Initialize the game objects.
    /// </summary>
    private void InitGameObjects()
    {
        UIFactory.LoadEventSystem(gameObject);

        State = gameObject.AddComponent<GameStateMachine>();

        var ui = UIFactory.LoadUICanvas(gameObject);

        ui.SetActive(false);
        UI = ui.AddComponent<UIManager>();
        UI.SetConfig(UIConfig);
        ui.SetActive(true);

        var campus = UIFactory.GenerateEmpty("Campus", transform);

        campus.SetActive(false);
        Campus = campus.AddComponent<CampusManager>();
        Campus.SetConfig(CampusConfig);
        campus.SetActive(true);

        TooltipManager.Initialize(ui.gameObject.transform);
    }

    /// <summary>
    /// For testing!
    /// </summary>
    private void Test()
    {
        //var ui = new UIData()
        //{
        //    ButtonGroups = new List<ButtonGroupData>()
        //    {
        //        new MainButtonGroupData()
        //        {
        //            Name = "Foo",
        //            Buttons = new List<ButtonData>()
        //            {
        //                new ButtonData()
        //                {
        //                    Name = "Bar",
        //                    OnSelect = new OpenWindowAction()
        //                    {
        //                        DataType = GameDataType.Building
        //                    }
        //                }
        //            }
        //        }
        //    }
        //};

        //GameLogger.Info(GameDataSerializer.Save(ui));
    }
}
