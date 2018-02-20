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
        /// The pip pointing towards the selected button.
        /// Just a style thing.
        /// </summary>
        public GameObject SelectionPip;

        /// <summary>
        /// The speed to scroll.
        /// </summary>
        public float ScrollSpeed = 2.5f;

        private bool _mouseOver;
        private float _maskWidth;
        private bool _scrollEnabled;
        private RectTransform _contentRect;
        private EventTrigger _eventTrigger;

        void Start()
        {
            // this.SelectionPip.SetActive(false);

            ScrollButtonLeft.GetComponent<Button>().OnMouseDown = ScrollLeft;
            ScrollButtonRight.GetComponent<Button>().OnMouseDown = ScrollRight;

            var buttonGroupRect = GetComponent<RectTransform>();
            _contentRect = Content.GetComponent<RectTransform>();

            _maskWidth = buttonGroupRect.rect.width;
            var padding = _maskWidth - _contentRect.rect.width;

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
        }

        void Update()
        {
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

        private void ScrollRight(float speed)
        {
            if (!_scrollEnabled)
                return;

            float xPos = _contentRect.anchoredPosition.x;
            float rightEdge = xPos + _contentRect.rect.width;

            if (rightEdge > _maskWidth)
            {
                _contentRect.anchoredPosition = new Vector2(xPos - speed, _contentRect.anchoredPosition.y);

                var button = Content.GetComponentInChildren<Button>();
                if (button != null && Selectable.SelectionManager.Selected != button.SelectionParent)
                {
                    Selectable.SelectionManager.UpdateSelection(button.SelectionParent);
                }
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(_maskWidth - _contentRect.rect.width, _contentRect.anchoredPosition.y);
            }
        }

        private void ScrollRight()
        {
            ScrollRight(ScrollSpeed);
        }

        private void ScrollLeft(float speed)
        {
            if (!_scrollEnabled)
                return;

            float xPos = _contentRect.anchoredPosition.x;

            if (xPos < 0.0f)
            {
                _contentRect.anchoredPosition = new Vector2(xPos + speed, _contentRect.anchoredPosition.y);

                var button = Content.GetComponentInChildren<Button>();
                if (button != null && Selectable.SelectionManager.Selected != button.SelectionParent)
                {
                    Selectable.SelectionManager.UpdateSelection(button.SelectionParent);
                }
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(0.0f, _contentRect.anchoredPosition.y);
            }
        }

        private void ScrollLeft()
        {
            ScrollLeft(ScrollSpeed);
        }
    }
}
