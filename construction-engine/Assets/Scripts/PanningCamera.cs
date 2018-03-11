using UnityEngine;

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
	protected void Start ()
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
	protected void Update ()
    {
        float vx = Input.GetAxis("Horizontal") * KeyboardSpeed;
        float vz = Input.GetAxis("Vertical") * KeyboardSpeed;

        // Move with the mouse if right-key is down
        if (Input.GetKey(KeyCode.Mouse1))
        {
            vx -= Input.GetAxis("Mouse X") * MouseSpeedModifier;
            vz -= Input.GetAxis("Mouse Y") * MouseSpeedModifier;
        }
        else
        {
            // Pan the camera using the mouse position if the right-key isn't down
            // and if the mouse is close to a screen edge
            if (Input.mousePosition.x > 0.0f - ScreenEdge &&
                Input.mousePosition.y > 0.0f - ScreenEdge &&
                Input.mousePosition.x < Screen.width + ScreenEdge &&
                Input.mousePosition.y < Screen.height + ScreenEdge)
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
        }

        // Normalize speed
        var mag = vx * vx + vz * vz;
        if (mag > _sqrSpeed)
        {
            var f = Mathf.Sqrt(_sqrSpeed / mag);
            vx = vx * f;
            vz = vz * f;
        }

        _pos.position += vx * _right;
        _pos.position += vz * _forward;
	}
}
