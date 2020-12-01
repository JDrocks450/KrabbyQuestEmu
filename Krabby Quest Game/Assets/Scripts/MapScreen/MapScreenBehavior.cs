using StinkyFile;
using StinkyFile.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapScreenBehavior : MonoBehaviour
{
    Map3DLoader mapScreen;
    PopulateLevels levelPopulator;

    static StinkyLevel CurrentLevel;

    // Start is called before the first frame update
    void Start()
    {
        mapScreen = GameObject.Find("Map3D").GetComponent<Map3DLoader>();
        levelPopulator = GameObject.Find("Level Scroll").GetComponent<PopulateLevels>();
        levelPopulator.OnLevelsLoaded += delegate 
        { SelectLevelOnMap(CurrentLevel, true); };
    }    

    public void GoToLevel_OnClick(Button button) => GoToLevel(button.tag as string);

    public void GoToLevel(string levelName)
    {        
        LevelObjectManager.ChangeLevel(levelName, true);
    }

    public void SelectLevelOnMap(StinkyLevel level, bool scrollTo = false)
    {
        if (level == null) return;
        mapScreen.ChangeSelectedLevel(level);
        CurrentLevel = level;
        if (scrollTo)
            levelPopulator.ScrollToLevel(level);
    }

    // Update is called once per frame
    void Update()
    {
        //rotate skybox
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * .5f);
    }
}
