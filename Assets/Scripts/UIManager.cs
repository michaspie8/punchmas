using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Diagnostics.Tracing;
using UnityEngine.Timeline;
using TMPro;
using System;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("UI elements")]
    public GameObject mainCanvas;

    [Header("Buttons")]
    public List<Sprite> universalButtonSprites;
    public List<Sprite> firstActionButtonSprites;
    public List<Sprite> secondActionButtonSprites;
    public List<Sprite> thirdActionButtonSprites;
    public List<Sprite> backButtonSprites;
    public Dictionary<ButtonType, Sprite> buttonSpriteDictionary;
    public enum ButtonType { universal, firstAction, secondAction, thirdAction, back };

    [Header("Cutscenes")]
    public CanvasUIManager canvasManager;

    public SignalReceiver signalReceiver;

    public GameObject blackPanel;
    public Fader blackPanelFader;

    public Coroutine cutsceneCo;
    public bool isCutscenePlaying = false;
    public Dictionary<string, CutsceneSO> cutsceneDictionary;

    public delegate void TypeWriterEnd(GameObject obj);
    public TypeWriterEnd typeWriterEnd;

    [Header("Interact")]
    public GameObject interactObject;

    [Header("Loading")]
    public List<Sprite> loadingImageAnimation;

    [Header("Options Menu")]
    public GameObject OptionsMenu;

    // Start is called before the first frame update
    void Start()
    {
        cutsceneDictionary = new Dictionary<string, CutsceneSO>();
        foreach (var cutscene in Resources.LoadAll<CutsceneSO>("Cutscenes"))
        {
            cutsceneDictionary.Add(cutscene.name, cutscene);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayCutscene(PlayableDirector playableDirector)
    {
        playableDirector.Play();
    }

    public void StopCutscene(PlayableDirector playableDirector)
    {
        playableDirector.Stop();
    }


    public void ChangeSprites(int i)
    {
        buttonSpriteDictionary = new Dictionary<ButtonType, Sprite>
        {
            { ButtonType.universal, universalButtonSprites[i] },
            { ButtonType.firstAction, firstActionButtonSprites[i] },
            { ButtonType.secondAction, secondActionButtonSprites[i] },
            { ButtonType.thirdAction, thirdActionButtonSprites[i] },
            { ButtonType.back, backButtonSprites[i] }
        };
    }
    public void ChangeSprites(GamepadType gamepadType)
    {
        switch (gamepadType)
        {
            case GamepadType.DualShock:
                ChangeSprites(0);
                break;
            case GamepadType.Xbox:
                ChangeSprites(1);
                break;
            case GamepadType.Generic:
                ChangeSprites(2);
                break;
            case GamepadType.Keyboard:
                ChangeSprites(3);
                break;
            default:
                break;
        }
    }



    public IEnumerator TypeWriter(string text, float waitTime, GameObject tmp)
    {
        for (var i = 0; i < text.Length; i++)
        {
            tmp.GetComponent<TextMeshProUGUI>().text = text.Substring(0, i);

            yield return new WaitForSecondsRealtime(waitTime);
        }
        typeWriterEnd?.Invoke(tmp);
    }
    public IEnumerator TypeWriter(string text, float waitTime, GameObject tmp, TypingAudioInfoSO so, bool makePredictable)
    {
        for (var i = 1; i < text.Length+1; i++)
        {
            if (so.typingSoundClips.Length > 0)
            {
                var clip = so.typingSoundClips[UnityEngine.Random.Range(0, so.typingSoundClips.Length)];
                if (!string.IsNullOrWhiteSpace(text.Substring(0, i).LastOrDefault().ToString()))
                    AudioManager.instance.PlayTypingSound(i, text.Substring(0, i).LastOrDefault(), so, makePredictable);
            }
            tmp.GetComponent<TextMeshProUGUI>().text = text.Substring(0, i);

            yield return new WaitForSecondsRealtime(waitTime);
        }
        typeWriterEnd?.Invoke(tmp);
    }

    //If enemy is null, player lost, otherwise player won and enemy is the enemy that lost
    public IEnumerator FightEndCutscene(Enemy enemy = null)
    {
        var win = true;
        if (enemy == null)
        {
            win = false;
            enemy = FindFirstObjectByType<Enemy>(FindObjectsInactive.Include);
        }
        GameManager.instance.DisablePlayerControls();
        GameManager.instance.DisableUIControls();
        PlayerController.instance.anim.SetTrigger(win ? "Victory" : "Lose");

        enemy.GetComponent<Animator>().SetTrigger(win ? "Lose" : "Victory");
        yield return new WaitForSecondsRealtime(5f);

        var faded = false;
        blackPanelFader.OnFadeInComplete += () => { faded = true; };
        blackPanelFader.FadeIn();
        yield return new WaitUntil(() => faded);

        canvasManager.cutsceneWindow.SetActive(true);
        var obj = canvasManager.cutsceneWindow.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Cutscene-fight-end");
        obj.text = "";
        obj.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();

        var end = false;
        typeWriterEnd += (GameObject obj) => { end = true; };

        StartCoroutine(TypeWriter("KNOCKOUT!!!", 0.07f,obj.gameObject));
        yield return new WaitUntil(()=>end);
        AudioManager.instance.musicSource.Stop();
        yield return new WaitForSecondsRealtime(0.5f);

        end = false;
        StartCoroutine(win ? TypeWriter("EGGSCELENT!\r\nYOU HAVE WON!", 0.1f, obj.gameObject) : TypeWriter("YOU HAVE LOST!", 0.1f, obj.gameObject));
        yield return new WaitUntil(() => end);

        yield return new WaitForSecondsRealtime(1f);
        if (win)
        {
            var finalPoints = 0;


            //Write about points
            end = false;
            obj.text = "";
            obj.alignment = TextAlignmentOptions.Left;
            var text = "";

            text += "Points:\n\r";
            text += "\tDamage dealt: " + enemy.points + "\n\r";
            if (PlayerController.instance.points > 0)
            {
                text += "\tDamage taken: " + PlayerController.instance.points;
                finalPoints = enemy.points - PlayerController.instance.points / 2;

            }
            else
            {
                text += "\tFlawless victory! + 1000";
                finalPoints = enemy.points + 1000;
            }

            text += "\n\rFinal: " + finalPoints;
            SaveManager.saveData.bossPoints[enemy.Name] = finalPoints;
            StartCoroutine(TypeWriter(text, 0.1f, obj.gameObject));


            yield return new WaitUntil(() => end);
        }


        yield return new WaitForSecondsRealtime(4f);
        canvasManager.cutsceneWindowUnactiveAll();
        if (win)
            SaveManager.saveData.bossPoints[enemy.Name] = 1;

        GameManager.instance.LoadMap();
        yield break;
    }

    public IEnumerator FightStartCutsceneCo(string enemyName = "")
    {
        var obj = canvasManager.cutsceneWindow.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Cutscene-fight-start");

        GameManager.instance.PauseGame();
        GameManager.instance.DisablePlayerControls();
        GameManager.instance.DisableUIControls();
        canvasManager.loadingImage.SetActive(false);
        canvasManager.cutsceneWindowUnactiveAll();


        canvasManager.cutsceneWindow.SetActive(true);


        obj.gameObject.SetActive(true);
        var faded = false;
        blackPanelFader.OnFadeOutComplete += () => { faded = true; };
        yield return new WaitForEndOfFrame();
        if (enemyName != "")
        {
            obj.text = PlayerController.instance.Name + "\nvs.\n" + enemyName;
            yield return new WaitForSecondsRealtime(2f);
        }
        yield return new WaitForSecondsRealtime(1f);
        obj.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        obj.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        obj.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        obj.text = "FIGHT!";
        yield return new WaitForSecondsRealtime(1f);
        AudioManager.instance.PlayMusic(enemyName, true);

        blackPanelFader.FadeOut();
        yield return new WaitUntil(() => faded);

        canvasManager.cutsceneWindowUnactiveAll();
        canvasManager.cutsceneWindow.SetActive(false);
        GameManager.instance.EnablePlayerControls();
        GameManager.instance.ResumeGame();
        yield break;

    }

    public void DisplayInteractionImage(string text)
    {
        interactObject.GetComponentInChildren<TMP_Text>().text = text;
        interactObject.SetActive(true);

    }

    public void EndDisplayInteractionImage()
    {
        interactObject.SetActive(false);
    }

    public IEnumerator PlayCutscene(CutsceneSO so, Action callback, bool isSkipable)
    {
        GameManager.instance.controls.Player.Menu.performed += ctx => { if (isCutscenePlaying && isSkipable) { StopCoroutine(cutsceneCo); callback(); } };
        AudioManager.instance.PlayMusic(so.musicName, true, so.musicDelay);
        isCutscenePlaying = true;
        yield return this;
        var blanksprite = Resources.Load<Sprite>("Sprites/blank");
        var sprites = new List<Sprite>();
        foreach (var path in so.pathToSprites)
        {
            var a = Resources.LoadAll<Sprite>(path).ToList();
            sprites.AddRange(a);
        }


        //TODO SORTING
        //sort alpthabeticly and by numbers (aka after 1 there is 2 not 10)
        //sprites = sprites.OrderBy(x => x.name).ToList();

        var cutsceneWindow = canvasManager.cutsceneWindow;
        var cutsceneImage = cutsceneWindow.GetComponent<Image>();

        var taskCompleted = false;
        //Fade to background
        blackPanelFader.OnFadeInComplete += () => { taskCompleted = true; };
        blackPanelFader.FadeIn();
        yield return new WaitUntil(() => taskCompleted);
        taskCompleted = false;

        //set blank sprite and activate window
        cutsceneImage.sprite = blanksprite;
        cutsceneWindow.SetActive(true);

        for (int frame = 0; frame < sprites.Count; frame++)
        {
            yield return new WaitForSecondsRealtime(so.timeBetweenFrames);
            cutsceneImage.sprite = sprites[frame];

            //if there is text for this frame, display it
            if (!string.IsNullOrEmpty(so.textForFrame[frame]))
            {
                var text = so.textForFrame[frame];
                yield return new WaitForSecondsRealtime(so.timeBetweenSpriteAndText);
                taskCompleted = false;
                var obj = blackPanel.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Cutscene-text");
                obj.text = "";
                obj.gameObject.SetActive(true);
                typeWriterEnd += (GameObject ob) => { taskCompleted = true; };
                StartCoroutine(TypeWriter(text, so.timeBetweenLetters, obj.gameObject, so.typingAudioInfo, false));
                yield return new WaitUntil(() => taskCompleted);
                taskCompleted = false;
                obj.gameObject.SetActive(false);
            }
            /*//if duration is 2, then there will be 2 frames with the same sprite
            if(duration < so.frameDurations[frame])
            {
                duration++;
                frame--;
            }*/
        }
        callback();
    }
}
