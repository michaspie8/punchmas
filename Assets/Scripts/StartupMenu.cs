using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartupMenu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject AuthorsMenu;
    private GameObject[] Menus;

    private void Start()
    {
        Menus = new GameObject[] { MainMenu, UIManager.instance.OptionsMenu, AuthorsMenu };
        GoToMenu(MainMenu);
    }
    public void GoToMenu(GameObject menuObject)
    {
        foreach (var menu in Menus)
        {
            if (menu.activeInHierarchy)
                menu.SetActive(false);
        }
        menuObject.SetActive(true);
    }
    public void Quit()
    {
        Debug.Log("Quit!!!");
        Application.Quit();

    }
    public void StartGame()
    {
        GameManager.instance.EnableUIControls();
        GameManager.instance.DisableUIControls();
        if (SaveManager.saveData.isGameStarted)
        {
            GameManager.instance.LoadMap();
            return;
        }
        else
        {
            GameManager.instance.PlayCutscene(UIManager.instance.cutsceneDictionary["Start"], () => { if (UIManager.instance.cutsceneCo != null) SaveManager.instance.NewGame(); }, true);
        }

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
