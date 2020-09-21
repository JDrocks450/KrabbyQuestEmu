using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PopulateLevels : MonoBehaviour
{
    Button sampleButton;
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
        int lastHeight = 0;
        foreach(var level in parser.LevelInfo)
        {
            var button = Instantiate(sampleButton);
            var transform = button.gameObject.transform as RectTransform;
            transform.position = new Vector3(0, -lastHeight, 0);
            lastHeight += (int)transform.rect.height + 10;
            transform.GetChild(0).GetComponent<Text>().text = level.Name;
            transform.SetParent(scrollRegion.content,false);            
            button.gameObject.SetActive(true);
            button.onClick.AddListener(delegate
            {
                mapScreenBhav.GoToLevel(Path.GetFileName(level.LevelFilePath).Replace(".lv5", ""));
            });
        }
        (scrollRegion.content.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lastHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
