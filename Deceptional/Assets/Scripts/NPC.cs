using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Assets.Scripts;
using System.Linq;
using System;

public class NPC : MonoBehaviour, IPointerDownHandler {

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
        set {
            npcList = value;
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
    public bool Mood;
    public int MoodDays;
    /// <summary>
    /// Determines if NPC is killer
    /// </summary>
    public bool IsKiller;

    /// <summary>
    /// References to rendering components for the NPC's body parts
    /// </summary>
    //public MeshFilter HeadMeshFilter;
    //public MeshRenderer HeadRenderer;
    //public MeshFilter TorsoMeshFilter;
    //public MeshRenderer TorsoRenderer;
    //public MeshFilter LegsMeshFilter;
    //public MeshRenderer LegsRenderer;

    
    public SkinnedMeshRenderer headRenderer;
    public SkinnedMeshRenderer hatRenderer;
    public SkinnedMeshRenderer torsoRenderer;
    public SkinnedMeshRenderer legsRenderer_m;
    public SkinnedMeshRenderer legsRenderer_f;
    public SkinnedMeshRenderer itemRenderer;

    public NPCPart Hat { get; set; }
    public NPCPart Torso { get; set; }
    public NPCPart Legs { get; set; }
    public NPCPart Item { get; set; }

    private NavMeshAgent navAgent;
    public Vector3 InterrogationPosition;
    public Vector3 PoliceBoxPosition;
    private Animator anim;

    //private NPC mingleTarget;
    public float BehaviourChangeChance;

    public float MinglingDistance;
    public float MinglingDuration;
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
    private NameLabel nameLabelScript;

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
    //public bool MingleReady;
    /// <summary>
    /// Determines how fast the NPCS rotate to face other NPCs for mingling
    /// </summary>
    public float RotationSpeed;



    public enum Behaviour {
        None,
        Waiting,
        Moving,
        MingleReady,
        Mingling,
        MingleWaiting,
        Interrogated
    };
    public Behaviour CurrentBehaviour = Behaviour.None;
    public bool CanMingle {
        get {
            if(CurrentBehaviour == Behaviour.None && currentCell != null && currentCell.Adjacent.Exists(cell => cell.Free)) {
                return true;
            }
            return false;
        }
    }


    #region MonoBehaviour methods
    // Use this for initialization
    void Awake() {
        // Get references to components
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        nameLabelScript = NameLabelHolder.GetComponent<NameLabel>();
        Mood = false;
        NPC.NPCList.Add(this);
    }

    void OnEnable() {
        ShowName = true;
        //CanMingle = true;
        warped = false;
        // Place npc on random empty position
        //Grid.Instance.FreeCell(currentCell);
        
        //currentCell = Grid.Instance.GetRandomCell();
        currentCell = Grid.Instance.GetIsolatedCell();




        //transform.position = currentCell.transform.position;
        //navAgent.Warp(currentCell.transform.position);
        if (!navAgent.enabled) {
            Debug.Log(Name + " was in the cage");
        }
        navAgent.enabled = false;
        transform.position = currentCell.transform.position;
        navAgent.enabled = true;

        NameLabelHolder.transform.GetComponentInChildren<UnityEngine.UI.Text>().text = Name;
        ShowNameLabel();

        Emoji.enabled = false;
        CurrentBehaviour = Behaviour.None;
        //HideNameLabel();
        // NPCs always start waiting
        //StartCoroutine(Waiting());
    }

    void OnDisable() {
        //CanMingle = false;
        StopAllCoroutines();
        if (Grid.Instance != null) {
            Grid.Instance.FreeCell(currentCell);
        }
        currentCell = null;
    }

    void OnDestroy() {
        StopAllCoroutines();
        NPC.NPCList.Remove(this);
        // No need to clear if there is no grid in the scene
        if(Grid.Instance != null) {
            Grid.Instance.FreeCell(currentCell);
        }        
        currentCell = null;
    }
    #endregion

    public void OnPointerDown(PointerEventData eventData) {
        PlayerController.Instance.SelectedNPC = this;
    }

    public void Assemble(NPCPart hat, NPCPart torso, NPCPart legs) {
        Assemble(hat, torso, legs, null);
    }

    public void Assemble(NPCPart hat, NPCPart torso, NPCPart legs, NPCPart item) {
        // Save parts
        Hat = hat;
        Torso = torso;
        Legs = legs;
        Item = item;
        // Load meshes and materials
        string gender_suffix = (IsMale ? "_m" : "_f");
        string material_suffix = "_mat";
        // Head
        headRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + "head" + gender_suffix);
        headRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + "head" + gender_suffix + material_suffix);
        // Hat
        hatRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + Hat.GetFileName() + gender_suffix);
        hatRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + Hat.GetFileName() + gender_suffix + material_suffix);
        // Torso
        torsoRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + Torso.GetFileName() + gender_suffix);
        torsoRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + Torso.GetFileName() + gender_suffix + material_suffix);
        // Legs
        if(IsMale) {
            // Load male legs
            legsRenderer_m.sharedMesh = Resources.Load<Mesh>("Models/" + Legs.GetFileName() + gender_suffix);
            legsRenderer_m.sharedMaterial = Resources.Load<Material>("Materials/" + Legs.GetFileName() + gender_suffix + material_suffix);
            // Set female to null
            legsRenderer_f.sharedMesh = null;
            legsRenderer_f.sharedMaterial = null;
            // Disable female
            legsRenderer_f.enabled = false;
        } else {
            // Load female legs
            legsRenderer_f.sharedMesh = Resources.Load<Mesh>("Models/" + Legs.GetFileName() + gender_suffix);
            legsRenderer_f.sharedMaterial = Resources.Load<Material>("Materials/" + Legs.GetFileName() + gender_suffix + material_suffix);
            // Set male to null
            legsRenderer_m.sharedMesh = null;
            legsRenderer_m.sharedMaterial = null;
            // Disable male
            legsRenderer_m.enabled = false;
        }
        
        // Item
        if(Item != null) {
            itemRenderer.sharedMesh = Resources.Load<Mesh>("Models/" + Item.GetFileName() + gender_suffix);
            itemRenderer.sharedMaterial = Resources.Load<Material>("Materials/" + Item.GetFileName() + gender_suffix + material_suffix);
        }       

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
        } while (!HasReachedDestination());
        // Stop walk animation
        anim.SetBool("Walk", false);
    }

    private bool RotateTowards(Transform target) {
        // Update rotation
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
        // Return true if target reached
        return Mathf.Approximately(Quaternion.Angle(transform.rotation, lookRotation), 0.0f);
    }

    private bool RotateTowards(Quaternion targetRotation) {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        // Update rotation of name label
        nameLabelScript.UpdateRotation();
        // Return true if target reached
        return Mathf.Approximately(Quaternion.Angle(transform.rotation, targetRotation), 0.0f);
    }

    private bool HasReachedDestination() {
        float dist = navAgent.remainingDistance;
        if (dist != Mathf.Infinity && 
            navAgent.pathStatus == NavMeshPathStatus.PathComplete && 
            Mathf.Approximately(navAgent.remainingDistance, 0.0f)) {
            return true;
        }
        return false;
    }
    

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

    public void WaitOrRoam() {   
        if(CurrentBehaviour != Behaviour.None) {
            return;
        }
        if(PlayerController.Instance.Rng.Next(0, 2) == 0) {
            Wait();
        } else {
            StartCoroutine(Roam());
        }
    }

    public bool AttemptMingle() {
        if(CurrentBehaviour != Behaviour.None) {
            return false;
        }
        List<NPC> targets = MinglingDirector.Instance.RequestMinglingTargets(this);
        if(targets.Count > 0) {
            CurrentBehaviour = Behaviour.Mingling;
            StartCoroutine(Mingle(targets));
            return true;
        }
        else {
            return false;
        }
    }
    #endregion

    
    // Replace with Task architecture.
    #region Behaviour coroutines
    private void Wait() {
        CurrentBehaviour = Behaviour.Waiting;
        // Set idle animation
    }

    private IEnumerator Mingle(List<NPC> targets) {
        if (Conversation == null) { yield break; }
        // Select random target
        NPC target = targets[PlayerController.Instance.Rng.Next(0, targets.Count)];
        Cell targetCell = target.currentCell.Adjacent.Find(cell => cell.Free);

        Debug.Log(Name + " mingles with " + target.Name);


        // Change state
        CurrentBehaviour = Behaviour.Mingling;
        // Set animator state
        anim.SetBool("Walk", true);
        // Inform target of mingling start
        target.WaitForMingle(this);
        // Navigate to target
        Grid.Instance.FreeCell(currentCell);
        currentCell = null;
        Grid.Instance.TakeCell(targetCell);
        currentCell = targetCell;
        navAgent.SetDestination(targetCell.transform.position);
        yield return null;   
        // Wait until near enough to the target
        yield return new WaitUntil(() => this.HasReachedDestination());
        //while(!Mathf.Approximately(navAgent.remainingDistance, 0.0f)) {
        //    yield return null;
            
        //}
        //Debug.Log("Reached target");
        target.CurrentBehaviour = Behaviour.MingleReady;
        // Set animator state
        anim.SetBool("Walk", false);
        // Face other NPC
        while (!RotateTowards(target.transform)) {
            yield return null; // Replace with WaitUntil(RotateTowards(target.transform));
        }        
        // Display result of mingling
        Emoji.sprite = GetMingleResult(target);
        Emoji.enabled = true;
    }

    private IEnumerator Roam() {
        // Set behaviour
        CurrentBehaviour = Behaviour.Moving;
        // Select destination
        Grid.Instance.FreeCell(currentCell);
        currentCell = null;
        //currentCell = Grid.Instance.GetRandomCell();
        currentCell = Grid.Instance.GetIsolatedCell();
        navAgent.SetDestination(currentCell.transform.position);
        // Set animator state
        anim.SetBool("Walk", true);
        // Wait until target is reached
        do {
            yield return null;
        } while (!HasReachedDestination());
        // Set animation state
        anim.SetBool("Walk", false);
    }

    private IEnumerator CoWaitForMingle(NPC other) {
        // Set behaviour to MingleWaiting
        CurrentBehaviour = Behaviour.MingleWaiting;


        Debug.Log(Name + " receives mingle from " + other.Name);
        // Start animation
        anim.SetBool("Walk", false);
        //MingleReady = false;
        // Wait for other NPC to get near
        // Wait for distance to be lower than threshold, instead of mingling signal from initiator?
        while (CurrentBehaviour != Behaviour.MingleReady) {
            yield return null;
        }
        //yield return new WaitUntil(() => CurrentBehaviour == Behaviour.MingleReady);
        // Face other NPC
        // Store original rotation
        //Quaternion originalRotation = transform.rotation;
        //transform.LookAt(other.transform);
        while (!RotateTowards(other.transform)) {
            yield return null;
        }
        // Set behaviour to mingling.
        //CurrentBehaviour = Behaviour.Mingling;
        // Display result of mingling
        Emoji.sprite = GetMingleResult(other);
        if (Emoji.sprite.Equals(NoResult)) {
            Debug.LogWarning("No result in mingle with " + Name);

        }

        Emoji.enabled = true;
        //yield return new WaitForSeconds(MinglingDuration);
        //yield return new WaitUntil(() => CurrentBehaviour != Behaviour.Mingling);
        //// Hide emoji
        //Emoji.enabled = false;
        //// Face original direction
        ////transform.rotation = originalRotation;
        //while (!RotateTowards(originalRotation)) {
        //    yield return null;
        //}
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
        } while (!HasReachedDestination());
        // Teleport to Interrogation room
        navAgent.Warp(InterrogationPosition);
        warped = true;
        // Hide name label
        HideNameLabel();
        // Set animation state
        anim.SetBool("Walk", false);
        CurrentBehaviour = Behaviour.Interrogated;

        // Inform Player Controller of arrival
		PlayerController.Instance.HandleNPCReachedInterrogation();
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

        //currentCell = Grid.Instance.GetRandomCell();
        currentCell = Grid.Instance.GetIsolatedCell();


        CurrentBehaviour = Behaviour.Moving;
        // Set animation state
        anim.SetBool("Walk", true);
        // Navigate to waiting room
        navAgent.SetDestination(currentCell.transform.position);
        // Wait until target is reached
        do {
            yield return null;
        } while (!HasReachedDestination());
        // Set animation state
        anim.SetBool("Walk", false);
        // Start Waiting coroutine
        //StartCoroutine(Waiting());
        // End
    }
    #endregion



    #region Mingling results
    private Sprite GetMingleResult(NPC other) {
        if (IsAgree(other.Conversation.ActualClue)) {
            return Agree;
        } else if (IsDisagree(other.Conversation.ActualClue)) {
            return Disagree;
        //} else if (IsHappy(other)) {
        //    return Trust;
        } else if (IsAngry(other)) {
            return Distrust;
        } else {
            return NoResult;
        }
    }   
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
                if (Conversation.ActualClue.Targets.Any(npc => other.Targets.Any(oNPC => npc == oNPC))) {
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
        // Cannot reasonably be converted to match new statements.
        //} else if ((Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory && other.Identifier == ClueIdentifier.Informational) ||
        //            (Conversation.ActualClue.Identifier == ClueIdentifier.Informational && other.Identifier == ClueIdentifier.Accusatory)) {
        //    if (Conversation.ActualClue.Target == other.Target) {
        //        return true;
        //    }
        }
        return false;
    }
    //public bool IsHappy(NPC other) {
    //    if ((other.Conversation.ActualClue.Identifier == ClueIdentifier.Informational &&
    //        other.Conversation.ActualClue.Target == this) ||
    //        (Conversation.ActualClue.Identifier == ClueIdentifier.Informational &&
    //        Conversation.ActualClue.Target == other)) {
    //        return true;
    //    }
    //    return false;
    //}
    public bool IsAngry(NPC other) {
        if ((other.Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory &&
            other.Conversation.ActualClue.Targets.Contains(this)) ||
            (Conversation.ActualClue.Identifier == ClueIdentifier.Accusatory &&
            Conversation.ActualClue.Targets.Contains(other))) {
            return true;
        }
        return false;
    }
    #endregion
}
