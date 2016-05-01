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
    // End day

    // New day


    // Arrest
    public Transition CageDrop;
    public Transition CageLift;
    // End game
    public Transition CamerasOut;

    private CameraController camController;

    // Use this for initialization
    void Awake() {
        //DontDestroyOnLoad(gameObject);
        if(WaitingRoomCam != null) {
            camController = WaitingRoomCam.gameObject.GetComponent<CameraController>();
        }
        
    }

    public void StartTransition() {
        StartCoroutine(StartCo());
    }
    private IEnumerator StartCo() {
        Title.Execute();
        StartButton.Execute();
        yield return new WaitUntil(() => Title.State == Transition.TransitionState.Done && StartButton.State == Transition.TransitionState.Done);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void GameTransition() {
        StartCoroutine(GameCo());
    }
    private IEnumerator GameCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        CamerasIn.Execute();
        yield return new WaitUntil(() => CamerasIn.State == Transition.TransitionState.Done);
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
        // Rotate waiting room
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
        // Show text
        PlayerController.Instance.ShowPlatformText();
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }

    public void BeginDayTransition() {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
			StartCoroutine(BeginDayCo());
		}        
    }
    private IEnumerator BeginDayCo() {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        // Tell PlayerController to start next day
        PlayerController.Instance.BeginDay();
        // Rotate waiting room back
        WaitingRoomCam.Execute(1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
        camController.enabled = true;
        // Drop buttons
        ButtonsIn.Execute();
        yield return new WaitUntil(() => ButtonsIn.State == Transition.TransitionState.Done);
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }

    public void DayOverTransition(bool gameFinished) {
        StartCoroutine(DayOverCo(gameFinished));
    }
    private IEnumerator DayOverCo(bool gameFinished) {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        if(gameFinished) {
            // Clear platform text
            PlayerController.Instance.ClearPlatformText();
            camController.enabled = false;
            WaitingRoomCam.Execute(-1);
            yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
            PlayerController.Instance.ShowPlatformText();
        } else {
            ButtonsOut.Execute();
            Clock.Execute(-1);
            yield return new WaitUntil(() => Clock.State == Transition.TransitionState.Done && ButtonsOut.State == Transition.TransitionState.Done);
        }
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }

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
        yield return new WaitUntil(() => Clock.State == Transition.TransitionState.Done && WaitingRoomCam.State == Transition.TransitionState.Done);
        PlayerController.Instance.ShowPlatformText();
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;
    }

    public void ArrestTransition(Transform arrested, bool gameFinished) {
		if(PlayerController.Instance.State == PlayerController.ControllerState.Enabled) {
        	StartCoroutine(ArrestCo(arrested, gameFinished));
		}
    }
    private IEnumerator ArrestCo(Transform arrested, bool gameFinished) {
		PlayerController.Instance.State = PlayerController.ControllerState.Disabled;
        // Drop cage
        CageDrop.Execute();
        yield return new WaitUntil(() => CageDrop.State == Transition.TransitionState.Done);
        // Parent accused to cage
        NavMeshAgent navAg = arrested.GetComponent<NavMeshAgent>();
        navAg.enabled = false;
        Transform parent = arrested.parent;
        arrested.SetParent(CageDrop.transform);
        // Clear platform text
        PlayerController.Instance.ClearPlatformText();
        // Lift cage and rotate waiting room
        CageLift.Execute();
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => CageLift.State == Transition.TransitionState.Done && WaitingRoomCam.State == Transition.TransitionState.Done);
        // Unparent accused from cage
        arrested.SetParent(parent);        
        
        if(gameFinished) {
            PlayerController.Instance.ClearScene();
        } else {
            // Tell PlayerController to generate next day
            PlayerController.Instance.GenerateNextDay();
        }
        PlayerController.Instance.ShowPlatformText();
		PlayerController.Instance.State = PlayerController.ControllerState.Enabled;

    }

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
}
