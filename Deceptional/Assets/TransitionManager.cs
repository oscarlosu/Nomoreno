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
    public Transition Cameras;
    public Transition Buttons;
    public RotateTransition WaitingRoomCam;


    // Use this for initialization
    void Awake() {
        //DontDestroyOnLoad(gameObject);
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
        Cameras.Execute();
        yield return new WaitUntil(() => Cameras.State == Transition.TransitionState.Done);
        // Rotate waiting room
        WaitingRoomCam.Execute(1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);
    }

    public void BeginDayTransition() {
        StartCoroutine(BeginDayCo());
    }
    private IEnumerator BeginDayCo() {
        // Tell PlayerController to start next day
        PlayerController.Instance.Begin();
        // Rotate waiting room back
        WaitingRoomCam.Execute(-1);
        yield return new WaitUntil(() => WaitingRoomCam.State == Transition.TransitionState.Done);        
        // Drop buttons
        Buttons.Execute();
        yield return new WaitUntil(() => Buttons.State == Transition.TransitionState.Done);
    }
}
