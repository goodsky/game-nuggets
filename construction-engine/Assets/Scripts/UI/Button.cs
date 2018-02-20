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
        public Sprite DefaultImage;
        
        /// <summary>The mouse over background image.</summary>
        public Sprite MouseOverImage;

        /// <summary>The selected/mouse down background image.</summary>
        public Sprite SelectedImage;

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

        protected override void Start()
        {
            base.Start();

            _image = gameObject.AddComponent<Image>();

            PostEvent();
        }

        protected void Update()
        {
            if (IsMouseOver && !string.IsNullOrEmpty(Tooltip))
            {
                _tooltipCount += Time.deltaTime;

                if (_tooltipCount > TooltipDelay)
                {
                    TooltipManager.PopUp(Tooltip);
                }
            }

            if (IsMouseDown && OnMouseDown != null)
            {
                OnMouseDown();
            }
        }

        protected override void InternalSelect()
        {
            base.InternalSelect();

            if (OnSelect != null)
            {
                OnSelect();
            }
        }

        protected override void InternalDeselect()
        {
            if (OnDeselect != null)
            {
                OnDeselect();
            }

            base.InternalDeselect();
        }

        public override void MouseOver(BaseEventData eventData)
        {
            _tooltipCount = 0.0f;

            base.MouseOver(eventData);
        }

        public override void MouseOut(BaseEventData eventData)
        {
            TooltipManager.PopDown();

            base.MouseOut(eventData);
        }

        public override void PostEvent()
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
                        _image.sprite = SelectedImage;
                    }
                }
                else if (IsMouseOver)
                {
                    if (_image != null)
                    {
                        _image.sprite = MouseOverImage;
                    }
                }
                else
                {
                    if (_image != null)
                    {
                        _image.sprite = DefaultImage;
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
