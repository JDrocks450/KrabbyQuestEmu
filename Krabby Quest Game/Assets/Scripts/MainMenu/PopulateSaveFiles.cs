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

    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        scrollRegion = GetComponent<ScrollRect>();
        var bhav = GameObject.Find("EventSystem").GetComponent<MainMenuBehavior>();
        sampleButton = scrollRegion.content.transform.GetChild(0).gameObject.GetComponent<Button>();
        ButtonStyler.Style(sampleButton);
        var saveFiles = StinkyFile.Save.SaveFile.GetAllSaves();
        int lastHeight = 0;
        foreach (var save in saveFiles)
        {                      
            MakeButton(save.SaveFileInfo.PlayerName, ref lastHeight,
                delegate
                {
                    bhav.GoToMapScreen(save);
                });
        }
        MakeButton("Create New Save File", ref lastHeight,
            delegate
            {
                var save = new SaveFile("Remy");
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
