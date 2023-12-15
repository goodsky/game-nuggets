using Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// UI Element to order and scroll a group of buttons.
    /// Contains stylistic flairs for my menu design.
    /// </summary>
    public class ButtonGroup : MonoBehaviour
    {
        /// <summary>
        /// The left scroll button.
        /// </summary>
        public GameObject ScrollButtonLeft;

        /// <summary>
        /// The right scroll button.
        /// </summary>
        public GameObject ScrollButtonRight;

        /// <summary>
        /// The scrolling content (should contain buttons).
        /// </summary>
        public GameObject Content;

        /// <summary>
        /// The buttons in this button group.
        /// </summary>
        public List<Button> Buttons { get; set; }

        /// <summary>
        /// The speed to scroll.
        /// </summary>
        public float ScrollSpeed = 3.0f;

        private bool _mouseOver;
        private bool _scrollEnabled;
        private RectTransform _buttonGroupRect;
        private RectTransform _contentRect;
        private EventTrigger _eventTrigger;

        private float _buttonGroupWidth = -1;
        private float _contentWidth = -1;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {
            ScrollButtonLeft.GetComponent<Button>().WhileMouseDown = ScrollLeft;
            ScrollButtonRight.GetComponent<Button>().WhileMouseDown = ScrollRight;

            _buttonGroupRect = GetComponent<RectTransform>();
            _contentRect = Content.GetComponent<RectTransform>();

            // Event Trigger used to track mouse in and mouse out
            _eventTrigger = gameObject.AddComponent<EventTrigger>();

            {
                var pointerEnter = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
                pointerEnter.callback.AddListener(_ => { _mouseOver = true; });
                _eventTrigger.triggers.Add(pointerEnter);
            }

            {
                var pointerExit = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener(_ => { _mouseOver = false; });
                _eventTrigger.triggers.Add(pointerExit);
            }

            CheckContentSizeAndFormat();
        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            CheckContentSizeAndFormat();

            if (_mouseOver)
            {
                var wheelScroll = Input.GetAxis("Mouse ScrollWheel");

                if (wheelScroll < -1e-3)
                {
                    ScrollRight(ScrollSpeed * 4.0f);
                }

                if (wheelScroll > 1e-3)
                {
                    ScrollLeft(ScrollSpeed * 4.0f);
                }
            }
        }

        /// <summary>
        /// Scroll the buttons to the right.
        /// </summary>
        /// <param name="speed">Speed to scroll right.</param>
        private void ScrollRight(float speed)
        {
            if (!_scrollEnabled)
                return;

            float xPos = _contentRect.anchoredPosition.x;
            float rightEdge = xPos + _contentRect.rect.width;

            if (rightEdge > _buttonGroupWidth)
            {
                _contentRect.anchoredPosition = new Vector2(xPos - speed, _contentRect.anchoredPosition.y);

                // When you start scrolling, set the focus to the parent of the scroll bar buttons.
                // This will be the button that popped up the scroll bar.
                var button = Content.GetComponentInChildren<Button>();
                if (button != null && SelectionManager.Selected != button.SelectionParent)
                {
                    SelectionManager.UpdateSelection(button.SelectionParent);
                }
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(_buttonGroupWidth - _contentRect.rect.width, _contentRect.anchoredPosition.y);
            }
        }

        /// <summary>
        /// Scroll the buttons to the right at the default speed.
        /// </summary>
        private void ScrollRight()
        {
            ScrollRight(ScrollSpeed);
        }

        /// <summary>
        /// Scroll the buttons to the left.
        /// </summary>
        /// <param name="speed">Speed to scroll left.</param>
        private void ScrollLeft(float speed)
        {
            if (!_scrollEnabled)
                return;

            float xPos = _contentRect.anchoredPosition.x;

            if (xPos < 0.0f)
            {
                _contentRect.anchoredPosition = new Vector2(xPos + speed, _contentRect.anchoredPosition.y);

                // When you start scrolling, set the focus to the parent of the scroll bar buttons.
                // This will be the button that popped up the scroll bar.
                var button = Content.GetComponentInChildren<Button>();
                if (button != null && SelectionManager.Selected != button.SelectionParent)
                {
                    SelectionManager.UpdateSelection(button.SelectionParent);
                }
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(0.0f, _contentRect.anchoredPosition.y);
            }
        }

        /// <summary>
        /// Scroll left at the default speed.
        /// </summary>
        private void ScrollLeft()
        {
            ScrollLeft(ScrollSpeed);
        }

        private void CheckContentSizeAndFormat()
        {
            if (_buttonGroupWidth != _buttonGroupRect.rect.width ||
                _contentWidth != -_contentRect.rect.width)
            {
                _buttonGroupWidth = _buttonGroupRect.rect.width;
                _contentWidth = _contentRect.rect.width;

                float padding = _buttonGroupWidth - _contentWidth;

                if (padding < 0.0)
                {
                    _scrollEnabled = true;
                    this.ScrollButtonLeft.SetActive(true);
                    this.ScrollButtonRight.SetActive(true);
                }
                else
                {
                    _scrollEnabled = false;
                    this.ScrollButtonLeft.SetActive(false);
                    this.ScrollButtonRight.SetActive(false);

                    _contentRect.anchoredPosition = new Vector2(padding / 2.0f, _contentRect.anchoredPosition.y);
                }
            }
        }
    }
}
