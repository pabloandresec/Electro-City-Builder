using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MissionController))]
public class MissionControllerCI : Editor
{
    private MissionController tgt;

    private void OnEnable()
    {
        tgt = (MissionController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Print missions state"))
        {
            tgt.PrintMissions();
        }
    }
}
