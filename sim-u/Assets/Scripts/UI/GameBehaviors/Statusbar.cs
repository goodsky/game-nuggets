using Common;
using Simulation;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Behaviour for the status bar at the top of the screen.
    /// </summary>
    public class Statusbar : MonoBehaviour
    {
        private readonly GameAccessor _accessor = new GameAccessor();
        private SimulationManager _simulation;

        private int _currentFunds;
        private SimulationDate _currentDate;

        public Text MoneyText;

        public Text DateText;

        public Button PauseButton;
        public Image PauseActiveImage;

        public Button PlayNormalButton;
        public Image PlayNormalActiveImage;

        public Button PlayFastButton;
        public Image PlayFastActiveImage;

        public Button[] Buttons => new[] { PauseButton, PlayNormalButton, PlayFastButton };

        /// <summary>
        /// A view on the current funds.
        /// </summary>
        public int CurrentFunds {
            get { return _currentFunds; }
            set
            {
                _currentFunds = value;
                MoneyText.text = string.Format(CultureInfo.CurrentCulture, "{0:C0}", _currentFunds);
            }
        }

        /// <summary>
        /// A view on the current date.
        /// </summary>
        public SimulationDate CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;

                string weekString = _currentDate.Week == SimulationDate.WeeksPerQuarter ?
                    "Finals" : $"Week {_currentDate.Week}";

                DateText.text = $"Year {_currentDate.Year} / {_currentDate.Quarter.ToString()} / {weekString}";
            }
        }

        /// <summary>
        /// The Unity start method
        /// </summary>
        protected void Start()
        {
            _simulation = _accessor.Simulation;

            PauseButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Paused);
            };

            PlayNormalButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Normal);
            };

            PlayFastButton.OnMouseDown = e =>
            {
                _simulation.SetSimulationSpeed(SimulationSpeed.Fast);
            };
        }

        /// <summary>
        /// This method is invoked whenever it is time to update the display with new simulation data.
        /// I could use Unit's Update directly... but it would involve pointless updates. So I did this. Is that okay?
        /// </summary>
        public void SimulationUpdateCallback()
        {
            CurrentDate = _simulation.Date;

            // Set the indicator of which speed the simulation is running at
            PauseActiveImage.enabled = _simulation.Speed == SimulationSpeed.Paused;
            PlayNormalActiveImage.enabled = _simulation.Speed == SimulationSpeed.Normal;
            PlayFastActiveImage.enabled = _simulation.Speed == SimulationSpeed.Fast;
        }
    }
}
