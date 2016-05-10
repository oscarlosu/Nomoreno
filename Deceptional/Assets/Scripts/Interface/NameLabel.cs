using UnityEngine;
using System.Collections;

public class NameLabel : MonoBehaviour {
    void OnEnable() {
        UpdateRotation();
    }
	// Update is called once per frame
	void Update () {
        UpdateRotation();
    }
    public void UpdateRotation() {        
        transform.eulerAngles = new Vector3(0, -Camera.main.transform.eulerAngles.y, 0);
    }

}
