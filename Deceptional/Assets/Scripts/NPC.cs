using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Assets.Scripts;

public class NPC : MonoBehaviour, IPointerClickHandler {
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
    public Vector3 PoliceBoxPosition;
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

    public GameObject NameLabelHolder;

    public Sprite Angry;
    public Sprite Happy;
    public Sprite Agree;
    public Sprite Disagree;

    public SpriteRenderer Emoji;
    private bool warped;


    // Use this for initialization
    void Awake()
    {
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        Mood = false;
    }

    void OnEnable() {
        CanMingle = true;
        warped = false;
        // Place npc on random empty position
        Grid.Instance.FreeCell(currentCell);
        currentCell = Grid.Instance.GetRandomCell();
        transform.position = currentCell.transform.position;

        NameLabelHolder.transform.GetComponentInChildren<TextMesh>().text = Name;
        NameLabelHolder.SetActive(false);
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

    public void OnPointerClick(PointerEventData eventData) {
        PlayerController.Instance.SelectedNPC = this;
    }

    public void ShowNameLabel() {
        NameLabelHolder.SetActive(true);
    }
    public void HideNameLabel() {
        NameLabelHolder.SetActive(false);
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
            StartCoroutine(Waiting());
            yield break;
        }
        // Try to get free adjacent cell
        Cell targetCell = currentCell;
        found = false;
        foreach(Cell c in target.currentCell.Adjacent) {
            if(c.Free) {
                targetCell = c;
                Grid.Instance.TakeCell(targetCell);
                found = true;
                break;
            }
        }
        if (!found) {
            StartCoroutine(Waiting());
            yield break;
        }
        // Set animator state
        anim.SetBool("Walk", true);
        // Inform target of mingling start
        target.WaitForMingle(this);
        // Navigate to target
        navAgent.SetDestination(targetCell.transform.position);
        // Wait until near enough to the target
        do
        {
            yield return null;
        } while (navAgent.remainingDistance > MinglingDistance);
        // Set animator state
        anim.SetBool("Walk", false);
        // Display result of mingling
        Emoji.sprite = GetMingleResult(target);
        Emoji.enabled = true;
        // Wait for mingling duration
        yield return new WaitForSeconds(MinglingDuration);
        Emoji.enabled = false;
        // Inform target of mingling end
        // TODO: Unnecesary? <- Handled by  WaitForMingle
        // Start Roam coroutine
        StartCoroutine(Roam());
        // End
    }
    
    
    private Sprite GetMingleResult(NPC other) {
        if(IsAgree(other.Conversation.ActualClue)) {
            return Agree;
        } else if (IsDisagree(other.Conversation.ActualClue)) {
            return Disagree;
        } else if(IsHappy(other)) {
            return Happy;
        } else if(IsAngry(other)) {
            return Angry;
        }
        return null;
    }

    private bool IsAgree(Clue other) {
        if(Conversation.ActualClue.Identifier == other.Identifier) {
            // Descriptive
            if(Conversation.ActualClue.Identifier == ClueIdentifier.Descriptive) {
                // Talking about the same part
                if (other.NPCPartType == Conversation.ActualClue.NPCPartType) {
                    // Agreement on descriptive clue
                    if (other.NPCDescription == Conversation.ActualClue.NPCDescription) {
                        return true;
                    }
                    // Disagreement on descriptive clue
                    else {
                        return false;
                    }
                }
            }
            // Supportive or  Accusatory
            else {
                // Same target
                if(Conversation.ActualClue.Target == other.Target) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        return false;
    }
    private bool IsDisagree(Clue other) {
        if (Conversation.ActualClue.Identifier == other.Identifier) {
            // Descriptive
            if (Conversation.ActualClue.Identifier == ClueIdentifier.Descriptive) {
                // Talking about the same part
                if (other.NPCPartType == Conversation.ActualClue.NPCPartType) {
                    // Agreement on descriptive clue
                    if (other.NPCDescription == Conversation.ActualClue.NPCDescription) {
                        return false;
                    }
                    // Disagreement on descriptive clue
                    else {
                        return true;
                    }
                }
            }
            // Supportive or  Accusatory
            else {
                return false;
            }
        } else if ((Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory && other.Identifier == ClueIdentifier.Informational) ||
                    (Conversation.ActualClue.Identifier == ClueIdentifier.Informational && other.Identifier == ClueIdentifier.Accusatory)) {
            if (Conversation.ActualClue.Target == other.Target) {
                return true;
            }
        }
        return false;
    }
    public bool IsHappy(NPC other) {
        if(other.Conversation.ActualClue.Identifier == ClueIdentifier.Informational &&
            other.Conversation.ActualClue.Target == this) {
            return true;
        }
        return false;
    }
    public bool IsAngry(NPC other) {
        if (other.Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory &&
            other.Conversation.ActualClue.Target == this) {
            return true;
        }
        return false;
    }

    private IEnumerator Roam()
    {
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
        // Display result of mingling
        Emoji.sprite = GetMingleResult(other);
        Emoji.enabled = true;
        // Wait for other NPC to finish mingling
        // TODO: Is this good enough?
        yield return new WaitForSeconds(MinglingDuration);
        // Face original direction
        transform.rotation = rotation;
        Emoji.enabled = false;
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
        // Hide emoji
        Emoji.enabled = false;
        // Start interrogation routine
        StartCoroutine(CoGoToInterrogation());        
    }
    
    private IEnumerator CoGoToInterrogation()
    {
        CanMingle = false;
        // Set animator state
        anim.SetBool("Walk", true);
        // Free cell
        Grid.Instance.FreeCell(currentCell);
        currentCell = null;
        // Navigate to police box
        navAgent.SetDestination(PoliceBoxPosition);
        // Wait until target is reached
        do {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Teleport to Interrogation room
        navAgent.Warp(InterrogationPosition);
        warped = true;
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
        CanMingle = false;
        // Teleport to waiting room
        if(warped) {
            navAgent.Warp(PoliceBoxPosition);
            warped = false;
        }        
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
