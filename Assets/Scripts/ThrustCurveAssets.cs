using UnityEngine;

[CreateAssetMenu(menuName = "Rocket/Thrust Curve")]
public class ThrustCurveAsset : ScriptableObject
{
    public AnimationCurve curve;
    public TextAsset csvFile;
}