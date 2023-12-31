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
using JetBrains.Annotations;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
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
            try
            {
                tmp.GetComponent<TextMeshProUGUI>().text = text.Substring(0, i);

            }
            catch
            {
                print("Error in TypeWriter");
            }
            yield return new WaitForSecondsRealtime(waitTime);
        }
        typeWriterEnd?.Invoke(tmp);
    }

    public IEnumerator TypeWriter(string text, float waitTime, GameObject tmp, TypingAudioInfoSO so, bool makePredictable)
    {
        for (var i = 1; i < text.Length + 1; i++)
        {
            try
            {
                if (so.typingSoundClips.Length > 0)
                {
                    var clip = so.typingSoundClips[UnityEngine.Random.Range(0, so.typingSoundClips.Length)];
                    if (!string.IsNullOrWhiteSpace(text.Substring(0, i).LastOrDefault().ToString()))
                        AudioManager.instance.PlayTypingSound(i, text.Substring(0, i).LastOrDefault(), so, makePredictable);
                }
                tmp.GetComponent<TextMeshProUGUI>().text = text.Substring(0, i);
            }
            catch
            {
                print("Error in TypeWriter");
            }
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
        canvasManager.cutsceneWindow.GetComponent<Image>().enabled = false;
        var obj = canvasManager.cutsceneWindow.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Cutscene-fight-end");
        obj.text = "";
        obj.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();

        var end = false;
        typeWriterEnd += (GameObject obj) => { end = true; };

        StartCoroutine(TypeWriter("KNOCKOUT!!!", 0.07f, obj.gameObject));
        yield return new WaitUntil(() => end);
        AudioManager.instance.musicSource.Stop();
        yield return new WaitForSecondsRealtime(0.5f);

        end = false;
        StartCoroutine(win ? TypeWriter("EGGSCELENT!\r\nYOU HAVE WON!", 0.1f, obj.gameObject) : TypeWriter("YOU HAVE LOST!", 0.1f, obj.gameObject));
        yield return new WaitUntil(() => end);

        yield return new WaitForSecondsRealtime(1f);
        if (win)
        {
            int finalPoints = 0;


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
            if (enemy.Name == "Дед Мороз Ротшильд")
            {
                SaveManager.saveData.finalBossDefeated = true;

                canvasManager.cutsceneWindowUnactiveAll();
                canvasManager.cutsceneWindow.SetActive(false);
                canvasManager.cutsceneWindow.GetComponent<Image>().enabled = true;
                GameManager.instance.LoadScene("Credits");
                yield break;
            }
        }


        yield return new WaitForSecondsRealtime(4f);
        canvasManager.cutsceneWindowUnactiveAll();
        canvasManager.cutsceneWindow.SetActive(false);
        canvasManager.cutsceneWindow.GetComponent<Image>().enabled = true;

        GameManager.instance.LoadMap();
        yield break;
    }

    public IEnumerator FightStartCutsceneCo(string enemyName = "")
    {
        var obj = canvasManager.cutsceneWindow.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Cutscene-fight-start");
        canvasManager.cutsceneWindow.GetComponent<Image>().enabled = false;
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
        AudioManager.instance.typingAudioSource.Stop();
        AudioManager.instance.PlayMusic(enemyName, true);

        blackPanelFader.FadeOut();
        yield return new WaitUntil(() => faded);

        canvasManager.cutsceneWindowUnactiveAll();
        canvasManager.cutsceneWindow.SetActive(false);
        canvasManager.cutsceneWindow.GetComponent<Image>().enabled = true;
        GameManager.instance.EnablePlayerControls();
        GameManager.instance.ResumeGame();
        yield break;

    }

    public void DisplayInteractionImage(string text)
    {
        interactObject.GetComponentInChildren<TMP_Text>().text = text;
        interactObject.GetComponentInChildren<Image>().sprite = buttonSpriteDictionary[ButtonType.universal];
        interactObject.SetActive(true);

    }
    public void DisplayInteractionImage(string text, ButtonType buttonType)
    {
        interactObject.GetComponentInChildren<TMP_Text>().text = text;
        interactObject.GetComponentInChildren<Image>().sprite = buttonSpriteDictionary[buttonType];
        interactObject.SetActive(true);

    }

    public void EndDisplayInteractionImage()
    {
        interactObject.SetActive(false);
    }

    public IEnumerator CreditsCutscene()
    {
        var end = false;
        GameManager.instance.PlayCutscene(cutsceneDictionary["before-credits"], () => { end = true; }, false);
        yield return new WaitUntil(() => end);
        end = false;


        int maxTextureWidth = 7680;
        GameObject textureObj = GameObject.Find("WalkingRenderTexture");

        float resizeVel = 0f;

        float moveTime = 10f;
        Vector3 moveVel = Vector3.zero;
        Vector3 startPos = PlayerController.instance.transform.position;
        Vector3 endPos = new Vector3(90, PlayerController.instance.transform.position.y, PlayerController.instance.transform.position.z);

        Time.timeScale = 1f;

        AudioManager.instance.PlayMusic("Credits", false);

        yield return new WaitForSecondsRealtime(1f);
        PlayerController.instance.anim.SetTrigger("endWalk");
        yield return new WaitForSecondsRealtime(0.1f);

        blackPanelFader.FadeOut();

        var rectt = textureObj.GetComponent<RectTransform>();
        //Move player to endPos until player is near the endpos
        while (Vector3.Distance(PlayerController.instance.transform.position, endPos) > 3)
        {
            var x = Vector3.SmoothDamp(PlayerController.instance.transform.position, endPos, ref moveVel, moveTime);
            PlayerController.instance.transform.position = x;

            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.SmoothDamp(rectt.rect.width, maxTextureWidth, ref resizeVel, moveTime*0.6f));
            yield return new WaitForEndOfFrame();
        }

        //then, fade in and display credits cutscene.
        blackPanelFader.FadeOut();
        Time.timeScale = 0f;

        GameManager.instance.PlayCutscene(cutsceneDictionary["Credits"], () => { end = true; }, false, true);
        yield return new WaitUntil(() => end);
        GameManager.instance.LoadScene("MainMenu");
        yield break;


    }

    public IEnumerator PlayCutscene(CutsceneSO so, Action callback, bool isSkipable, bool fastAnimation = false)
    {
        IEnumerator fastAnim(Sprite[] sprites, float timeBetweenFrames, Image image)
        {
            int i = 0;
            while (true)
            {
                image.sprite = sprites[i];

                i++;
                if (i >= sprites.Length)
                    i = 0;
                yield return new WaitForSecondsRealtime(timeBetweenFrames);

            }
        }
        if (isSkipable)
        {
            Action<UnityEngine.InputSystem.InputAction.CallbackContext> xd = null;
            xd = ctx => { if (isCutscenePlaying && isSkipable) { StopCoroutine(cutsceneCo); isCutscenePlaying = false; callback(); GameManager.instance.controls.Player.Menu.performed -= xd; } };

            GameManager.instance.controls.Player.Menu.performed += xd;
        }

        AudioManager.instance.typingAudioSource.Stop();
        if (!string.IsNullOrEmpty(so.musicName))
        {
            //DNS stands for do not stop, so the music wont stop
            if (so.musicName != "dns")
            {
                AudioManager.instance.musicSource.Stop();
                AudioManager.instance.PlayMusic(so.musicName, true, so.musicDelay);
            }
        }
        else
        {
            AudioManager.instance.musicSource.Stop();
        }
        isCutscenePlaying = true;

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

        if (fastAnimation)
        {
            StartCoroutine(fastAnim(sprites.ToArray(), so.timeBetweenFrames, cutsceneImage));
            for (int frame = 0; frame < so.textForFrame.Length; frame++)
            {
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
                    yield return new WaitForSecondsRealtime(so.timeBetweenSpriteAndText);
                    obj.gameObject.SetActive(false);
                }
            }

        }
        else
        {
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
                    yield return new WaitForSecondsRealtime(so.timeBetweenSpriteAndText);
                    obj.gameObject.SetActive(false);
                }
                /*//if duration is 2, then there will be 2 frames with the same sprite
                if(duration < so.frameDurations[frame])
                {
                    duration++;
                    frame--;
                }*/
            }
        }
        isCutscenePlaying = false;
        cutsceneWindow.SetActive(false);
        callback();
    }
}
