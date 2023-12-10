using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Timeline;
using System.Linq;

public class GameManager : MonoBehaviour
{
    //Singleton + Awake
    public static GameManager instance;
    private void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }
        instance = this;
        controls = new Controls();
        DontDestroyOnLoad(gameObject);

        //the same control needs to be in two layers -> Player and UI

        controls.Player.UniversalButton.performed += ctx => PlayerController.instance?.OnActionButton(0);
        controls.Player.FirstActionButton.performed += ctx => PlayerController.instance?.OnActionButton(1);
        controls.Player.FirstActionButton.started += ctx => PlayerController.instance?.animController?.handleNextJabPress();
        controls.Player.SecondActionButton.performed += ctx => PlayerController.instance?.OnActionButton(2);
        controls.Player.ThirdActionButton.performed += ctx => PlayerController.instance?.OnActionButton(3);
        controls.Player.Menu.performed += ctx => ToggleMenu();
        controls.Player.CameraChange.performed += ctx => CameraToggle();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public Controls controls;
    [Space]

    public GameObject mainCanvas;
    bool ableToChangeCamera = false;
    public Camera mainCamera;
    public GameObject zoomCam;
    public PlayableDirector playableDirector;

    public GamepadType gamepadType;

    public CanvasUIManager canvasManager;

    public Interactable nearestInteractable;
    public float nearestInteractableDistance;
    public bool isInteracting;
    public string currentScene;
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();

        //Handle input device change and set the correct interact image. TODO set all sprites at once, not only interact image
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Enabled)
            {
                Debug.Log("Device added: " + device);
            }
            else if (change == InputDeviceChange.Removed)
            {
                Debug.Log("Device removed: " + device);
            }
        };

        //TODO: Handle input device change and set the correct interact image.
        InputSystem.onEvent.Call(a =>
        {
            var device = InputSystem.GetDeviceById(a.deviceId);
            if (device == Gamepad.current)
            {


                switch (Gamepad.current)
                {
                    case DualShockGamepad:
                        gamepadType = GamepadType.DualShock;
                        UIManager.instance.ChangeSprites(0);
                        break;
                    case XInputController:
                        gamepadType = GamepadType.Xbox;
                        UIManager.instance.ChangeSprites(1);
                        break;
                    case Gamepad:
                        gamepadType = GamepadType.Generic;
                        UIManager.instance.ChangeSprites(2);
                        break;
                    default:

                        break;

                }
            }
            else if (device == Keyboard.current)
            {
                gamepadType = GamepadType.Keyboard;
                UIManager.instance.ChangeSprites(3);
            }
            else
            {
                gamepadType = GamepadType.Keyboard;
                UIManager.instance.ChangeSprites(3);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadScene(string name)
    {
        StartCoroutine(LoadSceneCo(name));


    }

    public void LoadMap()
    {
        StartCoroutine(LoadSceneCo("world-map"));
    }
    public void CameraToggle()
    {
        if (ableToChangeCamera && zoomCam != null)
        {
            zoomCam.SetActive(!zoomCam.activeInHierarchy);
        }
    }
    IEnumerator LoadSceneCo(string name)
    {
        IEnumerator loadingImageCo(float waitTime)
        {
            int i = 0;
            while (true)
            {
                UIManager.instance.canvasManager.loadingImage.GetComponent<Image>().sprite = UIManager.instance.loadingImageAnimation[i];
                i++;
                if (i > UIManager.instance.loadingImageAnimation.Count)
                    i = 0;
                yield return new WaitForSecondsRealtime(waitTime);
            }
        }

        var faded = false;

        UIManager.instance.blackPanelFader.OnFadeInComplete += () => { faded = true; };
        UIManager.instance.blackPanelFader.FadeIn();
        yield return new WaitUntil(() => faded);

        UIManager.instance.canvasManager.loadingImage.SetActive(true);
        StartCoroutine(loadingImageCo(0.1f));
        yield return new WaitForSecondsRealtime(0.2f);
        AsyncOperation op = SceneManager.LoadSceneAsync(name);
        while (!op.isDone)
        {
            yield return null;
        }
        StopCoroutine(loadingImageCo(0.1f));
        UIManager.instance.StopAllCoroutines();
        UIManager.instance.canvasManager.loadingImage.SetActive(false);
        yield break;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Stop all coroutines to prevent unwanted behaviour like running coroutines from previous scene
        StopAllCoroutines();
        UpdateAllObjects();
        AudioManager.instance.musicSource.Stop();
        switch (scene.name)
        {
            case "MainMenu":
                currentScene = "MainMenu";
                UIManager.instance.blackPanelFader.FadeOut();
                AudioManager.instance.PlayMusic("MainMenu", true, 2f);
                break;
            //if contains 'fight' in the name
            case string s when s.Contains("Fight"):
                currentScene = s;
                ableToChangeCamera = true;
                UIManager.instance.StartCoroutine(UIManager.instance.FightStartCutsceneCo(FindAnyObjectByType<Enemy>().Name));

                break;
            case "world-map":

                currentScene = "world-map";

                foreach (var item in SaveManager.saveData.bossPoints)
                {
                    switch (item.Key)
                    {
                        case "Frosty Fingers":
                            UIManager.instance.canvasManager.itemHolders[0].GetComponent<Image>().sprite = UIManager.instance.canvasManager.itemSprites[0];
                            UIManager.instance.canvasManager.itemHolders[0].SetActive(true);
                            break;
                        case "Whispering Reiver":
                            UIManager.instance.canvasManager.itemHolders[1].GetComponent<Image>().sprite = UIManager.instance.canvasManager.itemSprites[1];
                            UIManager.instance.canvasManager.itemHolders[1].SetActive(true);
                            break;
                        case "Ice Snatcher" :
                            UIManager.instance.canvasManager.itemHolders[2].GetComponent<Image>().sprite = UIManager.instance.canvasManager.itemSprites[2];
                            UIManager.instance.canvasManager.itemHolders[2].SetActive(true);
                            break;
                        case "Jack Lumber (He/His)":
                            UIManager.instance.canvasManager.itemHolders[3].GetComponent<Image>().sprite = UIManager.instance.canvasManager.itemSprites[3];

                            UIManager.instance.canvasManager.itemHolders[3].SetActive(true);
                            break;
                        case "Scarlet Bandit":
                            UIManager.instance.canvasManager.itemHolders[4].GetComponent<Image>().sprite = UIManager.instance.canvasManager.itemSprites[4];
                            UIManager.instance.canvasManager.itemHolders[4].SetActive(true);
                            break;
                    }
                }

                UIManager.instance.blackPanelFader.FadeOut();
                var p = FindFirstObjectByType<MapPlayerController>();
                p.transform.position = FindObjectsByType<DestinationPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList().Find(ctx => ctx.destinationName == SaveManager.saveData.destinationPointName).transform.position;
                EnablePlayerControls();
                EnableUIControls();
                ResumeGame();
                break;
            default:
                break;
        }
    }

    void UpdateAllObjects()
    {
        //GameManager:
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        zoomCam = GameObject.FindGameObjectWithTag("ZoomCam");
        ableToChangeCamera = false;
        //PlayerController:
        //PlayerController.instance = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //UIManager:
        UIManager.instance.mainCanvas = mainCanvas;
        UIManager.instance.canvasManager = mainCanvas.GetComponent<CanvasUIManager>();
        UIManager.instance.signalReceiver = UIManager.instance.canvasManager.GetComponent<SignalReceiver>();

        UIManager.instance.blackPanel = UIManager.instance.canvasManager.blackPanel;
        UIManager.instance.blackPanelFader = UIManager.instance.blackPanel.GetComponent<Fader>();

        UIManager.instance.interactObject = UIManager.instance.canvasManager.interactObject;
        UIManager.instance.OptionsMenu = UIManager.instance.canvasManager.OptionsMenu;

        SaveManager.instance.saveInput = UIManager.instance.canvasManager.saveInput;
    }
    public void PlayCutscene(CutsceneSO so, Action callback, bool isSkippable)
    {
        UIManager.instance.cutsceneCo = StartCoroutine(UIManager.instance.PlayCutscene(so, callback, isSkippable));
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }
    private void OnDisable()
    {
        controls.Player.Disable();
        controls.UI.Disable();
    }

    public void DisablePlayerControls()
    {
        controls.Player.Disable();
    }
    public void EnablePlayerControls()
    {
        controls.Player.Enable();
    }
    public void DisableUIControls()
    {
        controls.UI.Disable();
    }
    public void EnableUIControls()
    {
        controls.UI.Enable();
    }
    public void PauseGame()
    {
        Time.timeScale = 0;

    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
    public void EndGame()
    {
        PauseGame();
        DisablePlayerControls();
        DisableUIControls();
    }
    public static T CopyScriptableObject<T>(T t) where T : ScriptableObject
    {
        return Instantiate(t);
    }

    public void ToggleMenu()
    {
        if (UIManager.instance.canvasManager.OptionsMenu.activeInHierarchy)
        {
            ExitOptionsMenu();
        }
        else
        {
            EnterOptionsMenu();
        }
    }
    public void EnterOptionsMenu()
    {
        DisablePlayerControls();
        EnableUIControls();
        PauseGame();
        UIManager.instance.canvasManager.OptionsMenu.SetActive(true);

    }
    public void ExitOptionsMenu()
    {
        DisableUIControls();
        UIManager.instance.canvasManager.OptionsMenu.SetActive(false);
        ResumeGame();
        EnablePlayerControls();
    }
}

public enum GamepadType { DualShock, Xbox, Generic, Keyboard };