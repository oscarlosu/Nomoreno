using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;

public class Grid : MonoBehaviour {
    private static Grid instance = null;
    public static Grid Instance {
        get {
            if (instance == null) {
                instance = (Grid)FindObjectOfType(typeof(Grid));
                //if (instance == null)
                //    instance = (new GameObject("Grid")).AddComponent<Grid>();
            }
            return instance;
        }
    }
    public List<Cell> Cells;
    public List<Cell> FreeCells;
    public List<Cell> IsolatedCells;

    public float CellSideLength;
    private float ErrorMargin = 0.5f;

    public int MaxSearchAttempts = 20;
	
    public void FreeCell(Cell cell) {
        if(cell != null && !cell.Free) {
            cell.Free = true;
            FreeCells.Add(cell);
        }        
    }
    public Cell GetRandomCell() {
        Cell cell = null;
        if (FreeCells.Count > 0) {
            int index = PlayerController.Instance.Rng.Next(0, FreeCells.Count);
            cell = FreeCells[index];
            FreeCells.RemoveAt(index);
            cell.Free = false;
        }
        return cell;
    }

    public Cell GetIsolatedCell() {
        Cell cell = null;
        if (FreeCells.Count > 0) {
            int index = 0;
            for(int i = 0; i < MaxSearchAttempts; ++i) {
                index = PlayerController.Instance.Rng.Next(0, FreeCells.Count);
                cell = FreeCells[index];
                if(IsIsolated(cell)) {
                    break;
                }
            }
            FreeCells.RemoveAt(index);
            cell.Free = false;

        }
        return cell;
    }

    private bool IsIsolated(Cell cell) {
        foreach(Cell adj in cell.Adjacent) {
            if(!adj.Free) {
                return false;
            }
        }
        return true;
    }

    public void TakeCell (Cell cell) {
        FreeCells.Remove(cell);
        cell.Free = false;
    }

    public void SetupGrid() {
        // Find adjacent cells
        // Find Cells
        Cell[] cellArray = transform.GetComponentsInChildren<Cell>();
        List<Cell> cells = new List<Cell>(cellArray);
        // Sort Cells        
        List<Cell> sortedCells = cells.OrderBy(c => c.transform.position.x).ToList<Cell>();
        // Save adjacent cells
        for(int i = 0; i < sortedCells.Count; ++i) {
            Cell current = sortedCells[i];
            current.Adjacent.Clear();
            for (int j = 0; j < sortedCells.Count; ++j) {
                Cell other = sortedCells[j];
                // Skip current cell
                if (current.GetInstanceID() != other.GetInstanceID()) {
                    // Stop searching after the x coordinate already guarantees that the rest of the cells are too far
                    if (Mathf.Abs(current.transform.position.x - other.transform.position.x) < CellSideLength + ErrorMargin) {
                        if (Mathf.Abs(current.transform.position.z - other.transform.position.z) < CellSideLength + ErrorMargin) {                        
                            current.Adjacent.Add(other);
                        }                        
                    }
                }
            }
        }
        // Populate lists
        Cells = sortedCells;
        FreeCells = sortedCells;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            int takenCells = Cells.Count - FreeCells.Count;
            Debug.Log("Taken Cells: " + takenCells);
        }
    }
}
