using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    public class PlayerController : MonoBehaviour {
        /// <summary>
        /// Provides a singleton access.
        /// Since we want to be able to define parameters in the inspector, 
        /// this class is a Monobehaviour as well and there should be a 
        /// Game Object in the scene with this script and the appropriate parameter
        /// values set.
        /// </summary>
        private static PlayerController instance = null;
        public static PlayerController Instance {
            get {
                if (instance == null) {
                    instance = (PlayerController)FindObjectOfType(typeof(PlayerController));
                    if (instance == null)
                        instance = (new GameObject("PlayerController")).AddComponent<PlayerController>();
                }
                return instance;
            }
        }

        public int GeneratorSeed;
        public bool UseFixedSeed;

        public static GameObject NPCParent;

        #region Instance fields & properties
        public GameObject DefaultNPC;
        private NPC cit;
        public NPC CurrentInterrogationTarget {
            get {
                return cit;
            }
            set {
                cit = value;
            }
        }
        private NPC snpc;
        public NPC SelectedNPC {
            get {
                return snpc;
            }
            set {
                if(value == null) {
                    SelectionSpotlight.gameObject.SetActive(false);
                    SelectionSpotlight.parent = null;
                }
                snpc = value;
                if(snpc != null) {
                    SelectionSpotlight.parent = snpc.transform;
                    SelectionSpotlight.localPosition = new Vector3(0, SelectionSpotlightYOffset, 0);
                    SelectionSpotlight.gameObject.SetActive(true);
                }
            }
        }
        private NPC arrestedNPC;
        public bool UseRealTime;
        private bool timeRunning;
        public int DailyInteracions;
        public float DayDuration; // In seconds
        public int NumberOfNPCS;
        public int NumberOfDescriptiveClues;
        

        public ButtonController CallInButton;
        //public GameObject DismissButton;
        public GameObject NPCS;
        public TextMesh NewDayTextMesh;
        public TextMesh StatementTextMesh;
        public Transform ClockHourHandle;
        public Transform ClockMinuteHandle;
        public GameObject NewDayLabelHolder;

        private int interactionCount;
        public float currentTime;
        private int currentDay;

        public int NextDayDelay;
        public int PreNextDayDelay;

        public Camera InterrogationRoomCamera;
        public Transform SelectionSpotlight;

        private List<Coroutine> conversationCoroutines;
        public float PercentageLiars;
        public TextMesh NameText;
        public float SelectionSpotlightYOffset;

        #endregion

        #region Instance methods

        public void Awake() {
            //Hack to make sure currentTIme is DayDuration
            currentTime = DayDuration;
            conversationCoroutines = new List<Coroutine>();
            currentDay = 0;
            // Initialize NPCParent
            PlayerController.NPCParent = GameObject.FindGameObjectWithTag("NPCParent");
            // Disable NPCParent
            PlayerController.NPCParent.SetActive(false);
            // Generate NPCs 
            NPCHandler.GenerateMultipleWitnesses(NumberOfNPCS);
            // Create MinglingDirector;
            var dummy = MinglingDirector.Instance;

            // Generate new day
            StartCoroutine(NextDay());
        }

        public void Update() {
            HandleButtons();
            if (UseRealTime && timeRunning) UpdateRealTime();
        }

        private double oneSecond = 0;
        private void UpdateRealTime() {
            currentTime -= Time.deltaTime;
            oneSecond += Time.deltaTime;
            if (oneSecond >= 1.0) {
                oneSecond -= 1; // Animate minute hand.
                //AnimateRealClock();
                StartCoroutine(AnimateRealClock());
            }

            if (currentTime <= 0 && timeRunning) {
                timeRunning = false;
                StartCoroutine(NextDay());
            }
        }

        private void HandleButtons() {
            if(CurrentInterrogationTarget == null) {
                // Show call in
                CallInButton.ChangeButton("CALL IN", "CallIn");
            } else if(SelectedNPC == null) {
                // Show dismiss
                CallInButton.ChangeButton("DISMISS", "Dismiss");
            } else if(CurrentInterrogationTarget == SelectedNPC) {
                // Show dismiss
                CallInButton.ChangeButton("DISMISS", "Dismiss");
            } else if(CurrentInterrogationTarget != SelectedNPC) {
                // Show call in
                CallInButton.ChangeButton("CALL IN", "CallIn");
            }
        }

        public void CallIn() {
            if (SelectedNPC == null || SelectedNPC == CurrentInterrogationTarget) return;
            // Dismiss whoever is in the interrogation room
            Dismiss();
            // Set current interrogation target
            CurrentInterrogationTarget = SelectedNPC;
            // Make NPC go to interrogation room
            CurrentInterrogationTarget.GoToInterrogation();
        }
        
        public void DisplayConversation() {
            // Display name on wall
            NameText.text = CurrentInterrogationTarget.Name + " says:";
            // Get statement and break into lines
            string statement = string.Empty;
            if (CurrentInterrogationTarget.Mood) {
                statement = CurrentInterrogationTarget.Conversation.MoodyMessage;
            } else {
                statement = CurrentInterrogationTarget.Conversation.ActualClue.Statement;
            }

            statement = TextWrapper.BreakLine(statement);
            StatementTextMesh.gameObject.SetActive(true);
            Coroutine inst = StartCoroutine(CoDisplayText(statement, StatementTextMesh));
            conversationCoroutines.Add(inst);
        }

        public void DisplayConversation(string prefix) {
            NameText.text = CurrentInterrogationTarget.Name + " says:";
            var statement = string.Empty;
            if (CurrentInterrogationTarget.Mood) {
                statement = CurrentInterrogationTarget.Conversation.MoodyMessage;
            } else {
                statement = prefix + CurrentInterrogationTarget.Conversation.ActualClue.Statement;
            }

            statement = TextWrapper.BreakLine(statement);
            StatementTextMesh.gameObject.SetActive(true);
            Coroutine inst = StartCoroutine(CoDisplayText(statement, StatementTextMesh));
            conversationCoroutines.Add(inst);
        }

        private IEnumerator CoDisplayText(string text, TextMesh textField) {            
            // Show letters one at a time
            textField.text = "";
            for(int i = 0; i < text.Length; ++i) {
                textField.text += text[i];
                yield return null;
            }            
        }

        private void HideConversation() {
            for(int i = conversationCoroutines.Count - 1; i >= 0; --i) {
                StopCoroutine(conversationCoroutines[i]);
                conversationCoroutines.RemoveAt(i);
            }
            NameText.text = "";
            StatementTextMesh.text = "";
            StatementTextMesh.gameObject.SetActive(false);
        }

        public void Arrest() {
            if(CurrentInterrogationTarget != null) {
                if (UseRealTime) timeRunning = false;
                // Check if the acccused NPC is the killer
                if (CurrentInterrogationTarget.IsKiller) {
                    // Victory
                    NewDayTextMesh.text = "On day " + currentDay + " the detective\n caught the murderer:\n" + CurrentInterrogationTarget.Name + "\n\nPress any key to restart";
                    NewDayLabelHolder.SetActive(true);
                    StartCoroutine(WaitForRestart());
                } else if (NPC.NPCList.Count <= 1) {
                    // Game Over
                    NewDayTextMesh.text = "On day " + currentDay + " the detective\n hadn't arrested the murderer\n but since everyone else was dead, \nthere was no real need anymore...";
                    NewDayLabelHolder.SetActive(true);
                    StartCoroutine(WaitForRestart());
                } else { 
                    // Make NPC angry
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                    // Save reference to arrested NPC
                    arrestedNPC = CurrentInterrogationTarget;
                    // Deselect current interragation target. This prevents the player from triggering next day several times by spamming the arrest button
                    Dismiss();
                    // Start new day
                    StartCoroutine(NextDay());
                }
                
            }            
        }

        public void Accuse() {
            if (CurrentInterrogationTarget != null) {
                // Count interaction and update time.
                UpdateTime();
                // Hide text
                HideConversation();
                // Make NPC angry if you wrongfully accuse them of lying
                if (!CurrentInterrogationTarget.Conversation.Next(false)) {
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                }
                // Display next text lerping it
                DisplayConversation("Okay, okay, you got me... ");
            }
        }

        public void Dismiss() {
            if(CurrentInterrogationTarget != null) {
                CurrentInterrogationTarget.GoToWaiting();
                CurrentInterrogationTarget = null;
                HideConversation();
                UpdateTime();
                if (!UseRealTime && interactionCount == 0)
                    StartCoroutine(NextDay());
            }            
        }

        private void UpdateTime() {
            // Update interactions
            interactionCount--;
            // Animate clock
            if (!UseRealTime) StartCoroutine(AnimateClock());
        }

        private IEnumerator AnimateClock() {
            Vector3 hourRotation = ClockHourHandle.localEulerAngles;
            Vector3 minuteRotation = ClockMinuteHandle.localEulerAngles;
            // We want to rotate the hour handle to point to the next hour
            // We also want the minute handle to rotate 360 degrees        
            while (minuteRotation.y < 360.0f) {
                hourRotation.y += (30.0f * Time.deltaTime);
                ClockHourHandle.localEulerAngles = hourRotation;
                minuteRotation.y += (360.0f * Time.deltaTime);
                ClockMinuteHandle.localEulerAngles = minuteRotation;
                yield return null;
            }
            hourRotation.y = (int)hourRotation.y;
            ClockHourHandle.localEulerAngles = hourRotation;
            minuteRotation.y = 0;
            ClockMinuteHandle.localEulerAngles = minuteRotation;
        }

        private IEnumerator AnimateRealClock() {
            Vector3 hourRotation = ClockHourHandle.localEulerAngles;
            Vector3 minuteRotation = ClockMinuteHandle.localEulerAngles;
            // We want to rotate the hour handle to rotate 1/2 a degree.
            // We also want the minute handle to rotate to next second.
            var accelerator = 600 / DayDuration;
            minuteRotation.y += 6.0f * accelerator;
            ClockMinuteHandle.localEulerAngles = minuteRotation;
            hourRotation.y += 0.5f * accelerator;
            ClockHourHandle.localEulerAngles = hourRotation;
            yield return null;
        }

        private void ResetClock() {
            Vector3 hourRotation = ClockHourHandle.localEulerAngles;
            Vector3 minuteRotation = ClockMinuteHandle.localEulerAngles;
            hourRotation.y = -60;
            ClockHourHandle.localEulerAngles = hourRotation;
            minuteRotation.y = 0;
            ClockMinuteHandle.localEulerAngles = minuteRotation;
        }
        
        private IEnumerator NextDay() {
            yield return new WaitForSeconds(PreNextDayDelay);
            if(CurrentInterrogationTarget != null) {
                Cell cell = Grid.Instance.GetRandomCell();
                CurrentInterrogationTarget.currentCell = cell;
                CurrentInterrogationTarget.transform.position = cell.transform.position;
            }
            HideConversation();
            ++currentDay;
            // Hide scene
            HideScene();
            // Reset current time and interaction count
            currentTime = DayDuration;
            interactionCount = DailyInteracions;
            // Clear references to NPCs
            CurrentInterrogationTarget = null;
            SelectedNPC = null;
            // Murder new witness
            string victimName = MurderWitness();
            // Clear reference to arrested NPC
            arrestedNPC = null;
            // Cool NPC moods
            foreach (NPC n in NPC.NPCList) { n.CoolMood(); }
            // Generate conversations
            ConversationHandler.TruthGraph = GraphBuilder.BuildRandomGraph(NPC.NPCList.Count, NumberOfDescriptiveClues);
            ConversationHandler.SetupConversations(PercentageLiars);
            // Reset clock
            ResetClock();
            // Show new day message
            string nextDayText = "Day " + currentDay + ":\n\n" + victimName + " has\n been murdered.";
            NewDayLabelHolder.SetActive(true);
            // Show letters one at a time
            StartCoroutine(CoDisplayText(nextDayText, NewDayTextMesh));
            // Wait a bit
            yield return new WaitForSeconds(NextDayDelay);
            NewDayLabelHolder.SetActive(false);
            // Show scene
            ShowScene();
            if (UseRealTime) timeRunning = true;
        }

        private IEnumerator WaitForRestart() {
            // Disable NPCS
            NPCParent.SetActive(false);
            // Clear references to NPCs
            CurrentInterrogationTarget = null;
            SelectedNPC = null;
            // Hide conversation
            HideConversation();
            // Clear NPCList
            NPC.NPCList.Clear();
            // Wait for key press
            while (!Input.anyKeyDown) {
                yield return null;
            }
            //Application.LoadLevel(0);
                 
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }


        private void HideScene() {
            NPCS.SetActive(false);
        }

        private void ShowScene() {
            NPCS.SetActive(true);
        }

        private string MurderWitness() {
            string name = "Nobody";
            // Only murder if there is more than one NPC
            if (NPC.NPCList.Count > 1) {
                // Find an NPC that is not the killer
                int index;
                NPC target;
                do {
                    //index = UnityEngine.Random.Range(0, NPC.NPCList.Count);
                    index = UseFixedSeed ? new System.Random(GeneratorSeed).Next(NPC.NPCList.Count) : new System.Random(DateTime.Now.Millisecond).Next(NPC.NPCList.Count);
                    target = NPC.NPCList[index];
                } while (NPC.NPCList[index].IsKiller || arrestedNPC == target);
                // Save victim's name
                name = target.Name;
                // Remove from list
                NPC.NPCList.RemoveAt(index);
                // Destroy game object
                Destroy(target.gameObject);
            }
            return name;
        }
        #endregion
    }
}
