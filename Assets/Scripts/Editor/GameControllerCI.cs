using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(GameController))]
public class GameControllerCI : Editor
{
    GameController gc;
    private ReorderableList buildingList;

    private void OnEnable()
    {
        gc = (GameController)target;

        buildingList = new ReorderableList(serializedObject,
                       serializedObject.FindProperty("Waves"),
                       true, true, true, true);
    }
}
