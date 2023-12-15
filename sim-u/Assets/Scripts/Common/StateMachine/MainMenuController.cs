using System;
using UnityEngine.SceneManagement;

namespace Common
{
    /// <summary>
    /// This controller exits the game and backs out to the main menu.
    /// </summary>
    [StateController(HandledState = GameState.MainMenu)]
    public class MainMenuController : GameStateMachine.Controller
    {
        public override void TransitionIn(object context)
        {
            GameLogger.Info("Exiting to main menu!");
            SceneManager.LoadScene(Constant.MenuSceneName);
        }

        public override void TransitionOut()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
