using UnityEngine;
using System.Collections;

public class TestOverlapSphere : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Collider[] cols = Physics.OverlapSphere(Vector3.zero, 5.0f);
        if (cols.Length > 0)
        {
            Debug.Log("Overlap detected");
        }
	}
}
