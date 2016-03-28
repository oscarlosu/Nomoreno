﻿using System;
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

        public static int GeneratorSeed = 0;

        public static GameObject NPCParent;

        #region Instance fields & properties
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
                if(snpc != null) {
                    SelectionSpotlight.gameObject.SetActive(false);
                    SelectionSpotlight.parent = null;
                    SelectionSpotlight.position = new Vector3(0, SelectionSpotlight.position.y, 0);
                    snpc.HideNameLabel();
                }
                snpc = value;
                if(snpc != null) {
                    SelectionSpotlight.parent = snpc.transform;
                    SelectionSpotlight.localPosition = new Vector3(0, SelectionSpotlight.position.y, 0);
                    SelectionSpotlight.gameObject.SetActive(true);
                    snpc.ShowNameLabel();
                }
            }
        }
        public bool UseRealTime;
        public int DailyInteracions;
        public int DayDuration;
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
        private int currentTime;
        private int currentDay;

        public int NextDayDelay;
        public int PreNextDayDelay;

        public Camera InterrogationRoomCamera;
        public Transform SelectionSpotlight;

        private List<Coroutine> conversationCoroutines;
        public float PercentageLiars;
        public TextMesh NameText;

        #endregion

        #region Instance methods

        public void Awake() {
            conversationCoroutines = new List<Coroutine>();
            currentDay = 0;
            // Initialize NPCParent
            PlayerController.NPCParent = GameObject.FindGameObjectWithTag("NPCParent");
            // Disable NPCParent
            PlayerController.NPCParent.SetActive(false);
            // Generate NPCs 
            NPCHandler.GenerateMultipleWitnesses(NumberOfNPCS);
            // Generate new day
            StartCoroutine(NextDay());
            
        }

        public void Update() {
            HandleButtons();
        }
       

        private void HandleButtons() {
            if(CurrentInterrogationTarget == null) {
                // Show call in
                CallInButton.ChangeButton("Call In", "CallIn");
            } else if(SelectedNPC == null) {
                // Show dismiss
                CallInButton.ChangeButton("Dismiss", "Dismiss");
            } else if(CurrentInterrogationTarget == SelectedNPC) {
                // Show dismiss
                CallInButton.ChangeButton("Dismiss", "Dismiss");
            } else if(CurrentInterrogationTarget != SelectedNPC) {
                // Show call in
                CallInButton.ChangeButton("Call In", "CallIn");
            }
        }
        public void CallIn() {
            if (SelectedNPC == null) return;
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
                statement = "... I'm not gonna talk to you!";
            } else {
                statement = CurrentInterrogationTarget.Conversation.ActualClue.Statement;
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
                // Check if the acccused NPC is the killer
                if (CurrentInterrogationTarget.IsKiller) {
                    // Victory
                    NewDayTextMesh.text = "On day " + currentDay + " the detective\n caught the murderer:\n" + CurrentInterrogationTarget.Name + "\n\nPress any key to restart";
                    NewDayLabelHolder.SetActive(true);
                    StartCoroutine(WaitForRestart());
                } else {
                    // Make NPC angry
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                    // Deselect current interragation target. This prevents the player from triggering next day several times by spamming the arrest button
                    CurrentInterrogationTarget = null;
                    // Start new day
                    StartCoroutine(NextDay());
                }
                
            }            
        }
        public void Accuse() {
            if (CurrentInterrogationTarget != null) {                
                // Make NPC angry if you wrongfully accuse them of lying
                if (!CurrentInterrogationTarget.Conversation.Next(false)) {
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                }
                // Hide text
                HideConversation();
                // Display next text lerping it
                DisplayConversation();
            }
        }

        public void Dismiss() {
            if(CurrentInterrogationTarget != null) {
                if(SelectedNPC == CurrentInterrogationTarget) {
                    SelectedNPC.ShowName = true;
                }
                UpdateTime();
                CurrentInterrogationTarget.GoToWaiting();
                CurrentInterrogationTarget = null;
                HideConversation();
                if (UseRealTime && currentTime == 0)
                    StartCoroutine(NextDay());
                else if (!UseRealTime && interactionCount == 0)
                    StartCoroutine(NextDay());
            }            
        }

        private void UpdateTime() {
            // Update interactions
            interactionCount--;
            // Animate clock
            StartCoroutine(AnimateClock());
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
            // Cool NPC moods
            foreach (NPC n in NPC.NPCList) { n.CoolMood(); }
            // Generate conversations
            ConversationHandler.TruthGraph = GraphBuilder.BuildRandomGraph(NPC.NPCList.Count, NumberOfDescriptiveClues);
            ConversationHandler.SetupConversations(PercentageLiars);
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
        }

        private IEnumerator WaitForRestart() {
            // Disable NPCS
            NPCParent.SetActive(false);
            // Clear references to NPCs
            CurrentInterrogationTarget = null;
            SelectedNPC = null;
            // Hide conversation
            HideConversation();
            // Wait for key press
            while (!Input.anyKeyDown) {
                yield return null;
            }
            Application.LoadLevel(0);            
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
                    index = new System.Random(GeneratorSeed).Next(NPC.NPCList.Count);
                    target = NPC.NPCList[index];
                } while (NPC.NPCList[index].IsKiller);
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
