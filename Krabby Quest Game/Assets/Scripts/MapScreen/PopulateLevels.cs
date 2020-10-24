using Assets.Components;
using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PopulateLevels : MonoBehaviour
{
    Button sampleButton;
    StinkyLevel selected;
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        ScrollRect scrollRegion = GetComponent<ScrollRect>();
        var mapScreenBhav = GameObject.Find("EventSystem").GetComponent<MapScreenBehavior>();
        sampleButton = scrollRegion.content.transform.GetChild(0).gameObject.GetComponent<Button>();
        ButtonStyler.Style(sampleButton);
        var parser = LevelObjectManager.Parser = new StinkyFile.StinkyParser();
        parser.FindAllLevels(Path.Combine(TextureLoader.AssetDirectory, "levels"));
        int lastHeight = 0, CURRENT = 0, lastAvailableLevel = 0;
        foreach(var level in parser.LevelInfo)
        {
            bool isAvailable = false;            
            var levelInfo = level.GetSaveFileInfo(SaveFileManager.Current);
            isAvailable = levelInfo.IsAvailable;
            isAvailable = isAvailable ? true : CURRENT < 2; // first 2 levels always unlocked regardless            
            var button = Instantiate(sampleButton);
            var transform = button.gameObject.transform as RectTransform;
            transform.position = new Vector3(0, -lastHeight, 0);
            lastHeight += (int)transform.rect.height + 10;
            var name = ((isAvailable) ? "" : "LOCKED - ") + (levelInfo.WasPerfect ? "*" : "") + level.Name + (levelInfo.WasPerfect ? "*" : "");
            if (name.Length > 25)
                name = name.Substring(0, 25) + "...";
            transform.GetChild(0).GetComponent<Text>().text = name;
            transform.SetParent(scrollRegion.content,false);            
            button.gameObject.SetActive(true);
            if (isAvailable)
            {
                button.onClick.AddListener(delegate
                {
                    if (selected != level)
                    {
                        selected = level;
                        mapScreenBhav.SelectLevelOnMap(level);
                        return;
                    }
                    mapScreenBhav.GoToLevel(Path.GetFileName(level.LevelFilePath).Replace(".lv5", ""));
                });
            }
            else
            {
                button.spriteState = new SpriteState(); // make sure hovering doesn't work on these.
            }
            CURRENT++;
        }
        (scrollRegion.content.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lastHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
