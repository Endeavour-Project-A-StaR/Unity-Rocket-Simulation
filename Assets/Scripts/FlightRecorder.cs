using System;
using System.IO;
using UnityEngine;

/*
 * Records true state and sensor readings as paired JSONL files in FlightLogs, beside Assets.
 * Each line is one frame; timestamps use the SensorSimulator clock in milliseconds.
 * ! Matching timestamps do not guarantee that all callbacks sampled the same physics instant.
 */

public class FlightRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RocketSim rocketSim;
    [SerializeField] private CanardController canardController;
    [SerializeField] private SensorSimulator sensorSimulator;

    [Header("Recording Control")]
    [SerializeField] private bool recordOnStart = true;
    [SerializeField] private bool autoStopWhenLanded = true;

    [Header("Output")]
    [SerializeField] private string outputFolderName = "FlightLogs"; // sibling of Assets/, not inside it
    [SerializeField] private int flushEveryNFrames = 50;             // ~1 sim second at 50 Hz

    private Rigidbody rb;
    private bool isRecording;
    private int tickCounter;
    private StreamWriter truthWriter;
    private StreamWriter sensorWriter;
    private string truthFilePath;
    private string sensorFilePath;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rocketSim == null)
            Debug.LogError("FlightRecorder: RocketSim reference not assigned in the Inspector.");
        if (canardController == null)
            Debug.LogError("FlightRecorder: CanardController reference not assigned in the Inspector.");
        if (sensorSimulator == null)
            Debug.LogError("FlightRecorder: SensorSimulator reference not assigned in the Inspector.");

        if (recordOnStart)
            StartRecording();
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        int ts = sensorSimulator.GetTimestampMillis();
        RocketStateFrame truth = BuildTruthFrame(ts);
        SensorFrame sensor = sensorSimulator.GetLatestReading();

        truthWriter.WriteLine(JsonUtility.ToJson(truth));
        sensorWriter.WriteLine(JsonUtility.ToJson(sensor));

        if (++tickCounter % flushEveryNFrames == 0)
        {
            truthWriter.Flush();
            sensorWriter.Flush();
        }

        if (autoStopWhenLanded && sensorSimulator.GetFlightPhase() == SensorSimulator.FlightPhase.Landed)
            StopRecording();
    }

    private RocketStateFrame BuildTruthFrame(int timestamp)
    {
        Vector3 pos = transform.position;
        Vector3 vel = rb.linearVelocity;
        Quaternion rot = transform.rotation;
        Vector3 angVel = rb.angularVelocity;

        float[] rawCanardAngles = canardController.GetCanardAngles();
        float[] canardAnglesCopy = new float[4];
        for (int i = 0; i < 4 && i < rawCanardAngles.Length; i++)
            canardAnglesCopy[i] = rawCanardAngles[i];

        return new RocketStateFrame
        {
            timestamp = timestamp,
            state = (int)sensorSimulator.GetFlightPhase(),
            position = new float[] { pos.x, pos.y, pos.z },
            velocity = new float[] { vel.x, vel.y, vel.z },
            quats = new float[] { rot.x, rot.y, rot.z, rot.w },
            angular_velocity = new float[] { angVel.x, angVel.y, angVel.z },
            mass = rb.mass,
            fuel_remaining = rocketSim.GetRemainingFuel(),
            altitude = rocketSim.GetAltitude(),
            speed = rocketSim.GetSpeed(),
            angle_of_attack = rocketSim.GetAngleOfAttack(),
            canard_angles = canardAnglesCopy,
            engine_on = rocketSim.IsEngineOn(),
        };
    }

    public void StartRecording()
    {
        if (isRecording) return;

        string root = Directory.GetParent(Application.dataPath).FullName; // sibling of Assets/
        string folder = Path.Combine(root, outputFolderName);
        Directory.CreateDirectory(folder);

        string runId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        truthFilePath = Path.Combine(folder, $"flight_{runId}_truth.jsonl");
        sensorFilePath = Path.Combine(folder, $"flight_{runId}_sensors.jsonl");

        truthWriter = new StreamWriter(truthFilePath, append: false);
        sensorWriter = new StreamWriter(sensorFilePath, append: false);

        tickCounter = 0;
        isRecording = true;

        Debug.Log($"FlightRecorder: recording to {truthFilePath} and {sensorFilePath}");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        truthWriter.Flush();
        truthWriter.Close();
        sensorWriter.Flush();
        sensorWriter.Close();

        isRecording = false;
        Debug.Log("FlightRecorder: recording stopped.");
    }

    void OnDestroy()
    {
        if (isRecording) StopRecording();
    }

    void OnApplicationQuit()
    {
        if (isRecording) StopRecording();
    }
}
