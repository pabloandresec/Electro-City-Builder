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
    private ReorderableList componentsList;
    private static bool showBuildings = true;
    private static bool showComponents = true;

    private void OnEnable()
    {
        if(target == null)
        {
            return;
        }

        gc = (GameController)target;

        buildingList = new ReorderableList(serializedObject,
                       serializedObject.FindProperty("buildings"),
                       true, true, true, true);
        buildingList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Buildings");
        };
        buildingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = buildingList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            Rect indexLabelRect = new Rect(rect.x, rect.y, rect.x + 15, EditorGUIUtility.singleLineHeight);
            Rect scriptObjRect = new Rect(rect.x + 10, rect.y, rect.x + rect.width - 50, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(indexLabelRect, index.ToString());
            EditorGUI.PropertyField(scriptObjRect, element, GUIContent.none);
        };

        componentsList = new ReorderableList(serializedObject,
                       serializedObject.FindProperty("components"),
                       true, true, true, true);
        componentsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Components");
        };
        componentsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = componentsList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            Rect compIndexLabelRect = new Rect(rect.x, rect.y, rect.x + 15, EditorGUIUtility.singleLineHeight);
            Rect scriptObjRect = new Rect(rect.x + 10, rect.y, rect.x + rect.width - 50, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(compIndexLabelRect, index.ToString());
            EditorGUI.PropertyField(scriptObjRect, element, GUIContent.none);
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        showBuildings = EditorGUILayout.Foldout(showBuildings, "Building list");
        if(showBuildings)
        {
            buildingList.DoLayoutList();
        }
        showComponents = EditorGUILayout.Foldout(showComponents, "Components list");
        if(showComponents)
        {
            componentsList.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
