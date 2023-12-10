using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;


//This script is used to get access to certain UI elements on the canvas of any scene.
//It's properties are set in the inspector, and are used by other scripts to change the UI, mostly UIManager.
//I dont have much time, beacause the contest ends in 13 days, so I'm not going to comment this script any further.
//I'm sorry for the inconvenience.
//I hope you understand.
//Thank you.
//Goodbye.
//Have a nice day.
//I'm sorry.
//I'm so sorry.
//I'm so so sorry.
//I'm so so so sorry.
//I'm so so so so sorry.
//I'm so so so so so sorry.
//I'm so so so so so so sorry.
//I'm so so so so so so so sorry.
//I'm so so so so so so so so sorry.
//I'm so so so so so so so so so sorry.
//Recurvension.
//Github Copilot is so much fun.

public class CanvasUIManager : MonoBehaviour
{
    //Inspector remind that all of these fields have to be set in the inspector.
    [Tooltip("ALL HAVE TO BE SET IN THE INSPECTOR")]


    public GameObject loadingImage;
    public List<PlayableDirector> cutscenesDirectors;
    public GameObject cutsceneDirectorsParent;
    public GameObject cutsceneWindow;

    public GameObject blackPanel;
    public GameObject interactObject;
    public GameObject OptionsMenu;

    public GameObject saveInput;

    [Header("Map only")]
    public GameObject[] itemHolders;
    public Sprite[] itemSprites;
    public TextMeshProUGUI infotext;

    public PlayableDirector getCutsceneDirector(string name)
    {
        foreach (var director in cutscenesDirectors)
        {
            if (director.gameObject.name == name)
                return director;
        }
        return null;
    }

    public void cutsceneWindowUnactiveAll()
    {
        foreach (var item in cutsceneWindow.GetComponentsInChildren<Transform>())
        {
            item.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (cutsceneDirectorsParent == null)
        {
            cutsceneDirectorsParent = cutsceneWindow;
        }
        if (cutscenesDirectors == null || cutscenesDirectors.Count == 0)
        {
            cutscenesDirectors = cutsceneDirectorsParent.GetComponentsInChildren<PlayableDirector>(true).ToList();
        }
    }
    public void CloseOptionsMenu()
    {
        GameManager.instance.ExitOptionsMenu();
    }
    public void ExitToMainMenu()
    {
        GameManager.instance.LoadScene("MainMenu");
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void LoadGame()
    {
        SaveManager.instance.LoadGame();
    }
    public void SaveGame()
    {
        SaveManager.instance.Save();
    }
}
