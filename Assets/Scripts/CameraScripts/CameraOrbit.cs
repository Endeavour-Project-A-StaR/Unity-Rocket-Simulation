using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distanceFactor = 20f;
    public float sensitivity = 3f;

    private float yaw = 0f;
    private float pitch = 2f;

    void LateUpdate()
    {
        if (!enabled || target == null) return;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -10f, 80f);


        Bounds bounds = new Bounds(target.position, Vector3.zero);

        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector3 size = bounds.size;   // world‑space width, height, depth


        Vector3 dir = new Vector3(0, 0, -distanceFactor*size.y);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);

        transform.position = target.position + rot * dir;
        transform.LookAt(target);
    }


    public void SetDistanceFactor(float value)
    {
        distanceFactor = value; 
    }


}