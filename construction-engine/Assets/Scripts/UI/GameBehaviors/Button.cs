using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    using Selectable = Common.Selectable;

    /// <summary>
    /// Extends the custom selection framework to have events and images on a button.
    /// </summary>
    public class Button : Selectable
    {
        /// <summary>The default background image.</summary>
        public Color DefaultColor;
        
        /// <summary>The mouse over background image.</summary>
        public Color MouseOverColor;

        /// <summary>The selected/mouse down background image.</summary>
        public Color SelectedColor;

        /// <summary>(optional) Icon Image.</summary>
        public Image IconImage;

        private Image _image;

        /// <summary>
        /// Unity Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _image = GetComponent<Image>();
            if (_image == null)
            {
                _image = gameObject.AddComponent<Image>();
            }

            AfterEvent();
        }

        /// <summary>
        /// Update the button image state machine after each event.
        /// </summary>
        public override void AfterEvent()
        {
            if (IsEnabled)
            {
                if (IconImage != null)
                {
                    // Normal tint icon
                    IconImage.color = Color.white;
                }

                if (IsSelected || IsLeftMouseDown)
                {
                    if (_image != null)
                    {
                        _image.color = SelectedColor;
                    }
                }
                else if (IsMouseOver)
                {
                    if (_image != null)
                    {
                        _image.color = MouseOverColor;
                    }
                }
                else
                {
                    if (_image != null)
                    {
                        _image.color = DefaultColor;
                    }
                }
            }
            else
            {
                if (IconImage != null)
                {
                    // Grey-out icon when disabled
                    IconImage.color = Color.grey;
                }
            }
        }
    }
}
