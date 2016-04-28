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
        CamerasIn.Execute();
        yield return new WaitUntil(() => CamerasIn.State == Transition.TransitionState.Done);
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
        // Rotate waiting room
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
    }

    public void BeginDayTransition() {
        StartCoroutine(BeginDayCo());
    }
    private IEnumerator BeginDayCo() {
        // Tell PlayerController to start next day
        PlayerController.Instance.BeginDay();
        // Rotate waiting room back
        WaitingRoomCam.Execute(1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
        camController.enabled = true;
        // Drop buttons
        ButtonsIn.Execute();
        yield return new WaitUntil(() => ButtonsIn.State == Transition.TransitionState.Done);
    }

    public void DayOverTransition() {
        StartCoroutine(DayOverCo());
    }
    private IEnumerator DayOverCo() {
        ButtonsOut.Execute();
        Clock.Execute(-1);
        yield return new WaitUntil(() => Clock.State == Transition.TransitionState.Done && ButtonsOut.State == Transition.TransitionState.Done);        
    }

    public void EndDayTransition() {
        StartCoroutine(EndDayCo());
    }
    private IEnumerator EndDayCo() {
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
        Clock.Execute(1);
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => Clock.State == Transition.TransitionState.Done && WaitingRoomCam.State == Transition.TransitionState.Done);
    }

    public void ArrestTransition(Transform arrested) {
        StartCoroutine(ArrestCo(arrested));
    }
    private IEnumerator ArrestCo(Transform arrested) {
        // Drop cage
        CageDrop.Execute();
        yield return new WaitUntil(() => CageDrop.State == Transition.TransitionState.Done);
        // Parent accused to cage
        NavMeshAgent navAg = arrested.GetComponent<NavMeshAgent>();
        navAg.enabled = false;
        Transform parent = arrested.parent;
        arrested.SetParent(CageDrop.transform);
        // Lift cage and rotate waiting room
        CageLift.Execute();
        camController.enabled = false;
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => CageLift.State == Transition.TransitionState.Done && WaitingRoomCam.State == Transition.TransitionState.Done);
        // Unparent accused from cage
        arrested.SetParent(parent);        
        // Tell PlayerController to generate next day
        PlayerController.Instance.GenerateNextDay();
    }

    public void EndGameTransition() {
        StartCoroutine(EndGameCo());
    }
    private IEnumerator EndGameCo() {
        CamerasOut.Execute();
        yield return new WaitUntil(() => CamerasOut.State == Transition.TransitionState.Done);
        PlayerController.Instance.LoadMenu();
    }
}
