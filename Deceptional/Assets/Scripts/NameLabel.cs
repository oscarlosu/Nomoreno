using UnityEngine;
using System.Collections;

public class NameLabel : MonoBehaviour {
    void OnEnable() {
        SetRotation();
    }
	// Update is called once per frame
	void Update () {
        SetRotation();
    }
    private void SetRotation() {        
        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }

}
