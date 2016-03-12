using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RigTester : MonoBehaviour {

    public SkinnedMeshRenderer hatSocket;

    public List<Mesh> hats;
    public int index;    

    private Animation anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animation>();
        index = 0;

        // Load hats from file
        Mesh[] hatContainers = Resources.LoadAll<Mesh>("Hats");
        hats.AddRange(hatContainers);
        
    }
	
	// Update is called once per frame
	void Update () {
        // Loop through loaded hats when spacebar is pressed
        if(Input.GetKeyDown(KeyCode.Space))
        {
            index = (index + 1) % hats.Count;
            hatSocket.sharedMesh = hats[index];
        }
        // Restart animation as soon as it ends
	    if(!anim.isPlaying)
        {
            anim.Play();
        }
	}
}
