using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                    //if (instance == null)
                    //    instance = (new GameObject("PlayerController")).AddComponent<PlayerController>();
                }
                return instance;
            }
        }

        public int Seed;
        public bool UseFixedSeed;
        public System.Random Rng;

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
        public int DailyInteracions;
        public int NumberOfNPCS;
        public int NumberOfDescriptiveClues;
        public int Difficulty;
        

        public ButtonController CallInButton;
        public ButtonController PlatformButton;
        //public GameObject DismissButton;
        public GameObject NPCS;
        
        public Transform ClockHourHandle;
        public Transform ClockMinuteHandle;
        public GameObject NewDayLabelHolder;

        private int interactionCount;
        private int currentDay;

        public Camera InterrogationRoomCamera;
        public Transform SelectionSpotlight;

        private List<Coroutine> conversationCoroutines;
        public float PercentageLiars;
        public float PercentageDescriptiveLiars;

        public float SelectionSpotlightYOffset;

        public int MaxMinglers;
        public int MinMinglers;

        private string platformText;

        public UnityEngine.UI.Text PlatformTextMesh;
        public UnityEngine.UI.Text NameText;
        public UnityEngine.UI.Text StatementTextMesh;
        public UnityEngine.UI.Text CalendarTextMesh;


        public enum ControllerState {
			Disabled,
			Enabled
		}
		public ControllerState State;

        public AudioClip ClockSound;
        public float ClockSoundVolume = 1.0f;
        public AudioClip MinglingSound;
        public float MinglingSoundVolume = 1.0f;


        public AudioClip WrongAccuseSound;
        public float WrongAccuseSoundVolume = 1.0f;
        public AudioClip RightAccuseSound;
        public float RightAccuseSoundVolume = 1.0f;


        #endregion





        #region Instance methods

        public void Awake() {
			State = ControllerState.Disabled;
            // Initialize RNG
            if (!UseFixedSeed) {
                Seed = (int) DateTime.Now.Ticks;
            }
            Rng = new System.Random(Seed);
            conversationCoroutines = new List<Coroutine>();
            currentDay = 0;
            // Initialize NPCParent
            PlayerController.NPCParent = GameObject.FindGameObjectWithTag("NPCParent");
            // Disable NPCParent
            PlayerController.NPCParent.SetActive(false);
            // Generate NPCs 
            NPCHandler.GenerateMultipleWitnesses(NumberOfNPCS);
            // Create MinglingDirector;
            //var dummy = MinglingDirector.Instance;


            TransitionManager.Instance.GameTransition();            
        }

        public void CallIn() {
			if (State == ControllerState.Disabled || 
				SelectedNPC == null || CurrentInterrogationTarget != null) {
				return;
			}
			State = ControllerState.Disabled;
			CallInButton.ChangeButton("DISMISS", "Dismiss");
            // Dismiss whoever is in the interrogation room
            //Dismiss();
            // Set current interrogation target
            CurrentInterrogationTarget = SelectedNPC;
            // Make NPC go to interrogation room
            CurrentInterrogationTarget.GoToInterrogation();
        }
		public void HandleNPCReachedInterrogation() {
			State = ControllerState.Enabled;
			DisplayConversation();
		}

        public void DisplayConversation() {
            DisplayConversation(string.Empty);
        }

        public void DisplayConversation(string prefix) {
            // Display name on wall
            NameText.text = CurrentInterrogationTarget.Name + " says:";
            // Get statement and break into lines
            var statement = string.Empty;
            if (CurrentInterrogationTarget.Mood) {
                statement = CurrentInterrogationTarget.Conversation.MoodyMessage;
            } else {
                statement = prefix + CurrentInterrogationTarget.Conversation.ActualClue.Statement;
                // Add displayed clue to AIDirector list if not present already
                if (!AIDirector.Instance.DailyClues.Contains(CurrentInterrogationTarget.Conversation.ActualClue))
                    AIDirector.Instance.DailyClues.Add(CurrentInterrogationTarget.Conversation.ActualClue);
                // Add displayed hard clue to AIDirector list if not present already
                if (!AIDirector.Instance.HardClues.Contains(CurrentInterrogationTarget.Conversation.ActualClue) &&
                    CurrentInterrogationTarget.Conversation.ActualClue.Identifier == ClueIdentifier.Descriptive)
                    AIDirector.Instance.HardClues.Add(CurrentInterrogationTarget.Conversation.ActualClue);
            }

            statement = TextWrapper.BreakLine(statement, 23);
            StatementTextMesh.gameObject.SetActive(true);
            Coroutine inst = StartCoroutine(CoDisplayText(statement, StatementTextMesh));
            conversationCoroutines.Add(inst);
        }

        private IEnumerator CoDisplayText(string text, UnityEngine.UI.Text textField) {            
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

        public void PrepareArrestSuccess() {
            // Victory
            platformText = TextWrapper.BreakLine(currentDay + " people were killed before the detective caught the murderer: " + CurrentInterrogationTarget.Name);
            PlatformButton.ChangeButton("RETURN TO\nMENU", "EndGameTransition");
        }

        public void PrepareGameOver() {
            // Game Over
            platformText = TextWrapper.BreakLine("After " + currentDay + " people had been killed, the detective was fired.");
            PlatformButton.ChangeButton("RETURN TO\nMENU", "EndGameTransition");
        }
        public void ShowPlatformText() {
            // Show letters one at a time
            StartCoroutine(CoDisplayText(platformText, PlatformTextMesh));
        }
        public void ShowCalendarText() {            
            // Show letters one at a time
            StartCoroutine(CoDisplayText("Day " + currentDay + "\n\n" + "10:00 - 20:00", CalendarTextMesh));
        }

        public void Arrest() {
			if(State == ControllerState.Enabled && CurrentInterrogationTarget != null) {
                // Check if the acccused NPC is the killer
                if (CurrentInterrogationTarget.IsKiller) {
                    PrepareArrestSuccess();
                    TransitionManager.Instance.ArrestTransition(CurrentInterrogationTarget.transform, true);
                } else if (NPC.NPCList.Count <= 2) {
                    PrepareGameOver();
                    TransitionManager.Instance.ArrestTransition(CurrentInterrogationTarget.transform, true);
                } else { 
                    // Make NPC angry
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                    // Save reference to arrested NPC
                    arrestedNPC = CurrentInterrogationTarget;

                    CallInButton.ChangeButton("CALL\nIN", "CallIn");
                    TransitionManager.Instance.ArrestTransition(CurrentInterrogationTarget.transform, false);
                }                
            }            
        }

        public void Accuse() {
			if (State == ControllerState.Enabled && 
				CurrentInterrogationTarget != null && !CurrentInterrogationTarget.Mood) {
                // Count interaction and update time.
                UpdateTime();
                // Hide text
                HideConversation();
                // Make NPC angry if you wrongfully accuse them of lying
                if (!CurrentInterrogationTarget.Conversation.Next(false)) {
                    CurrentInterrogationTarget.Mood = true;
                    CurrentInterrogationTarget.MoodDays = 1;
                    AudioManager.Instance.Play(WrongAccuseSound, s => s.volume = WrongAccuseSoundVolume);
                } else {
                    AudioManager.Instance.Play(RightAccuseSound, s => s.volume = RightAccuseSoundVolume);
                }
                // Display next text lerping it
                DisplayConversation("Okay, okay, you got me... ");
            }
        }

        public void Dismiss() {
			if(State == ControllerState.Enabled && 
				CurrentInterrogationTarget != null) {
                CurrentInterrogationTarget.GoToWaiting();
                CurrentInterrogationTarget = null;

				CallInButton.ChangeButton("CALL\nIN", "CallIn");
                ExecuteMinglePhase();

                HideConversation();
                UpdateTime();
                if (interactionCount <= 0) {
                    //StartCoroutine(NextDay());
                    //TODO
                    if (NPC.NPCList.Count <= 2) {
                        PrepareGameOver();
                        TransitionManager.Instance.DayOverTransition(true);
                    } else {
                        TransitionManager.Instance.DayOverTransition(false);
                    }
                        
                }
                    
            }            
        }


        private void ExecuteMinglePhase() {
            AudioManager.Instance.Play(MinglingSound, s => s.volume = MinglingSoundVolume);
            // Reset States
            for (int i = 0; i < NPC.NPCList.Count; ++i) {
                if(NPC.NPCList[i].CurrentBehaviour != NPC.Behaviour.ReturningFromInterrogation) {
                    NPC.NPCList[i].CurrentBehaviour = NPC.Behaviour.None;
                    NPC.NPCList[i].Emoji.enabled = false;
                }
                
            }

            // Shuffles NPCList
            List<NPC> shuffledList = new List<NPC>();
            while(NPC.NPCList.Count > 0) { 
                int index = Rng.Next(0, NPC.NPCList.Count);
                shuffledList.Add(NPC.NPCList[index]);
                NPC.NPCList.RemoveAt(index);
            }
            NPC.NPCList = shuffledList;

            // Select minglers
            int nMinglers = Rng.Next(PlayerController.Instance.MinMinglers, PlayerController.Instance.MaxMinglers + 1);
            int confirmed = 0;
            for(int i = 0; confirmed < nMinglers && i < NPC.NPCList.Count; ++i) {
                if(NPC.NPCList[i].AttemptMingle()) {
                    ++confirmed;
                }
            }
            //Debug.Log("Confirmed " + confirmed + "Attempted " + nMinglers);
            // Make other NPCs either wait or roam
            for (int i = 0; i < NPC.NPCList.Count; ++i) {
                if (NPC.NPCList[i].CurrentBehaviour == NPC.Behaviour.None) {
                    NPC.NPCList[i].WaitOrRoam();
                }
            }
        }

        private void UpdateTime() {
            // Update interactions
            interactionCount--;
            // Play clock sound
            AudioManager.Instance.Play(ClockSound, s => s.volume = ClockSoundVolume);
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

        private void ResetClock() {
            Vector3 hourRotation = ClockHourHandle.localEulerAngles;
            Vector3 minuteRotation = ClockMinuteHandle.localEulerAngles;
            hourRotation.y = -60;
            ClockHourHandle.localEulerAngles = hourRotation;
            minuteRotation.y = 0;
            ClockMinuteHandle.localEulerAngles = minuteRotation;
        }
        public void ClearScene() {
            HideConversation();
            // Update AIDirector
            AIDirector.Instance.CalculateDifficulty();

            // Hide scene
            HideScene();
            // Reset interaction count
            interactionCount = DailyInteracions;
            // Clear references to NPCs
            CurrentInterrogationTarget = null;
            SelectedNPC = null;
            // Reset clock
            ResetClock();            
        }

        public void ClearPlatformText() {
            PlatformTextMesh.text = "";
        }

        public void GenerateNextDay() {
            ClearScene();
            //ClearPlatformText();
            ++currentDay;
            // Murder new witness
            Clue.LatestVictim = MurderWitness();
            // Clear reference to arrested NPC
            arrestedNPC = null;
            // Cool NPC moods
            foreach (NPC n in NPC.NPCList) { n.CoolMood(); }
            // Generate conversations
            ConversationHandler.TruthGraph = GraphBuilder.BuildGraph(
                AIDirector.Instance.NumberOfDescriptiveClues, 
                AIDirector.Instance.PeopleLocationClues, 
                AIDirector.Instance.MurderLocationClues,
                AIDirector.Instance.Pointers);
            ConversationHandler.SetupConversations(
                AIDirector.Instance.PercentageLiars, 
                AIDirector.Instance.PercentageDescriptiveLiars);
            // Reset clock
            ResetClock();
            // Show new day message
            var dayStartStatements = IO.FileLoader.GetLimericks();
            // Fuck Ann that sleazy greaseball, she is always stealing my fries. #FuckAnn
            var platformMessage = Clue.LatestVictim.Name != "Ann" && Rng.Next(5) != 0 ?
                ConstructDayStatement(dayStartStatements[Rng.Next(dayStartStatements.Count)], Clue.LatestVictim.Name, Clue.LatestVictim.IsMale) :
                "I was in a room with Hitler, Bin Laden, and Ann, carrying a gun with two bullets, and I still shot Ann twice.";
            platformText = TextWrapper.BreakLine(platformMessage);
            
        }

        #region Pronouns
        private static List<string> malePronouns = new List<string>() { "man", "he", "his", "men", "him", "Mister" };
        private static List<string> femalePronouns = new List<string>() { "woman", "she", "hers", "women", "her", "Miss" };
        #endregion
        private string ConstructDayStatement(string limerick, string name, bool isMale) {
            Regex genderRegex = new Regex(@"(\[gender\[(\d)\]\])");
            var genderMatches = genderRegex.Matches(limerick);
            foreach (Match m in genderMatches) {
                var pronoun = isMale ? malePronouns[int.Parse(m.Groups[2].Value) - 1] : femalePronouns[int.Parse(m.Groups[2].Value) - 1];
                limerick = limerick.Replace(m.Value, pronoun);
            }
            limerick = limerick.Replace("[name]", name);

            return limerick;
        }

        public void BeginDay() {
            // Show scene
            ShowScene();
            // Mingling phase
            ExecuteMinglePhase();
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
            LoadMenu();

        }

        public void LoadMenu() {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }


        private void HideScene() {
            NPCS.SetActive(false);
        }

        private void ShowScene() {
            NPCS.SetActive(true);
            foreach(NPC npc in NPC.NPCList) {
                npc.enabled = false;
                npc.enabled = true;
            }
        }

        private NPC MurderWitness() {
            NPC victim = null;
            // Only murder if there is more than one NPC
            if (NPC.NPCList.Count > 1) {
                // Find an NPC that is not the killer
                int index;
                NPC target;
                do {
                    //index = UnityEngine.Random.Range(0, NPC.NPCList.Count);
                    index = Rng.Next(NPC.NPCList.Count);
                    //index = UseFixedSeed ? new System.Random(Seed).Next(NPC.NPCList.Count) : new System.Random(DateTime.Now.Millisecond).Next(NPC.NPCList.Count);
                    target = NPC.NPCList[index];
                } while (NPC.NPCList[index].IsKiller || (arrestedNPC == target && NPC.NPCList.Count > 2));
                // Save victim's name
                victim = target;
                // Remove from list
                NPC.NPCList.RemoveAt(index);
                // Destroy game object
                Destroy(target.gameObject);
            }
            return victim;
        }
        #endregion
    }
}
