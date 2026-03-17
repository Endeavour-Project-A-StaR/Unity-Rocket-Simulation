using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ThrustCurveAsset))]
public class ThrustCurveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ThrustCurveAsset asset = (ThrustCurveAsset)target;

        if (GUILayout.Button("Import CSV"))
        {
            if (asset.csvFile == null)
            {
                Debug.LogError("No CSV file assigned.");
                return;
            }

            string[] lines = asset.csvFile.text.Split('\n');
            List<Keyframe> keys = new List<Keyframe>();

            // Basic CSV parsing with a space separator, new lines, and no header
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(' ');

                float time = float.Parse(parts[0]);
                float thrust = float.Parse(parts[1]);

                keys.Add(new Keyframe(time, thrust));
            }

            asset.curve = new AnimationCurve(keys.ToArray());
            EditorUtility.SetDirty(asset);
            Debug.Log("Thrust curve imported.");
        }
    }
}