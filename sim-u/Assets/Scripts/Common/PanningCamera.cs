using UnityEngine;

namespace Common
{
    public class PanningCamera : MonoBehaviour
    {
        /// <summary>Speed of movement via keyboards.</summary>
        public float KeyboardSpeed = 0.2f;

        /// <summary>Speed of movement via mouse dragging.</summary>
        public float MouseSpeedModifier = 0.1f;

        /// <summary>Speed of zooming with the mouse wheel.</summary>
        public float MouseWheelModifier = 2.0f;

        /// <summary>Size of the band against the edge of the screen for movement.</summary>
        public float ScreenEdge = 50.0f;

        /// <summary>Minimum height of the camera (for zooming with mouse wheel)</summary>
        public float MinHeight = 3.0f;

        /// <summary>Maximum height of the camera (for zooming with mouse wheel)</summary>
        public float MaxHeight = 5.0f;

        private float _sqrSpeed;

        private Transform _pos;
        private Vector3 _right;
        private Vector3 _forward;
        private Vector3 _zoom;

        private float _minZoom;
        private float _maxZoom;
        private float _zoomPosition;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {
            _sqrSpeed = KeyboardSpeed * KeyboardSpeed;

            _pos = this.transform;
            var yRotation = _pos.rotation.eulerAngles.y;
            var xRotation = _pos.rotation.eulerAngles.x;

            // Calculate movement axes based off of the starting rotation of the camera.
            _forward = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (-yRotation + 90.0f)), 0.0f, Mathf.Sin(Mathf.Deg2Rad * (-yRotation + 90.0f)));
            _right = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (-yRotation)), 0.0f, Mathf.Sin(Mathf.Deg2Rad * (-yRotation)));
            _zoom = _pos.forward;

            _maxZoom = (_pos.transform.position.y - MinHeight) / Mathf.Sin(Mathf.Deg2Rad * xRotation);
            _minZoom = (_pos.transform.position.y - MaxHeight) / Mathf.Sin(Mathf.Deg2Rad * xRotation);
            _zoomPosition = 0.0f;
        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            float vx = 0.0f;
            float vz = 0.0f;
            float vZoom = 0.0f;

            if (Input.GetKey(KeyCode.Mouse1))
            {
                // Free mouse-movement if right-key is down.
                vx -= Input.GetAxis("Mouse X") * MouseSpeedModifier;
                vz -= Input.GetAxis("Mouse Y") * MouseSpeedModifier;

                // TODO: the world selection shouldn't move while right-click moving
            }
            else
            {
                // Arrow-Key and Mouse Edge if right-key is not down.
                vx += Input.GetAxis("Horizontal") * KeyboardSpeed;
                vz += Input.GetAxis("Vertical") * KeyboardSpeed;

                // Pan the camera using the mouse position if the right-key isn't down
                // and the mouse is within the view port.
                if (Input.mousePosition.x > 0.0f &&
                    Input.mousePosition.y > 0.0f &&
                    Input.mousePosition.x < Screen.width &&
                    Input.mousePosition.y < Screen.height)
                {
                    if (Input.mousePosition.x <= 0.0f + ScreenEdge)
                        vx -= KeyboardSpeed;

                    if (Input.mousePosition.y <= 0.0f + ScreenEdge)
                        vz -= KeyboardSpeed;

                    if (Input.mousePosition.x >= Screen.width - ScreenEdge)
                        vx += KeyboardSpeed;

                    if (Input.mousePosition.y >= Screen.height - ScreenEdge)
                        vz += KeyboardSpeed;
                }

                // Normalize speed
                var mag = vx * vx + vz * vz;
                if (mag > _sqrSpeed)
                {
                    var f = Mathf.Sqrt(_sqrSpeed / mag);
                    vx = vx * f;
                    vz = vz * f;
                }

                // Zoom in and out with the mouse wheel
                var wheelScroll = MouseWheelModifier * Input.GetAxis("Mouse ScrollWheel");
                float newZoom = _zoomPosition + wheelScroll;
                if (newZoom > _maxZoom)
                {
                    vZoom = _maxZoom - _zoomPosition;
                    _zoomPosition = _maxZoom;
                }
                else if (newZoom < _minZoom)
                {
                    vZoom = _minZoom - _zoomPosition;
                    _zoomPosition = _minZoom;
                }
                else
                {
                    vZoom = wheelScroll;
                    _zoomPosition = newZoom;
                }
            }

            _pos.position += vx * _right;
            _pos.position += vz * _forward;
            _pos.position += vZoom * _zoom;

            // Cap the position
            _pos.position =
                new Vector3(
                    _pos.position.x,
                    _pos.position.y,
                    _pos.position.z);
        }
    }
}