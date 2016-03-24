﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour {
    private static Grid instance = null;
    public static Grid Instance {
        get {
            if (instance == null) {
                instance = (Grid)FindObjectOfType(typeof(Grid));
                if (instance == null)
                    instance = (new GameObject("Grid")).AddComponent<Grid>();
            }
            return instance;
        }
    }
    public List<Cell> Cells;
    public List<Cell> FreeCells;

    public float CellSideLength;
    private float ErrorMargin = 0.5f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void FindAdjacentCells() {
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

    }
}
