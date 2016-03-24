using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Assets.Scripts;

public class NPC : MonoBehaviour
{
    private static GameObject defaultNpc = Resources.Load<GameObject>(@"Prefabs\NPC");
    public static GameObject DefaultNPC {
        get { return Instantiate(defaultNpc); }
    }

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
    public static Vector3 WaitingRoomMin = new Vector3(8.948f, 0.1f, -8.625f);
    public static Vector3 WaitingRoomMax = new Vector3(-8.764f, 0.1f, 8.765f);
    [Tooltip("The higher the value, the higher the chance to Mingle and the lower the chance to Roam. [0, 1]")]
    public float MingleRoamChanceRatio;

    public bool CanMingle;
    public int MaxSelectionAttempts;

    public Cell currentCell;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("Mono " + GetInstanceID());
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        // Add self to list
        //NPC.NPCList.Add(this);
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
        //HeadMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Head.Type.ToString());
        HeadRenderer.material = Resources.Load<Material>("Materials/" + this.Head.Description.ToString());
        // Torso
        //TorsoMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Torso.Type.ToString());
        TorsoRenderer.material = Resources.Load<Material>("Materials/" + this.Torso.Description.ToString());
        // Legs
        //LegsMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Legs.Type.ToString());
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
        //Vector3 result = RandomVector3(WaitingRoomMin, WaitingRoomMax);
        Grid.Instance.FreeCell(currentCell);
        currentCell = Grid.Instance.GetRandomCell();
        navAgent.SetDestination(currentCell.transform.position);
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
        //Vector3 result = RandomVector3(WaitingRoomMin, WaitingRoomMax);
        currentCell = Grid.Instance.GetRandomCell();
        navAgent.SetDestination(currentCell.transform.position);
        // Set animation state
        anim.SetBool("Walk", true);
        // Navigate to waiting room
        navAgent.SetDestination(currentCell.transform.position);
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
