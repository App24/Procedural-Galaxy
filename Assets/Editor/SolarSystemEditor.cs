using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SolarSystem))]
[CanEditMultipleObjects]
public class SolarSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Randomize"))
        {
            var random = new System.Random();
            foreach (var target in targets)
            {
                var solarSystem = target as SolarSystem;
                solarSystem.seed = random.Next();
            }
            if(Application.isPlaying)
            foreach (var target in targets)
            {
                var solarSystem = target as SolarSystem;
                solarSystem.Recreate();
            }
        }
    }
}
