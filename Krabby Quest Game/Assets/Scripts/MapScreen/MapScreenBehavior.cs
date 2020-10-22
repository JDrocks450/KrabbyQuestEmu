using StinkyFile;
using StinkyFile.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapScreenBehavior : MonoBehaviour
{
    Map3DLoader mapScreen;

    // Start is called before the first frame update
    void Start()
    {
        mapScreen = GameObject.Find("Map3D").GetComponent<Map3DLoader>();
    }

    public void GoToLevel_OnClick(Button button) => GoToLevel(button.tag as string);

    public void GoToLevel(string levelName)
    {
        LevelObjectManager.ChangeLevel(levelName);
    }

    public void SelectLevelOnMap(StinkyLevel level) => mapScreen.ChangeSelectedLevel(level);

    // Update is called once per frame
    void Update()
    {
        
    }
}
