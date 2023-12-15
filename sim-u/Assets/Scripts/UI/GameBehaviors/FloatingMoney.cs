using UnityEngine;

namespace UI
{
    public class FloatingMoney : MonoBehaviour
    {
        public float StayAliveTime = 2.0f;
        public float FloatingSpeed = 0.3f;
        public float FloatingDecay = 0.95f;

        private RectTransform _rectTransform;

        /// <summary>
        /// Unity Start call
        /// </summary>
        protected void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            Destroy(gameObject, StayAliveTime);
        }

        /// <summary>
        /// Unity Update call
        /// </summary>
        protected void Update()
        {
            _rectTransform.anchoredPosition = new Vector2(
                _rectTransform.anchoredPosition.x + (FloatingSpeed / 3.0f),
                _rectTransform.anchoredPosition.y + FloatingSpeed);

            FloatingSpeed *= FloatingDecay;
        }
    }
}
