using NUnit.Framework;
using UnityEngine;

public class CanardController : MonoBehaviour
{
    [Header("Canard References")]
    [SerializeField] private Transform canardsParent; // Assign the "canards" parent object

    [Header("Control Settings")]
    [SerializeField] private float maxDeflectionAngle = 20f; // Maximum deflection in degrees
    [SerializeField] private float deflectionSpeed = 60f; // Degrees per second

    [Header("Current Deflections")]
    [SerializeField] private float pitchDeflection = 0f; // Up/Down (Z-axis rotation)
    [SerializeField] private float yawDeflection = 0f;   // Left/Right (X-axis rotation)

    private Transform[] canards;
    private float[] canardAngles;

    void Start()
    {
        canards = new Transform[4];
        canardAngles = new float[4];

        for (int i = 0; i < 4; i++) { canards[i] = canardsParent.GetChild(i); }
    }

    void Update()
    {
        ManageInput();
        ApplyCanardRotations();
    }

    void ManageInput()
    {
        // Left/Right arrows yaw control
        if (Input.GetKey(KeyCode.LeftArrow)) { yawDeflection -= deflectionSpeed * Time.deltaTime; }
        else if (Input.GetKey(KeyCode.RightArrow)) { yawDeflection += deflectionSpeed * Time.deltaTime; }
        else { yawDeflection = Mathf.MoveTowards(yawDeflection, 0f, deflectionSpeed * Time.deltaTime); }
        

        // Up/Down arrows pitch control
        //if (Input.GetKey(KeyCode.UpArrow)){ pitchDeflection += deflectionSpeed * Time.deltaTime; }
        //else if (Input.GetKey(KeyCode.DownArrow)) { pitchDeflection -= deflectionSpeed * Time.deltaTime; }
        //else { pitchDeflection = Mathf.MoveTowards(pitchDeflection, 0f, deflectionSpeed * Time.deltaTime);  }

        // Clamp to max deflection angles
        yawDeflection = Mathf.Clamp(yawDeflection, -maxDeflectionAngle, maxDeflectionAngle);
        //pitchDeflection = Mathf.Clamp(pitchDeflection, -maxDeflectionAngle, maxDeflectionAngle); // !Removed for now 
    }

    void ApplyCanardRotations()
    {
        if (canards == null || canards.Length == 0) return;

        // This applies the same deflection to all canards. The index matches the canard name/position in the hierarchy
        // Viewed from above, the canards are ordered 0-3 where 0 is normal in the rocket x direction, 1 is normal in the y direction
        // and so on moving anticlockwise from a top-down view
        // Canard deflections are applied clockwise in units of degrees from an external view (looking at the canard normally)
        canards[0].localRotation = Quaternion.Euler(yawDeflection, 0, 0);
        canardAngles[0] = yawDeflection;
        canards[1].localRotation = Quaternion.Euler(0, yawDeflection, 0);
        canardAngles[1] = yawDeflection;
        canards[2].localRotation = Quaternion.Euler(-yawDeflection, 0, 0);
        canardAngles[2] = yawDeflection;
        canards[3].localRotation = Quaternion.Euler(0, -yawDeflection, 0);
        canardAngles[3] = yawDeflection;
    }



    
    public float GetPitchDeflection()
    {
        return pitchDeflection;
    }

    public float GetYawDeflection()
    {
        return yawDeflection;
    }

    // Optional: Reset to neutral position
    public void ResetCanards()
    {
        pitchDeflection = 0f;
        yawDeflection = 0f;
    }

    public float[] GetCanardAngles()
    {
        return canardAngles;
    }

    public Transform[] GetCanardTransforms()
    {
        return canards;
    }

    public float GetCanardsHeight() 
    {
        return 0.9f;
    }

    void OnGUI()
    {
        // Simple HUD to show current deflections
        GUI.Label(new Rect(10, 10, 200, 20), $"Pitch: {pitchDeflection:F1}°");
        GUI.Label(new Rect(10, 30, 200, 20), $"Yaw: {yawDeflection:F1}°");
        GUI.Label(new Rect(10, 50, 300, 20), "Arrow Keys: Control Canards");
    }
}