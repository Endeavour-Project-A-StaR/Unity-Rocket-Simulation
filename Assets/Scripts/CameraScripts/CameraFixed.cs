using UnityEngine;

public class CameraFixed : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (!enabled || target == null) return;

        transform.LookAt(target);
    }
}