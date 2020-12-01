using Assets.Components;
using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopulateLevels : MonoBehaviour
{
    Button sampleButton;
    StinkyLevel selected;

    public event EventHandler OnLevelsLoaded;
    Dictionary<StinkyLevel, RectTransform> transforms = new Dictionary<StinkyLevel, RectTransform>();

    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        ScrollRect scrollRegion = GetComponent<ScrollRect>();
        var mapScreenBhav = GameObject.Find("EventSystem").GetComponent<MapScreenBehavior>();
        sampleButton = scrollRegion.content.transform.GetChild(0).gameObject.GetComponent<Button>();
        ButtonStyler.Style(sampleButton);
        var parser = GameInitialization.GlobalParser;
        parser.FindAllLevels(Path.Combine(TextureLoader.AssetDirectory, "levels"));
        int lastHeight = 0, CURRENT = 0, lastAvailableLevel = 0;
        transforms.Clear();
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
            transforms.Add(level, transform);
            CURRENT++;
        }
        (scrollRegion.content.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lastHeight);
        OnLevelsLoaded?.Invoke(this, null);
    }

    public void ScrollToLevel(string LevelWorldName)
    {
        ScrollToLevel(transforms.Keys.FirstOrDefault(x => x.LevelWorldName == LevelWorldName));
    }

    public void ScrollToLevel(StinkyLevel level)
    {
        if (level == default) return;
        ScrollIntoView(transforms[level]);
    }

    void ScrollIntoView(RectTransform target)
    {
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        var contentPanel = scrollRect.content;
        contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x,
            ((Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position)).y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
