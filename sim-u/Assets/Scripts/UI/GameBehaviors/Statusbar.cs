using System;
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
        public string Test { get; set; }

        /// <summary>
        /// The funds string
        /// </summary>
        public Text FundsText;

        /// <summary>
        /// The Date string
        /// </summary>
        public Text DateText;

        /// <summary>
        /// A view on the current funds.
        /// </summary>
        public int CurrentFunds {
            get { return _currentFunds; }
            set
            {
                _currentFunds = value;
                FundsText.text = string.Format(CultureInfo.CurrentCulture, "{0:C0}", _currentFunds);
            }
        }

        /// <summary>
        /// A view on the current date.
        /// </summary>
        public string CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                DateText.text = _currentDate;
            }
        }

        private int _currentFunds;
        private string _currentDate;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {
            if (FundsText == null)
                throw new ArgumentNullException("FundsText");

            if (DateText == null)
                throw new ArgumentNullException("DateText");
        }
    }
}
