using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Grid myScript = (Grid)target;
        if (GUILayout.Button("Ship it!")) {
            myScript.SetupGrid();
        }
    }
}