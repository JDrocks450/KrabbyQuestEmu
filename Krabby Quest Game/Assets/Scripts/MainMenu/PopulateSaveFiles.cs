using StinkyFile.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopulateSaveFiles : MonoBehaviour
{
    private Button sampleButton;
    ScrollRect scrollRegion;
    SaveFile selected;
    GameObject playerNameField;
    bool saveFileMode = false;

    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        scrollRegion = GetComponent<ScrollRect>();
        var bhav = GameObject.Find("EventSystem").GetComponent<MainMenuBehavior>();
        sampleButton = scrollRegion.content.transform.GetChild(0).gameObject.GetComponent<Button>();
        ButtonStyler.Style(sampleButton);
        playerNameField = transform.parent.transform.GetChild(4).gameObject;
        var saveFiles = StinkyFile.Save.SaveFile.GetAllSaves();
        int lastHeight = 0;
        foreach (var save in saveFiles)
        {
            if (save.SaveFileInfo.IsBackup) continue;
            MakeButton(save.SaveFileInfo.PlayerName, ref lastHeight,
                delegate
                {
                    saveFileMode = false;
                    playerNameField.SetActive(false);
                    if (save == selected)
                        bhav.GoToMapScreen(save);
                    else
                    {
                        SoundLoader.Play("sb-type.wav", false).volume = .5f;
                        selected = save;
                        save.FullLoad();
                        SaveFileInfoPopulate.PopulateInfo(save);
                    }
                });
        }
        var createButton = MakeButton("Create New Save File", ref lastHeight, delegate
        {
#if false
            if (!saveFileMode)
            {
                SaveFileInfoPopulate.PopulateInfo(null);
                playerNameField.SetActive(true);
                createButton.transform.GetChild(0).GetComponent<Text>().text = "OK";
                return;
            }
            var text = playerNameField.transform.GetChild(1).GetChild(1).GetComponent<Text>();
#endif
            var name = DateTime.Today.ToShortDateString();
            var save = new SaveFile(name);
            bhav.GoToMapScreen(save);
        });
        MakeButton("Go Back", ref lastHeight,
            delegate
            {
                bhav.CloseSaveSelect();
            });
        (scrollRegion.content.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lastHeight);
    }

    Button MakeButton(string Content, ref int lastHeight, UnityAction onClick)
    {
        var button = Instantiate(sampleButton);
        var transform = button.gameObject.transform as RectTransform;
        transform.position = new Vector3(0, -lastHeight, 0);
        lastHeight += (int)transform.rect.height + 10;
        transform.GetChild(0).GetComponent<Text>().text = Content;
        transform.SetParent(scrollRegion.content, false);
        button.gameObject.SetActive(true);
        button.onClick.AddListener(onClick);        
        return button;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
