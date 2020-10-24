using StinkyFile.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileInfoPopulate : MonoBehaviour
{
    Text PlayerName, Unlocked, Completed, Perfected;
    static SaveFile saveFile;
    private SaveFile lastSaveFile;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerName = transform.GetChild(0).GetComponent<Text>();
        Unlocked = transform.GetChild(1).GetComponent<Text>();
        Completed = transform.GetChild(2).GetComponent<Text>();
        Perfected = transform.GetChild(3).GetComponent<Text>();
    }

    public static void PopulateInfo(SaveFile saveFile)
    {        
        SaveFileInfoPopulate.saveFile = saveFile;
    }

    // Update is called once per frame
    void Update()
    {
        if (saveFile == null)
        {
            PlayerName.text = "";
            Unlocked.text = "";
            Completed.text = "";
            Perfected.text = "";
            return;
        }
        if (saveFile == lastSaveFile) return;        
        PlayerName.text = $"Player Name: {saveFile.SaveFileInfo.PlayerName}";
        Unlocked.text = saveFile.UnlockedLevelsString;
        Completed.text = saveFile.CompletedLevelString;
        Perfected.text = saveFile.PerfectLevelsString;
        lastSaveFile = saveFile;
    }
}
