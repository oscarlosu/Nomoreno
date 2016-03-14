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

    public float MinglingDistance;
    public float MinglingDuration;
    public Vector3 WaitingRoomMin;
    public Vector3 WaitingRoomMax;


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
        /*
        // TESTS
        // Assemble test
        NPCPart head = new NPCPart(NPCPart.NPCPartType.Hat, NPCPart.NPCPartDescription.Black);
        NPCPart torso = new NPCPart(NPCPart.NPCPartType.Shirt, NPCPart.NPCPartDescription.Yellow);
        NPCPart legs = new NPCPart(NPCPart.NPCPartType.Pants, NPCPart.NPCPartDescription.Red);
        Assemble(head, torso, legs);
        // Test navmesh agent
        GoToInterrogation();
        */
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
        anim.SetTrigger("Mingle");
        anim.SetBool("Travelling", true);
        // Select target (make sure it is available)
        int index;
        NPC target;
        do
        {
            index = Random.Range(0, NPCList.Instance.Count);
            target = NPCList.Instance.Get(index);
        } while (!target.CanMingle());
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
        anim.SetBool("Travelling", false);
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
        // Set animator state
        anim.SetBool("Interrogation", false);
        anim.SetBool("Travelling", true);
        // Select destination
        Vector3 result;        
        while (RandomPoint(RandomVector3(WaitingRoomMin, WaitingRoomMax), 10.0f, out result))
        {
            yield return null;
        }
        navAgent.SetDestination(result);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Travelling", false);
        // Start Waiting coroutine
        StartCoroutine(Waiting());
        // End
    }

    private Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x),
                                Random.Range(min.y, max.y),
                                Random.Range(min.z, max.z));
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
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
        anim.SetBool("WaitForMingle", true);
        // Wait for other NPC to get near
        while(Vector3.Distance(transform.position, other.transform.position) > MinglingDistance)
        {
            yield return null;
        }
        // Face other NPC
        Quaternion rotation = transform.rotation;
        transform.LookAt(other.transform);
        // Wait for other NPC to finish mingling
        // TODO: Is this good enough?
        yield return new WaitForSeconds(MinglingDuration);
        // Set animator state
        anim.SetBool("WaitForMingle", false);
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
        // Set animator state
        anim.SetBool("Interrogation", true);
        anim.SetBool("Travelling", true);
        // Navigate to interrogation room
        navAgent.SetDestination(InterrogationPosition);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Travelling", false);
        // Inform Player Controller of arrival
        Assets.Scripts.PlayerController.Instance.DisplayConversation();
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
        anim.SetBool("Interrogation", false);
        anim.SetBool("Travelling", true);
        // Select destination in waiting room
        Vector3 result;
        while (RandomPoint(RandomVector3(WaitingRoomMin, WaitingRoomMax), 10.0f, out result))
        {
            yield return null;
        }
        // Navigate to waiting room
        navAgent.SetDestination(result);
        // Wait until target is reached
        do
        {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Travelling", false);
        // Start Waiting coroutine
        StartCoroutine(Waiting());
        // End
    }

}
