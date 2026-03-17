using UnityEngine;

public class CameraChase : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0.2f, -0.1f);
    public float smoothSpeed = 5f;
    public float distanceFactor = 1f;

    void LateUpdate()
    {
        if (!enabled || target == null) return;

        Vector3 desiredPos = target.position + target.TransformDirection(distanceFactor * offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
