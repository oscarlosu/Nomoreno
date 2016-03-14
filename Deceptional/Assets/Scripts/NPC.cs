using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    public string Name;
    /**
    *  Index of this NPC in NPCList
    *
    */
    public int Index;
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

    private NPC mingleTarget;
    public float BehaviourChangeChance;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("Mono " + GetInstanceID());
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        // Save index and add self to NPCList
        Index = NPCList.Instance.Count;
        NPCList.Instance.Add(this);
        // NPCs always start waiting
        StartCoroutine(Waiting());
        return;





        // TESTS
        // Assemble test
        NPCPart head = new NPCPart(NPCPart.NPCPartType.Hat, NPCPart.NPCPartDescription.Black);
        NPCPart torso = new NPCPart(NPCPart.NPCPartType.Shirt, NPCPart.NPCPartDescription.Yellow);
        NPCPart legs = new NPCPart(NPCPart.NPCPartType.Pants, NPCPart.NPCPartDescription.Red);
        Assemble(head, torso, legs);
        // Test navmesh agent
        GoToInterrogation();
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

    private IEnumerator HandleWalkAnimation()
    {
        // Start walk animation        
        anim.SetBool("Travelling", true);
        // Wait for agent to reach destination
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Stop walk animation
        anim.SetBool("Travelling", false);
    }
    
    private IEnumerator Waiting()
    {
        while(true)
        {
            // Switch to Roam or Mingle?
            if (Random.Range(0.0f, 1.0f) < BehaviourChangeChance)
            {
                // Select Roam or Mingle
                if(Random.Range(0, 2) == 0)
                {
                    StartCoroutine(Mingle());
                }
                else
                {
                    StartCoroutine(Roam());
                }
                yield break;
            }
            // Wait for one second
            yield return new WaitForSeconds(1.0f);
        }        
    }

    private IEnumerator Mingle()
    {
        // Set animator state
        // Select target (make sure it is available)
        // Inform target of mingling start
        // Navigate to target
        // Wait until target is reached
        // Set animator state
        // Display result of mingling
        // Wait for mingling duration
        // Inform target of mingling end
        // Start Roam coroutine
        // End
        yield return null;
    }

    private IEnumerator Roam()
    {
        // Set animator state
        // Select destination
        // Wait until target is reached
        // Set animation state
        // Start Waiting coroutine
        // End
        yield return null;
    }

    public bool CanMingle()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Roam") &&
               anim.GetCurrentAnimatorStateInfo(0).IsName("Waiting");
    }

    public void WaitForMingle(NPC other)
    {
        // Interrupt other coroutines
        StopAllCoroutines();
        // Start WaitForMingle coroutine
        StartCoroutine(CoWaitForMingle(other));
    }

    private IEnumerator CoWaitForMingle(NPC other)
    {
        // Set animator state
        // Wait for other NPC to get near
        // Face other NPC
        // Wait for other NPC to finish mingling
        // Set animator state
        // Face original direction
        // Start Waiting coroutine
        // End
        yield return null;
    }

    public void GoToInterrogation()
    {
        // Interrupt other coroutines
        StopAllCoroutines();
        // Start interrogation routine
        StartCoroutine(CoGoToInterrogation());        
    }

    private IEnumerator CoGoToInterrogation()
    {
        // Set animator state
        anim.SetBool("Interrogation", true);
        anim.SetBool("Travelling", true);
        // Navigate to interrogation room
        navAgent.SetDestination(InterrogationPosition);
        // Wait until target is reached
        // Set animation state
        // Inform Player Controller of arrival
        yield return null;
    }

    public void GoToWaiting()
    {
        // Interrupt other coroutines 
        StopAllCoroutines();
        // Start GoToWaiting coroutine
        StartCoroutine(CoGoToWaiting());
    }

    private IEnumerator CoGoToWaiting()
    {
        // Set animation state
        // Select destination in waiting room
        // Navigate to waiting room
        // Wait until target is reached
        // Set animation state
        // Start Waiting coroutine
        // End
        yield return null;
    }

}
