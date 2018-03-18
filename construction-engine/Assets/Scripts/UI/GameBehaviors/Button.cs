using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
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

        /// <summary>Action to invoke when the button is selected (one per click).</summary>
        public Action OnSelect;

        /// <summary>Action to invoke when the button is deselected.</summary>
        public Action OnDeselect;

        /// <summary>Action to invoke each step the mouse is down on it.</summary>
        public Action OnMouseDown;

        /// <summary>Tooltip message to display over this button when hovered over.</summary>
        public string Tooltip;

        /// <summary>Delay in seconds before the tooltip will pop up.</summary>
        public float TooltipDelay = 1.0f;

        private Image _image;
        private float _tooltipCount;

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
        /// Unity Update method.
        /// </summary>
        protected void Update()
        {
            // Tooltip Popup
            if (IsMouseOver && !string.IsNullOrEmpty(Tooltip))
            {
                _tooltipCount += Time.deltaTime;

                if (_tooltipCount > TooltipDelay)
                {
                    TooltipManager.PopUp(Tooltip);
                }
            }

            // Call the MouseDown event each step the mouse is down
            if (IsMouseDown && OnMouseDown != null)
            {
                OnMouseDown();
            }
        }

        /// <summary>
        /// Override the Select event
        /// </summary>
        protected override void InternalSelect()
        {
            base.InternalSelect();

            if (OnSelect != null)
            {
                OnSelect();
            }
        }

        /// <summary>
        /// Override the Deselect event
        /// </summary>
        protected override void InternalDeselect()
        {
            if (OnDeselect != null)
            {
                OnDeselect();
            }

            base.InternalDeselect();
        }

        /// <summary>
        /// Override the MouseOver event
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public override void MouseOver(BaseEventData eventData)
        {
            _tooltipCount = 0.0f;

            base.MouseOver(eventData);
        }

        /// <summary>
        /// Override the MouseOut event
        /// </summary>
        /// <param name="eventData">Event system data</param>
        public override void MouseOut(BaseEventData eventData)
        {
            TooltipManager.PopDown();

            base.MouseOut(eventData);
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

                if (IsSelected || IsMouseDown)
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
