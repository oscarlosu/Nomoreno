using UnityEngine;
using System.Collections;

public class HoverOver : MonoBehaviour {

    //public Transform FacingTarget;
    public SpriteRenderer Rend;

    void Update() {
        transform.LookAt(Camera.main.transform.position);
        //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);



        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
