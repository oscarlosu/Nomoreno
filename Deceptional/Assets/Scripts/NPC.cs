using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Assets.Scripts;

public class NPC : MonoBehaviour
{
    private static List<NPC> npcList;
    public static List<NPC> NPCList
    {
        get
        {
            if(NPC.npcList == null)
            {
                NPC.npcList = new List<NPC>();
            }
            return NPC.npcList;
        }
    }

    public string Name;
    public bool IsMale;
    /**
    *  Index of this NPC in NPCList
    *
    */
    public int Index {
        get {
            return NPC.NPCList.IndexOf(this);
        }
    }
    public Conversation Conversation { get; set; }

    /// <summary>
    /// False means they will talk with the detective, true means they will refuse to talk to him
    /// </summary>
    public bool Mood { get; set; }
    //public bool Mood;

    public bool IsKiller { get; set; }
    //public bool IsKiller    

    public MeshFilter HeadMeshFilter;
    public MeshRenderer HeadRenderer;
    public MeshFilter TorsoMeshFilter;
    public MeshRenderer TorsoRenderer;
    public MeshFilter LegsMeshFilter;
    public MeshRenderer LegsRenderer;

    public NPCPart Head { get; set; }
    public NPCPart Torso { get; set; }
    public NPCPart Legs { get; set; }

    private NavMeshAgent navAgent;
    public Vector3 InterrogationPosition;
    private Animator anim;

    private NPC mingleTarget;
    public float BehaviourChangeChance;

    public float MinglingDistance;
    public float MinglingDuration;
    public Vector3 WaitingRoomMin;
    public Vector3 WaitingRoomMax;
    [Tooltip("The higher the value, the higher the chance to Mingle and the lower the chance to Roam. [0, 1]")]
    public float MingleRoamChanceRatio;

    public bool CanMingle;
    public int MaxSelectionAttempts;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("Mono " + GetInstanceID());
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        // Add self to list
        NPC.NPCList.Add(this);
        Mood = false;
        CanMingle = true;
        // NPCs always start waiting
        StartCoroutine(Waiting());
    }

    public void Assemble(NPCPart head, NPCPart torso, NPCPart legs)
    {
        // Save parts
        this.Head = head;
        this.Torso = torso;
        this.Legs = legs;
        // Load meshes and materials
        // Head
        HeadMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Head.Type.ToString());
        HeadRenderer.material = Resources.Load<Material>("Materials/" + this.Head.Description.ToString());
        // Torso
        TorsoMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Torso.Type.ToString());
        TorsoRenderer.material = Resources.Load<Material>("Materials/" + this.Torso.Description.ToString());
        // Legs
        LegsMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Legs.Type.ToString());
        LegsRenderer.material = Resources.Load<Material>("Materials/" + this.Legs.Description.ToString());
    }

    private IEnumerator HandleWalkAnimation()
    {
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
    
    private IEnumerator Waiting()
    {
        Debug.Log("Waiting");
        CanMingle = true;
        while (true)
        {
            // Switch to Roam or Mingle?
            if (Random.Range(0.0f, 1.0f) < BehaviourChangeChance)
            {
                // Select Roam or Mingle
                if(Random.Range(0.0f, 1.0f) < MingleRoamChanceRatio)
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
        Debug.Log("Mingle");
        CanMingle = false;
        // Select target (make sure it is available)
        int index = Random.Range(0, NPC.NPCList.Count);
        NPC target = NPC.NPCList[index];
        bool found = false;
        for (int i = 0; i < MaxSelectionAttempts; ++i)
        {            
            if(target.CanMingle)
            {
                found = true;
                break;                
            }
            index = Random.Range(0, NPC.NPCList.Count);
            target = NPC.NPCList[index];
        }
        if(!found)
        {
            //CanMingle = true;
            StartCoroutine(Waiting());
            Debug.Log("Mingle cancelled");
            yield break;
        }
        // Set animator state
        anim.SetBool("Walk", true);
        // Inform target of mingling start
        target.WaitForMingle(this);
        // Navigate to target
        navAgent.SetDestination(target.transform.position);
        // Wait until near enough to the target
        do
        {
            yield return null;
        } while (navAgent.remainingDistance > MinglingDistance);
        // Set animator state
        anim.SetBool("Walk", false);
        // Display result of mingling
        // TODO
        // Wait for mingling duration
        yield return new WaitForSeconds(MinglingDuration);
        // Inform target of mingling end
        // TODO: Unnecesary? <- Handled by  WaitForMingle
        // Start Roam coroutine
        StartCoroutine(Roam());
        // End
    }

    private IEnumerator Roam()
    {
        Debug.Log("Roam");
        CanMingle = true;
        // Select destination
        Vector3 result = RandomVector3(WaitingRoomMin, WaitingRoomMax);
        navAgent.SetDestination(result);
        // Set animator state
        anim.SetBool("Walk", true);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Walk", false);
        // Start Waiting coroutine
        StartCoroutine(Waiting());
        // End
    }

    private Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        float minX = Mathf.Min(min.x, max.x);
        float maxX = Mathf.Max(min.x, max.x);
        float minY = Mathf.Min(min.y, max.y);
        float maxY = Mathf.Max(min.y, max.y);
        float minZ = Mathf.Min(min.z, max.z);
        float maxZ = Mathf.Max(min.z, max.z);
        return new Vector3(Random.Range(minX, maxX),
                           Random.Range(minY, maxY),
                           Random.Range(minZ, maxZ));
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
        Debug.Log("WaitForMingle");
        anim.SetBool("Walk", false);
        CanMingle = false;
        // Wait for other NPC to get near
        while (Vector3.Distance(transform.position, other.transform.position) > MinglingDistance)
        {
            yield return null;
        }
        // Face other NPC
        Quaternion rotation = transform.rotation;
        transform.LookAt(other.transform);
        // Wait for other NPC to finish mingling
        // TODO: Is this good enough?
        yield return new WaitForSeconds(MinglingDuration);
        // Face original direction
        transform.rotation = rotation;
        // Start Waiting coroutine
        StartCoroutine(Waiting());
        // End
    }
    /*
    private void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    */

    public void GoToInterrogation()
    {
        // Interrupt other coroutines
        StopAllCoroutines();
        // Start interrogation routine
        StartCoroutine(CoGoToInterrogation());        
    }

    private IEnumerator CoGoToInterrogation()
    {
        Debug.Log("GoToInterrogation");
        CanMingle = false;
        // Set animator state
        anim.SetBool("Walk", true);        
        // Navigate to interrogation room
        navAgent.SetDestination(InterrogationPosition);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Walk", false);
        // Inform Player Controller of arrival
        PlayerController.Instance.DisplayConversation();
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
        Debug.Log("GoToWaiting");
        CanMingle = false;
        // Select destination in waiting room
        Vector3 result = RandomVector3(WaitingRoomMin, WaitingRoomMax);
        // Set animation state
        anim.SetBool("Walk", true);
        // Navigate to waiting room
        navAgent.SetDestination(result);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Walk", false);
        // Start Waiting coroutine
        StartCoroutine(Waiting());
        // End
    }

}
