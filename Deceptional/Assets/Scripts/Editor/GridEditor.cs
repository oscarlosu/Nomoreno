using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Grid myScript = (Grid)target;
        if (GUILayout.Button("Find Adjacent Cells")) {
            myScript.FindAdjacentCells();
        }
    }
}