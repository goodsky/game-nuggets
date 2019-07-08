using UnityEngine;

namespace Common
{
    public class OrthoPanningCamera : MonoBehaviour
    {
        /// <summary>Speed of movement via keyboards.</summary>
        public float KeyboardSpeed = 0.2f;

        /// <summary>Speed of movement via mouse dragging.</summary>
        public float MouseSpeedModifier = 0.1f;

        /// <summary>Speed of zooming with the mouse wheel.</summary>
        public float MouseWheelModifier = 2.0f;

        /// <summary>Size of the band against the edge of the screen for movement.</summary>
        public float ScreenEdge = 50.0f;

        /// <summary>Minimum size of the orthographic camera.</summary>
        public float MinOrthographicSize = 3;

        /// <summary>Maximum size of the orthographic camear.</summary>
        public float MaxOrthographicSize = 7;

        private float _sqrSpeed;

        private Transform _pos;
        private Vector3 _right;
        private Vector3 _forward;

        private Camera _camera;

        private bool _cameraIsMoving = true;

        /// <summary>
        /// Stop the camera from moving and zooming.
        /// E.g. when a window has control of the scene.
        /// </summary>
        public void FreezeCamera()
        {
            _cameraIsMoving = false;
        }

        /// <summary>
        /// Unfreeze the camera. Let it be free you monster!
        /// </summary>
        public void UnfreezeCamera()
        {
            _cameraIsMoving = true;
        }

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

            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
            if (!_cameraIsMoving)
            {
                return;
            }

            float vx = 0.0f;
            float vz = 0.0f;

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
                var wheelScroll = -1 * MouseWheelModifier * Input.GetAxis("Mouse ScrollWheel");
                float newOrthographicSize = _camera.orthographicSize + wheelScroll;
                _camera.orthographicSize = Mathf.Clamp(newOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            }

            _pos.position += vx * _right;
            _pos.position += vz * _forward;

            // Cap the position
            _pos.position =
                new Vector3(
                    _pos.position.x,
                    _pos.position.y,
                    _pos.position.z);
        }
    }
}