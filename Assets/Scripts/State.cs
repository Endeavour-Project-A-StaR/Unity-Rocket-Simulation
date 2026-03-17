using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public float[] canard_angles;

    public State(float t, Vector3 pos, Quaternion rot, float[] c_angles)
    {
        time = t;
        position = pos;
        rotation = rot;
        canard_angles = c_angles;
    }
}
