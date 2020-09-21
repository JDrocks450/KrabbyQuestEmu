using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapScreenBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GoToLevel_OnClick(Button button) => GoToLevel(button.tag as string);

    public void GoToLevel(string levelName)
    {
        LevelObjectManager.ChangeLevel(levelName);
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
