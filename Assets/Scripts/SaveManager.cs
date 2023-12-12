using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    #region Singleton
    public static SaveManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of SaveManager found!");
            return;
        }
        instance = this;
    }
    #endregion

    [Serializable]
    public class SaveData
    {
        public bool isGameStarted = false;
        //Destination point at which the player will spawn
        public string destinationPointName = "Home";
        //Points of the player for each boss, if boss is not defeated, points are -1
        public Dictionary<string, int> bossPoints = new Dictionary<string, int>();
        //if final boss is defeated, yellow star is shown on the map
        public bool finalBossDefeated = false;
    }

    public static SaveData saveData = new();
    public GameObject saveInput;


    public bool LoadSave(string saveDataStr)
    {
        try
        {
            if(saveDataStr == "D¿ajco" || saveDataStr == "Jajco")
            {
                GameManager.instance.LoadScene("jajco");
            }
            saveData = JsonUtility.FromJson<SaveData>(saveDataStr);
            GameManager.instance.LoadMap();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading save data: " + e);
            return false;
        }
    }
    public void NewGame()
    {
        saveData = new SaveData();
        saveData.destinationPointName = "Home";
        saveData.bossPoints = new Dictionary<string, int>();
        saveData.finalBossDefeated = false;
        saveData.isGameStarted = true;
        GameManager.instance.LoadMap();
    }
    public void Save()
    {
        switch (GameManager.instance.currentScene)
        {
            case string s when s.Contains("MainMenu"):
                saveData.destinationPointName = "Home";
                break;
            case "world-map":
                saveData.destinationPointName = "Home";
                break;
            case string s when s.Contains("Fight"):
                saveData.destinationPointName = s;
                break;
        }
        try
        {
            var data = JsonUtility.ToJson(saveData,false);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (!WebGLFileSaver.IsSavingSupported())
                {
                    Debug.LogWarning("Saving is not supported on this device.");
                    return;
                }
                WebGLFileSaver.SaveFile(data, "save.txt");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving data: " + e);

        }
    }

    public void LoadGame()
    {
        var temp = saveData;
        try
        {
            var saveData = saveInput.GetComponent<TextMeshProUGUI>().text;
            instance.LoadSave(saveData);
        }
        catch
        {
            Debug.LogError("Error loading save data");
            saveData = temp;
        }
    }
}