// Simulated hardware sensor readings for one timestep, exported as JSON Lines
// (one JsonUtility.ToJson(SensorFrame) call per line) so it can be read directly
// in Python via pandas.read_json(path, lines=True).
[System.Serializable]
public class SensorFrame
{
    public int timestamp;      // ms, simulated flight time (fixed-step accumulated, not wall clock)
    public int state;          // SensorSimulator.FlightPhase code

    public float[] raw_accel;  // body-frame specific force [x,y,z], m/s^2 (noisy + biased)
    public float[] raw_gyro;   // body-frame angular rate [x,y,z], rad/s (noisy + biased)

    public float pressure;     // Pa, noisy simulated barometric pressure
    public float altitude;     // m, ESTIMATE inverted from `pressure` — not ground truth

    // Ground-truth orientation for this pass — there is no attitude-estimation filter
    // yet. A complementary/Madgwick filter consuming raw_accel/raw_gyro is a natural
    // future enhancement enabled by this class.
    public float[] quats;      // [x,y,z,w]

    // No servo-lag model in this pass since CanardController.SetDeflections() is
    // instantaneous; a future addition could add slew-rate/latency here too.
    public float[] servo;      // 4 canard deflection angles, degrees, ground-truth actuator position

    public float[] gyro_bias;  // [x,y,z] internal random-walk gyro bias state, rad/s
}
