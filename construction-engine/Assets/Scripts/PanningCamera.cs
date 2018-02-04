using UnityEngine;

public class PanningCamera : MonoBehaviour
{
    public float Speed = 0.75f;
    public float ScreenEdge = 50.0f;

    private Transform _pos;
    private Vector3 _right;
    private Vector3 _forward;

	void Start ()
    {
        _pos = this.transform;
        var yRotation = _pos.rotation.eulerAngles.y;

        _forward = new Vector3(Mathf.Cos(Mathf.Deg2Rad * yRotation), 0.0f, Mathf.Sin(Mathf.Deg2Rad * yRotation));
        _right = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (yRotation - 90.0f)), 0.0f, Mathf.Sin(Mathf.Deg2Rad * (yRotation - 90.0f)));
	}

	void Update ()
    {
        float vx = Input.GetAxis("Horizontal") * Speed;
        float vz = Input.GetAxis("Vertical") * Speed;

        // Only pan with camera if the mouse is inside the screen
        if (Input.mousePosition.x > 0.0f - ScreenEdge &&
            Input.mousePosition.y > 0.0f - ScreenEdge &&
            Input.mousePosition.x < Screen.width + ScreenEdge &&
            Input.mousePosition.y < Screen.height + ScreenEdge)
        {
            if (Input.mousePosition.x <= 0.0f + ScreenEdge)
                vx -= Speed;

            if (Input.mousePosition.y <= 0.0f + ScreenEdge)
                vz -= Speed;

            if (Input.mousePosition.x >= Screen.width - ScreenEdge)
                vx += Speed;

            if (Input.mousePosition.y >= Screen.height - ScreenEdge)
                vz += Speed;
        }

        _pos.position += vx * _right;
        _pos.position += vz * _forward;
	}
}
