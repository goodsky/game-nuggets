using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Companion behavior for the New Construction UI window.
    /// </summary>
    public class NewConstructionWindow : MonoBehaviour
    {
        /// <summary>
        /// The building title text.
        /// </summary>
        public Text TitleText;

        /// <summary>
        /// The building image.
        /// </summary>
        public Image ConstructionImage;

        /// <summary>
        /// The building description text.
        /// </summary>
        public Text DescriptionText;

        /// <summary>
        /// The build button.
        /// </summary>
        public Button BuildButton;

        /// <summary>
        /// A view on the construction window title text.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                TitleText.text = _title;
            }
        }

        /// <summary>
        /// A view on the construction window description text.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                DescriptionText.text = _description;
            }
        }

        private string _title;
        private string _description;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected virtual void Start()
        {
            if (TitleText == null)
                throw new ArgumentNullException("TitleText");

            if (ConstructionImage == null)
                throw new ArgumentNullException("ConstructionImage");

            if (DescriptionText == null)
                throw new ArgumentNullException("DescriptionText");

            if (BuildButton == null)
                throw new ArgumentNullException("BuildButton");

            BuildButton.OnSelect = () => { Debug.Log("Selected!"); };
        }
    }
}
