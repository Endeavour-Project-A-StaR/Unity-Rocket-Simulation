using UnityEngine;

public class CameraController : MonoBehaviour
{
    //public CameraMode mode = CameraMode.Chase;
    public int mode = 0; // 0: Chase, 1: Orbit, 2: Fixed

    private CameraChase chaseCam;
    private CameraOrbit orbitCam;
    private CameraFixed fixedCam;

    void Awake()
    {
        chaseCam = GetComponent<CameraChase>();
        orbitCam = GetComponent<CameraOrbit>();
        fixedCam = GetComponent<CameraFixed>();
    }

    void Start()
    {
        SetMode(mode);
    }

    public void SetMode(int newMode)
    {
        mode = newMode;

        chaseCam.enabled = (mode == 0);
        orbitCam.enabled = (mode == 1);
        fixedCam.enabled = (mode == 2);
    }
}