using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineTesting : MonoBehaviour {
    List<Coroutine> list;
	// Use this for initialization
	void Start () {
        list = new List<Coroutine>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A)) {
            Coroutine inst = StartCoroutine(MyCoroutine());
            list.Add(inst);
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            for(int i = list.Count -1; i >=0; --i) {
                StopCoroutine(list[i]);
                list.RemoveAt(i);
            }            
        }
    }
    IEnumerator MyCoroutine() {
        while(true) {
            Debug.Log("Running");
            yield return null;
        }
    }
}
