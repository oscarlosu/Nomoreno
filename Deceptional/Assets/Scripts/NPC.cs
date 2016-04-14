using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Assets.Scripts;

public class NPC : MonoBehaviour, IPointerClickHandler {

    /// <summary>
    /// List of existing NPCS
    /// </summary>
    private static List<NPC> npcList;
    public static List<NPC> NPCList {
        get {
            if (NPC.npcList == null) {
                NPC.npcList = new List<NPC>();
            }
            return NPC.npcList;
        }
    }
    /// <summary>
    /// Name of the NPC
    /// </summary>
    public string Name;
    /// <summary>
    /// Determines gender of NPC
    /// </summary>
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
    public int MoodDays;
    /// <summary>
    /// Determines if NPC is killer
    /// </summary>
    public bool IsKiller;

    /// <summary>
    /// References to rendering components for the NPC's body parts
    /// </summary>
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

    /// <summary>
    /// True if the NPC is available with mingle, false otherwise
    /// </summary>
    //public bool CanMingle;
    /// <summary>
    /// Number of random selection attempts before aborting a random selection phase
    /// </summary>
    public int MaxSelectionAttempts;
    /// <summary>
    /// Currently occupied cell in the grid
    /// </summary>
    public Cell currentCell;
    /// <summary>
    /// Game object that holds the name label
    /// </summary>
    public GameObject NameLabelHolder;
    /// <summary>
    /// Controller for the name label
    /// </summary>
    private NameLabel nameLabelScritpt;

    /// <summary>
    /// References to the mingling icons
    /// </summary>
    public Sprite Distrust;
    public Sprite Trust;
    public Sprite Agree;
    public Sprite Disagree;
    public Sprite NoResult;

    /// <summary>
    /// Reference to the sprite renderer that will render the mingling result sprites
    /// </summary>
    public SpriteRenderer Emoji;
    /// <summary>
    /// True if the NPC has warped to the interrogation room, false otherwise (or after the NPC has warped bacck to the waiting room)
    /// </summary>
    private bool warped;
    public bool ShowName;
    /// <summary>
    /// Used to syncronize two NPCs when they are preparing for mingling
    /// </summary>
    public bool MingleReady;
    /// <summary>
    /// Determines how fast the NPCS rotate to face other NPCs for mingling
    /// </summary>
    public float RotationSpeed;

    #region MonoBehaviour methods
    // Use this for initialization
    void Awake() {
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        nameLabelScritpt = NameLabelHolder.GetComponent<NameLabel>();
        Mood = false;
        NPC.NPCList.Add(this);
    }

    void OnEnable() {
        ShowName = true;
        //CanMingle = true;
        warped = false;
        // Place npc on random empty position
        Grid.Instance.FreeCell(currentCell);
        currentCell = Grid.Instance.GetRandomCell();
        //transform.position = currentCell.transform.position;
        navAgent.Warp(currentCell.transform.position);

        NameLabelHolder.transform.GetComponentInChildren<TextMesh>().text = Name;
        //HideNameLabel();
        // NPCs always start waiting
        //StartCoroutine(Waiting());
    }

    void OnDisable() {
        //CanMingle = false;
        StopAllCoroutines();
    }

    void OnDestroy() {
        StopAllCoroutines();
        NPC.NPCList.Remove(this);
    }
    #endregion

    public void OnPointerClick(PointerEventData eventData) {
        PlayerController.Instance.SelectedNPC = this;
    }

    public void Assemble(NPCPart head, NPCPart torso, NPCPart legs) {
        // Save parts
        Head = head;
        Torso = torso;
        Legs = legs;
        // Load meshes and materials
        // Head
        //HeadMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Head.Type.ToString());
        HeadRenderer.material = Resources.Load<Material>("Materials/" + Head.Description.ToString());
        // Torso
        //TorsoMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Torso.Type.ToString());
        TorsoRenderer.material = Resources.Load<Material>("Materials/" + Torso.Description.ToString());
        // Legs
        //LegsMeshFilter.mesh = Resources.Load<Mesh>("Models/" + this.Legs.Type.ToString());
        LegsRenderer.material = Resources.Load<Material>("Materials/" + Legs.Description.ToString());
    }

    public void CoolMood() {
        if (Mood)
            if (MoodDays <= 0) {
                Mood = false;
            } else {
                MoodDays--;
            }
    }

    public void ShowNameLabel() {
        NameLabelHolder.SetActive(true);
    }
    public void HideNameLabel() {
        NameLabelHolder.SetActive(false);
    }

