using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour {
    public List<Cell> Adjacent;
    public bool Free;

    public void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f); ;
        Grid grid = Grid.Instance;
        Gizmos.DrawCube(transform.position, new Vector3(grid.CellSideLength, 0.02f, grid.CellSideLength));
        name = "(" + transform.position.x + ", " + transform.position.z + ")";
    }
}
