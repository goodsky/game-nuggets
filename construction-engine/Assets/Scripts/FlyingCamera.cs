using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    public float Speed = 1.0f;
    public float Rotation = 1.0f;

    private Transform _pos;
    private float _adjustedSpeed;

	void Start ()
    {
        _pos = this.transform;
        _adjustedSpeed = Speed * Mathf.Sqrt(0.5f);
	}

    void Update()
    {
        float vz = Input.GetAxis("Vertical");
        float vy = Input.GetAxis("Skyler");
        float vx = Input.GetAxis("Horizontal");
        float vrx = Input.GetAxis("Mouse Y");
        float vry = Input.GetAxis("Mouse X");

        if (vz != 0 && vx != 0)
        {
            vz = vz < 0.0f ? -_adjustedSpeed : _adjustedSpeed;
            vx = vx < 0.0f ? -_adjustedSpeed : _adjustedSpeed;
        }

        _pos.position += _pos.forward * vz;
        _pos.position += _pos.up * vy;
        _pos.position += _pos.right * vx;
        _pos.localEulerAngles += new Vector3(-vrx, vry, 0.0f);
    }
}
