using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class TransitionManager : MonoBehaviour {
    /// <summary>
    /// Singleton pattern implementation that allows setting values in the inspector
    /// </summary>
    private static TransitionManager instance;
    public static TransitionManager Instance {
        get {
            if (instance == null) {
                instance = (TransitionManager)FindObjectOfType(typeof(TransitionManager));
                //if (instance == null)
                //    instance = (new GameObject("TransitionManager")).AddComponent<TransitionManager>();
            }
            return instance;
        }
    }
    // Start screen
    public Transition Title;
    public Transition StartButton;
    // Game starting
    public Transition CamerasIn;
    public Transition ButtonsIn;
    public RotateTransition WaitingRoomCam;
    // Day over
    public Transition ButtonsOut;
    public RotateTransition Clock;
    public AudioClip DayOverSound;
    public float DayOverSoundVolume = 1.0f;
    // End day

    // New day
    public FaceMainCamera PlatformBackController;
	public Transition BeginDayButtonEnter;
	public Transition BeginDayButtonExit;
    // Arrest
    public Transition CageDrop;
    public Transition CageLift;
	public GameObject CageBase;
    public AudioClip SuccessSound;
    public float SuccessSoundVolume = 1.0f;
    public AudioClip FailureSound;
    public float FailureSoundVolume = 1.0f;
    public AudioClip CageHitSound;
    public float CageHitSoundVolume = 1.0f;
    public AudioClip CageChainSound;
    public float CageChainSoundVolume = 1.0f;
    // End game
    public Transition CamerasOut;

    private CameraController camController;
    // Help
    public GameObject Help;

    public AudioClip PlatformTurningSound;
    public float PlatformTurningSoundVolume = 1.0f;
    public AudioClip PlatformStopSound;
    public float PlatformStopSoundVolume = 1.0f;


    public float ResultSoundDelay = 1.0f;

    // Use this for initialization
    void Awake() {
        //DontDestroyOnLoad(gameObject);
        if(WaitingRoomCam != null) {
            camController = WaitingRoomCam.gameObject.GetComponent<CameraController>();
        }
        
    }
    /// <summary>
    /// Transition that happends when the user presses the Start button. (Start screen drops down)
    /// </summary>
    public void StartTransition() {
        StartCoroutine(StartCo());
    }
    private IEnumerator StartCo() {
        Title.Execute();
        StartButton.Execute();
        yield return new WaitUntil(() => Title.State == Transition.TransitionState.Done && StartButton.State == Transition.TransitionState.Done);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    /// <summary>
    /// Transition that happens when the Game Scene is loaded
    /// </summary>
    public void GameTransition() {
        StartCoroutine(GameCo());
    }
    private IEnumerator GameCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        CamerasIn.Execute();
        yield return new WaitUntil(() => CamerasIn.State == Transition.TransitionState.Done);
        // Tell PlayerController to generate next day
        PlayerController.Instance.ClearPlatformText();
        PlayerController.Instance.GenerateNextDay();
        // Rotate waiting room
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        // Play platform rotation sound
        AudioSource rotationAS = AudioManager.Instance.Play(PlatformTurningSound, s => s.volume = PlatformTurningSoundVolume);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
        // Play end platform rotation sound and stop previous
        AudioManager.Instance.Stop(rotationAS);
        AudioManager.Instance.Play(PlatformStopSound, s => s.volume = PlatformStopSoundVolume);
        // Show text
        PlayerController.Instance.ShowPlatformText();        
		// Drop begin day button
		BeginDayButtonEnter.Execute();
		yield return new WaitUntil(() => BeginDayButtonEnter.State == Transition.TransitionState.Done);
        PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }
    /// <summary>
    /// Transition that happens at the beginning of each day, after the player presses the Begin Day button.
    /// </summary>
    public void BeginDayTransition() {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
			StartCoroutine(BeginDayCo());
		}        
    }
    private IEnumerator BeginDayCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
		// Disable rotation of Platform back
		PlatformBackController.enabled = false;
        // Tell PlayerController to start next day
        PlayerController.Instance.BeginDay();
        // Update calendar
        PlayerController.Instance.ShowCalendarText();
        // Rotate waiting room back
        WaitingRoomCam.Execute(1);
        // Play platform rotation sound
        AudioSource rotationAS = AudioManager.Instance.Play(PlatformTurningSound, s => s.volume = PlatformTurningSoundVolume);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
        // Play end platform rotation sound and stop previous
        AudioManager.Instance.Stop(rotationAS);
        AudioManager.Instance.Play(PlatformStopSound, s => s.volume = PlatformStopSoundVolume);
        camController.enabled = true;
        // Drop Begin Day button
        BeginDayButtonExit.Execute();
        // Drop buttons
        ButtonsIn.Execute();
        yield return new WaitUntil(() => ButtonsIn.State == Transition.TransitionState.Done);
		// Enable rotation of Platform back
		PlatformBackController.enabled = true;
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;

    }
    /// <summary>
    /// Transition that happens when the day ends after the player presses the Dismiss button.
    /// </summary>
    /// <param name="gameFinished"></param>
    public void DayOverTransition(bool gameFinished) {
        StartCoroutine(DayOverCo(gameFinished));
    }
    private IEnumerator DayOverCo(bool gameFinished) {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        // Clear platform text
        PlayerController.Instance.ClearPlatformText();
        // Play day over sound
        AudioManager.Instance.Play(DayOverSound, s => s.volume = DayOverSoundVolume);
        if (gameFinished) {
            camController.enabled = false;
            WaitingRoomCam.Execute(-1);
            ButtonsOut.Execute();


            // Play platform rotation sound
            AudioSource rotationAS = AudioManager.Instance.Play(PlatformTurningSound, s => s.volume = PlatformTurningSoundVolume);
            do {
                yield return null;
                if (WaitingRoomCam.State == Transition.TransitionState.Done) {
                    // Play end platform rotation sound and stop previous
                    AudioManager.Instance.Stop(rotationAS);
                    AudioManager.Instance.Play(PlatformStopSound, s => s.volume = PlatformStopSoundVolume);
                }                
            } while (ButtonsOut.State != Transition.TransitionState.Done || WaitingRoomCam.State != Transition.TransitionState.Done);

        PlayerController.Instance.ShowPlatformText();
        } else {
            ButtonsOut.Execute();
            Clock.Execute(-1);


            yield return new WaitUntil(() => Clock.State == Transition.TransitionState.Done &&
                                             ButtonsOut.State == Transition.TransitionState.Done);
        }
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }
    /// <summary>
    /// Transition that happens when the player presses the End Day button.
    /// </summary>
    public void EndDayTransition() {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
        	StartCoroutine(EndDayCo());
		}
    }
    private IEnumerator EndDayCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
        Clock.Execute(1);
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);

        // Play platform rotation sound
        AudioSource rotationAS = AudioManager.Instance.Play(PlatformTurningSound, s => s.volume = PlatformTurningSoundVolume);

        do {
            yield return null;
            if (WaitingRoomCam.State == Transition.TransitionState.Done) {
                // Play end platform rotation sound and stop previous
                AudioManager.Instance.Stop(rotationAS);
                AudioManager.Instance.Play(PlatformStopSound, s => s.volume = PlatformStopSoundVolume);
            }            
        } while (Clock.State != Transition.TransitionState.Done || WaitingRoomCam.State != Transition.TransitionState.Done);

        PlayerController.Instance.ShowPlatformText();
        // Drop begin day button
        BeginDayButtonEnter.Execute();
		yield return new WaitUntil(() => BeginDayButtonEnter.State == Transition.TransitionState.Done);

		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }
    /// <summary>
    /// Transition that happens when the player Arrests someone.
    /// </summary>
    /// <param name="arrested"></param>
    /// <param name="gameFinished"></param>
    public void ArrestTransition(Transform arrested, bool gameFinished) {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
        	StartCoroutine(ArrestCo(arrested, gameFinished));
		}
    }
    private IEnumerator ArrestCo(Transform arrested, bool gameFinished) {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
		// Disable cage base
		CageBase.SetActive(false);
        // Drop cage
        CageDrop.Execute();

        // Play cage chain sound
        AudioSource cageChainAS = AudioManager.Instance.Play(CageChainSound, s => s.volume = CageChainSoundVolume);
        yield return new WaitUntil(() => CageDrop.State == Transition.TransitionState.Done);
        // Stop cage sound
        AudioManager.Instance.Stop(cageChainAS);
        AudioManager.Instance.Play(CageHitSound, s => s.volume = CageHitSoundVolume);


        // Parent accused to cage
        NavMeshAgent navAg = arrested.GetComponent<NavMeshAgent>();
        navAg.enabled = false;
        Transform parent = arrested.parent;
        arrested.SetParent(CageDrop.transform);
        // Clear platform text
        PlayerController.Instance.ClearPlatformText();
		// Enable cage base
		CageBase.SetActive(true);
        // Lift cage and rotate waiting room
        CageLift.Execute();
        camController.enabled = false;
        ButtonsOut.Execute();
        WaitingRoomCam.Execute(-1);
        // Play day over sound
        AudioManager.Instance.Play(DayOverSound, s => s.volume = DayOverSoundVolume);
        // Play platform turning sound
        AudioSource rotationAS = AudioManager.Instance.Play(PlatformTurningSound, s => s.volume = PlatformTurningSoundVolume);
        // Play cage chain sound
        cageChainAS = AudioManager.Instance.Play(CageChainSound, s => s.volume = CageChainSoundVolume);
        do {
            yield return null;
            if (WaitingRoomCam.State == Transition.TransitionState.Done) {
                // Play end platform rotation sound and stop previous
                AudioManager.Instance.Stop(rotationAS);
                AudioManager.Instance.Play(PlatformStopSound, s => s.volume = PlatformStopSoundVolume);
            }
            if (CageLift.State == Transition.TransitionState.Done) {
                // Stop cage sound
                AudioManager.Instance.Stop(cageChainAS);
                AudioManager.Instance.Play(CageHitSound, s => s.volume = CageHitSoundVolume);
            }            
        } while (CageLift.State != Transition.TransitionState.Done || WaitingRoomCam.State != Transition.TransitionState.Done || ButtonsOut.State != Transition.TransitionState.Done);

        if(WaitingRoomCam.State != Transition.TransitionState.Done) {
            Debug.Log("Platform rotation not done");
        }
            // Unparent accused from cage
            arrested.SetParent(parent);
        // Play result sound with small delay to avoid sound overlapping
        if(gameFinished) {
            Invoke("PlayArrestSuccesSound", ResultSoundDelay);
        } else {
            Invoke("PlayArrestFailureSound", ResultSoundDelay);
        }

        PlayerController.Instance.ShowPlatformText();
        
        // Drop begin day button
        BeginDayButtonEnter.Execute();
		yield return new WaitUntil(() => BeginDayButtonEnter.State == Transition.TransitionState.Done);

		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;

    }

    void PlayArrestSuccesSound() {
        // Play success
        AudioManager.Instance.Play(SuccessSound, s => s.volume = SuccessSoundVolume);
        PlayerController.Instance.ClearScene();
    }

    void PlayArrestFailureSound() {
        // Play failure
        AudioManager.Instance.Play(FailureSound, s => s.volume = FailureSoundVolume);
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
    }

    

    /// <summary>
    /// Transition that happens when the player presses the Return to Start button or the Restart button.
    /// </summary>
    public void EndGameTransition() {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
        	StartCoroutine(EndGameCo());
		}
    }
    private IEnumerator EndGameCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        CamerasOut.Execute();
        yield return new WaitUntil(() => CamerasOut.State == Transition.TransitionState.Done);
        PlayerController.Instance.LoadMenu();
    }
    /// <summary>
    /// Transition that happens when the user presses the Help button
    /// </summary>
    public void ToggleHelpTransition() {
        Help.SetActive(!Help.activeSelf);
    }
}
