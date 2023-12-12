using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class DestinationPoint : Interactable
{
    public string destinationName;
    public GameObject info;
    public string bossName;
    public string altBossName;
    public override void Interact()
    {

        base.Interact();
        if (destinationName == "Santa's Palace")
        {
            if (SaveManager.saveData.finalBossDefeated)
            {
                //if final boss is defeated, player can go to the end cutscene
                GameManager.instance.LoadScene("End");
                return;
            }
            else
            {
                //If all bosses are defeated, player can go to the final boss
                if (SaveManager.saveData.bossPoints.ContainsKey("Frosty Fingers") &&
                    SaveManager.saveData.bossPoints.ContainsKey("Whispering Reiver") &&
                    SaveManager.saveData.bossPoints.ContainsKey("Ice Snatcher") &&
                    (SaveManager.saveData.bossPoints.ContainsKey("Lumberjack") || SaveManager.saveData.bossPoints.ContainsKey("Jack Lumber (He/His)")) &&
                    SaveManager.saveData.bossPoints.ContainsKey("Scarlet Bandit"))
                {
                    GameManager.instance.LoadScene("Boss");
                }
                else
                {
                    //else he cant
                    UIManager.instance.canvasManager.infotext.text = "You must have all 5 artifacts to go there";
                    StartCoroutine(ShowInfo());

                    return;
                }
            }
        }
        else
        {
            GameManager.instance.LoadScene("Fight" + destinationName);
        }

    }
    IEnumerator ShowInfo()
    {
        UIManager.instance.canvasManager.infotext.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        UIManager.instance.canvasManager.infotext.gameObject.SetActive(false);
    }
    public override void Update()
    {
        base.Update();
        if (isFocus && GameManager.instance.nearestInteractable == this.GetComponent<Interactable>())
        {
            LoadInfo();
            info.SetActive(true);
        }
        else
        {
            if (!UIManager.instance.interactObject.activeInHierarchy)
                info.SetActive(false);
        }

    }

    public void LoadInfo()
    {
        var bossName = this.bossName;
        if (SaveManager.saveData.bossPoints.ContainsKey(bossName) == false && !string.IsNullOrEmpty(altBossName) && SaveManager.saveData.bossPoints.ContainsKey(altBossName))
        {
            bossName = altBossName;
        }
        foreach (var obj in info.GetComponentsInChildren<TextMeshProUGUI>())
        {
            
            if (obj.gameObject.name == "BossName")
            {
                obj.text = this.bossName;
            }
            if (obj.gameObject.name == "points")
            {
                if (SaveManager.saveData.bossPoints.ContainsKey(bossName))
                    obj.text = SaveManager.saveData.bossPoints[bossName].ToString();
                else
                {
                    obj.text = "";
                }
            }
            if (obj.gameObject.name == "pointsText")
            {
                if (SaveManager.saveData.bossPoints.ContainsKey(bossName))
                    obj.text = "Record of points:";
                else
                {
                    obj.text = "You haven't fought this boss yet";
                }
            }
            if (obj.gameObject.name == "MainName")
            {
                obj.text = destinationName;
            }
        }
    }

}
