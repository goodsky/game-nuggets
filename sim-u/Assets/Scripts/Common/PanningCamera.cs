using UnityEngine;

namespace Common
{
    public class PanningCamera : MonoBehaviour
    {
        /// <summary>Speed of movement via keyboards.</summary>
        public float KeyboardSpeed = 0.75f;

        /// <summary>Speed of movement via mouse dragging.</summary>
        public float MouseSpeedModifier = 0.5f;

        /// <summary>Size of the band against the edge of the screen for movement.</summary>
        public float ScreenEdge = 50.0f;

        private float _sqrSpeed;

        private Transform _pos;
        private Vector3 _right;
        private Vector3 _forward;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected void Start()
        {
            _sqrSpeed = KeyboardSpeed * KeyboardSpeed;

            _pos = this.transform;
            var yRotation = _pos.rotation.eulerAngles.y;

            _forward = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (-yRotation + 90.0f)), 0.0f, Mathf.Sin(Mathf.Deg2Rad * (-yRotation + 90.0f)));
            _right = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (-yRotation)), 0.0f, Mathf.Sin(Mathf.Deg2Rad * (-yRotation)));
        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected void Update()
        {
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
            }

            _pos.position += vx * _right;
            _pos.position += vz * _forward;
        }
    }
}