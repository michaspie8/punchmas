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


    public class SaveData
    {
        public bool isGameStarted = false;
        //Destination point at which the player will spawn
        public string destinationPointName = "Home";
        //Points of the player for each boss, if boss is not defeated, points are -1

        public Dictionary<string, int> bossPoints = new Dictionary<string, int>();
        //if final boss is defeated, yellow star is shown on the map
        public bool finalBossDefeated = false;
        public SaveData()
        {

        }

        public SaveData(SaveData data)
        {
            isGameStarted = data.isGameStarted;
            destinationPointName = data.destinationPointName;
            bossPoints = data.bossPoints;
            finalBossDefeated= data.finalBossDefeated;

        }

        public SaveData(bool isGameStarted, string destinationPointName, Dictionary<string, int> bossPoints, bool finalBossDefeated)
        {
            this.isGameStarted = isGameStarted;
            this.destinationPointName = destinationPointName;
            this.bossPoints = bossPoints;
            this.finalBossDefeated = finalBossDefeated;
        }


    }
    public SaveDataForSerialization ToSaveDataFS(bool isGameStarted, string destinationPointName, Dictionary<string, int> bossPoints, bool finalBossDefeated)
    {
        var temp = new SaveDataForSerialization();
        temp.igs = isGameStarted;
        temp.dpn = destinationPointName;
        foreach(var (a, b) in bossPoints)
        {
            temp.bn.Add(a);
            temp.bp.Add(b);
        }
        temp.fbd = finalBossDefeated;
        return temp;
    }
    public SaveDataForSerialization ToSaveDataFS(SaveData data)
    {
        var temp = new SaveDataForSerialization();
        temp.igs = data.isGameStarted;
        temp.dpn = data.destinationPointName;
        foreach (var (a, b) in data.bossPoints)
        {
            temp.bn.Add(a);
            temp.bp.Add(b);
        }
        temp.fbd = data.finalBossDefeated;
        return temp;
    }
    public SaveData FromSaveDataFS(SaveDataForSerialization data)
    {
        var save = new SaveData();
        save.isGameStarted = data.igs;
        save.destinationPointName= data.dpn;
        for(int i = 0; i<data.bn.Count; i++)
        {
            save.bossPoints.Add(data.bn[i], data.bp[i]);
        }
        save.finalBossDefeated = data.fbd;
        return save;
    }
    [Serializable]
    public class SaveDataForSerialization
    {
        public bool igs = false;
        public string dpn = "Home";
        public List<string> bn = new();
        public List<int> bp = new();
        public bool fbd = false;
    }
    public static SaveData saveData = new();
    public GameObject saveInput;


    public bool LoadSave(string saveDataStr)
    {
        var backup = new SaveData(saveData);
        try
        {
            if (saveDataStr == "D¿ajco" || saveDataStr == "Jajco")
            {
                GameManager.instance.LoadScene("jajco");
            }
            var temp = JsonUtility.FromJson<SaveDataForSerialization>(saveDataStr);
            saveData = FromSaveDataFS(temp);
            GameManager.instance.LoadMap();
            return true;
        }
        catch (Exception e)
        {
            saveData = backup;
            Debug.LogError("Error loading save data: " + e.Message);
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
            var data = JsonUtility.ToJson(ToSaveDataFS(saveData), false);
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
            var saveData = saveInput.GetComponent<TMP_InputField>().text;
            instance.LoadSave(saveData);
        }
        catch
        {
            Debug.LogError("Error loading save data");
            saveData = temp;
        }
    }
}