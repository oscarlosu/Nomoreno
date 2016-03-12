using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{

    public string Name;
    public Conversation Conversation { get; set; }

    public bool Mood { get; set; }
    //public bool Mood;

    public bool IsKiller { get; set; }
    //public bool IsKiller    

    public SkinnedMeshRenderer HeadRenderer;
    public SkinnedMeshRenderer TorsoRenderer;
    public SkinnedMeshRenderer LegsRenderer;

    private NPCPart head;
    private NPCPart torso;
    private NPCPart legs;

    private NavMeshAgent navAgent;
    public Vector3 InterrogationPosition;
    private Animator anim;


    // Use this for initialization
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        // TESTS
        // Assemble test
        NPCPart head = new NPCPart(NPCPart.NPCPartType.Hat, NPCPart.NPCPartDescription.Black);
        NPCPart torso = new NPCPart(NPCPart.NPCPartType.Shirt, NPCPart.NPCPartDescription.Yellow);
        NPCPart legs = new NPCPart(NPCPart.NPCPartType.Pants, NPCPart.NPCPartDescription.Red);
        Assemble(head, torso, legs);
        // Test navmesh agent
        GoToInterrogation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Assemble(NPCPart head, NPCPart torso, NPCPart legs)
    {
        // Save parts
        this.head = head;
        this.torso = torso;
        this.legs = legs;
        // Load meshes and materials
        // Head
        HeadRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + this.head.Type.ToString());
        HeadRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + this.head.Description.ToString());
        // Torso
        TorsoRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + this.torso.Type.ToString());
        TorsoRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + this.torso.Description.ToString());
        // Legs
        LegsRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + this.legs.Type.ToString());
        LegsRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + this.legs.Description.ToString());
    }

    public void GoToInterrogation()
    {
        navAgent.SetDestination(InterrogationPosition);
        StartCoroutine(HandleWalkAnimation());
    }

    private IEnumerator HandleWalkAnimation()
    {
        Debug.Log(navAgent.remainingDistance);
        // Start walk animation        
        anim.SetBool("Walk", true);

        // Wait for agent to reach destination
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Stop walk animation
        anim.SetBool("Walk", false);
    }

    public void Behaviour()
    {

    }

}
