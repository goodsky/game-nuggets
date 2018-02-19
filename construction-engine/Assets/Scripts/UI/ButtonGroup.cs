using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI Element to order and scroll a group of buttons.
    /// Contains stylistic flairs for my design. 
    /// Could be split into 'functionality' and 'flair' subclasses.
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

        private float _maskWidth;
        private RectTransform _contentRect;

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
                // scrolling enabled
                this.ScrollButtonLeft.SetActive(true);
                this.ScrollButtonRight.SetActive(true);
            }
            else
            {
                // scrolling disabled
                this.ScrollButtonLeft.SetActive(false);
                this.ScrollButtonRight.SetActive(false);

                _contentRect.anchoredPosition = new Vector2(padding / 2.0f, _contentRect.anchoredPosition.y);
            }
        }

        private void ScrollLeft()
        {
            float xPos = _contentRect.anchoredPosition.x;
            float rightEdge = xPos + _contentRect.rect.width;

            if (rightEdge > _maskWidth)
            {
                _contentRect.anchoredPosition = new Vector2(xPos - ScrollSpeed, _contentRect.anchoredPosition.y);
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(_maskWidth - _contentRect.rect.width, _contentRect.anchoredPosition.y);
            }
        }

        private void ScrollRight()
        {
            float xPos = _contentRect.anchoredPosition.x;

            if (xPos < 0.0f)
            {
                _contentRect.anchoredPosition = new Vector2(xPos + ScrollSpeed, _contentRect.anchoredPosition.y);
            }
            else
            {
                _contentRect.anchoredPosition = new Vector2(0.0f, _contentRect.anchoredPosition.y);
            }
        }
    }
}