    private IEnumerator HandleWalkAnimation() {
        // Start walk animation        
        anim.SetBool("Walk", true);
        // Wait for agent to reach destination
        do {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Stop walk animation
        anim.SetBool("Walk", false);
    }

    private bool RotateTowards(Transform target) {
        // Update rotation
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
        // Update rotation of name label
        nameLabelScritpt.UpdateRotation();
        // Return true if target reached
        return Mathf.Approximately(Quaternion.Angle(transform.rotation, lookRotation), 0.0f);
    }

    private bool RotateTowards(Quaternion targetRotation) {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        // Update rotation of name label
        nameLabelScritpt.UpdateRotation();
        // Return true if target reached
        return Mathf.Approximately(Quaternion.Angle(transform.rotation, targetRotation), 0.0f);
    }


    private Sprite GetMingleResult(NPC other) {
        if (IsAgree(other.Conversation.ActualClue)) {
            return Agree;
        } else if (IsDisagree(other.Conversation.ActualClue)) {
            return Disagree;
        } else if (IsHappy(other)) {
            return Trust;
        } else if (IsAngry(other)) {
            return Distrust;
        } else {
            return NoResult;
        }
    }

    #region Mingling results
    private bool IsAgree(Clue other) {
        if (Conversation.ActualClue.Identifier == other.Identifier) {
            // Descriptive
            if (Conversation.ActualClue.Identifier == ClueIdentifier.Descriptive) {
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
                if (Conversation.ActualClue.Target == other.Target) {
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
        if ((other.Conversation.ActualClue.Identifier == ClueIdentifier.Informational &&
            other.Conversation.ActualClue.Target == this) ||
            (Conversation.ActualClue.Identifier == ClueIdentifier.Informational &&
            Conversation.ActualClue.Target == other)) {
            return true;
        }
        return false;
    }
    public bool IsAngry(NPC other) {
        if ((other.Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory &&
            other.Conversation.ActualClue.Target == this) ||
            (Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory &&
            Conversation.ActualClue.Target == other)) {
            return true;
        }
        return false;
    }
    #endregion

    #region Behaviour bootstrappers
    public void WaitForMingle(NPC other) {
        CurrentBehaviour = Behaviour.MingleWaiting;
        // Interrupt other coroutines
        StopAllCoroutines();
        // Start WaitForMingle coroutine
        StartCoroutine(CoWaitForMingle(other));
    }

    public void GoToInterrogation() {
        // Interrupt other coroutines
        StopAllCoroutines();
        // Hide emoji
        Emoji.enabled = false;
        // Start interrogation routine
        StartCoroutine(CoGoToInterrogation());
    }

    public void GoToWaiting() {
        // Interrupt other coroutines 
        StopAllCoroutines();
        // Start GoToWaiting coroutine
        StartCoroutine(CoGoToWaiting());
    }

    public void ChangeBehaviour() {
        if (CurrentBehaviour == Behaviour.Mingling) {
            CurrentBehaviour = Behaviour.Waiting;
        }
        Emoji.enabled = false;
        if (!(CurrentBehaviour == Behaviour.MingleWaiting)) {
            if (MinglingDirector.Instance.IsMingler()) StartCoroutine(Mingle());
            else StartCoroutine(Roam());
            //if (CurrentBehaviour != Behaviour.Moving && CanMingle == true) {
            //    StartCoroutine(Roam());
            //}
        }
    }
    #endregion

    private enum Behaviour { Waiting, Moving, Mingling, MingleWaiting, Interrogated }
    private Behaviour CurrentBehaviour = Behaviour.Waiting;
    public bool CanMingle { get { return CurrentBehaviour == Behaviour.Waiting || CurrentBehaviour == Behaviour.Moving; } }
    // Replace with Task architecture.
    #region Behaviour coroutines
    //private IEnumerator Waiting() {
    //    CurrentBehaviour = Behaviour.Waiting;
    //    CanMingle = true;
    //    while (true) {
    //        //// Switch to Roam or Mingle?
    //        //if (Random.Range(0.0f, 1.0f) < BehaviourChangeChance) {
    //        //    // Select Roam or Mingle
    //        //    if (Random.Range(0.0f, 1.0f) < MingleRoamChanceRatio) {
    //        //        StartCoroutine(Mingle());
    //        //    } else {
    //        //        StartCoroutine(Roam());
    //        //    }
    //        //    yield break;
    //        //}
    //        // Wait for one second
    //        yield return new WaitForSeconds(1.0f);
    //    }
    //}

    private IEnumerator Mingle() {
        //CanMingle = false;
        if (Conversation == null) { yield break; }

        // Select all potential targets.
        List<NPC> targets = MinglingDirector.Instance.RequestMinglingTargets(this);
        Debug.Log(Name + " has " + targets.Count + " potential mingle targets");
        //for (int i = 0; i < MaxSelectionAttempts; ++i) {
        //    if (target.CanMingle) {
        //        found = true;
        //        break;
        //    }
        //    //index = Random.Range(0, NPC.NPCList.Count);
        //    //target = NPC.NPCList[index];
        //    target = MinglingDirector.Instance.RequestMinglingTarget(this);
        //}

        // Find a suitable target.
        NPC target = null;
        foreach (NPC npc in targets) {
            if (npc.CanMingle) {
                target = npc;
                break;
            }
        }

        // If no suitable target, return.
        bool found = target != null;
        if (!found) {
            //StartCoroutine(Waiting());
            yield break;
        }

        // Try to get free adjacent cell
        Cell targetCell = null;
        found = false;
        foreach (Cell c in target.currentCell.Adjacent) {
            if (c.Free) {
                targetCell = c;
                Grid.Instance.TakeCell(targetCell);
                break;
            }
        }

        // If no suitable adjecent cell, return.
        found = targetCell != null;
        if (!found) {
            //StartCoroutine(Waiting());
            yield break;
        }
        Debug.Log(Name + " found mingle target: " + target.Name);

        // Set animator state
        anim.SetBool("Walk", true);
        // Inform target of mingling start
        target.WaitForMingle(this);
        // Navigate to target
        navAgent.SetDestination(targetCell.transform.position);
        MinglingDirector.Instance.DecrementMinglers();
        CurrentBehaviour = Behaviour.Mingling;
        // Wait until near enough to the target
        do {
            yield return null; // Replace with WaitUntil(Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        target.MingleReady = true;
        // Face other NPC
        while (!RotateTowards(target.transform)) {
            yield return null; // Replace with WaitUntil(RotateTowards(target.transform));
        }
        // Set animator state
        anim.SetBool("Walk", false);
        // Display result of mingling
        Emoji.sprite = GetMingleResult(target);
        if (!Emoji.sprite.Equals(NoResult)) Emoji.enabled = true;
        // Wait for mingling duration
        //yield return new WaitForSeconds(MinglingDuration);
        yield return new WaitUntil(() => CurrentBehaviour != Behaviour.Mingling);
        Emoji.enabled = false;
        // Inform target of mingling end
        // TODO: Unnecesary? <- Handled by  WaitForMingle
        // Start Roam coroutine
        StartCoroutine(Roam());
        // End
    }

    private IEnumerator Roam() {
        //CanMingle = true;
        // Select destination
        //Vector3 result = RandomVector3(WaitingRoomMin, WaitingRoomMax);
        Grid.Instance.FreeCell(currentCell);
        currentCell = Grid.Instance.GetRandomCell();
        navAgent.SetDestination(currentCell.transform.position);
        CurrentBehaviour = Behaviour.Moving;
        // Set animator state
        anim.SetBool("Walk", true);
        // Wait until target is reached
        do {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Walk", false);
        // Start Waiting coroutine
        //StartCoroutine(Waiting());
        // End
    }

    private IEnumerator CoWaitForMingle(NPC other) {
        anim.SetBool("Walk", false);
        //CanMingle = false;
        MingleReady = false;
        // Wait for other NPC to get near
        while (!MingleReady) {
            yield return null;
        }
        // Face other NPC
        // Store original rotation
        Quaternion originalRotation = transform.rotation;
        //transform.LookAt(other.transform);
        while (!RotateTowards(other.transform)) {
            yield return null;
        }
        // Set behaviour to mingling.
        CurrentBehaviour = Behaviour.Mingling;
        // Display result of mingling
        Emoji.sprite = GetMingleResult(other);
        if (!Emoji.sprite.Equals(NoResult)) Emoji.enabled = true;
        //yield return new WaitForSeconds(MinglingDuration);
        yield return new WaitUntil(() => CurrentBehaviour != Behaviour.Mingling);
        // Hide emoji
        Emoji.enabled = false;
        // Face original direction
        //transform.rotation = originalRotation;
        while (!RotateTowards(originalRotation)) {
            yield return null;
        }
        // Start Waiting coroutine
        //StartCoroutine(Waiting());
        // End
    }

    private IEnumerator CoGoToInterrogation() {
        //CanMingle = false;
        // Set animator state
        anim.SetBool("Walk", true);
        // Free cell
        Grid.Instance.FreeCell(currentCell);
        currentCell = null;
        // Navigate to police box
        navAgent.SetDestination(PoliceBoxPosition);
        CurrentBehaviour = Behaviour.Moving;
        // Wait until target is reached
        do {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Teleport to Interrogation room
        navAgent.Warp(InterrogationPosition);
        warped = true;
        // Hide name label
        HideNameLabel();
        // Set animation state
        anim.SetBool("Walk", false);
        CurrentBehaviour = Behaviour.Interrogated;
        // Inform Player Controller of arrival
        PlayerController.Instance.DisplayConversation();
        yield return null;
    }

    private IEnumerator CoGoToWaiting() {
        //CanMingle = false;
        // Teleport to waiting room
        if (warped) {
            navAgent.Warp(PoliceBoxPosition);
            warped = false;
        }
        ShowNameLabel();
        currentCell = Grid.Instance.GetRandomCell();
        navAgent.SetDestination(currentCell.transform.position);
        CurrentBehaviour = Behaviour.Moving;
        // Set animation state
        anim.SetBool("Walk", true);
        // Navigate to waiting room
        navAgent.SetDestination(currentCell.transform.position);
        // Wait until target is reached
        do {
            yield return null;
        } while (!Mathf.Approximately(navAgent.remainingDistance, 0.0f));
        // Set animation state
        anim.SetBool("Walk", false);
        // Start Waiting coroutine
        //StartCoroutine(Waiting());
        // End
    }
    #endregion
}
