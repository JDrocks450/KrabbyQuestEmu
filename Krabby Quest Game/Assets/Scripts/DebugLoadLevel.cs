using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLoadLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var button = GameObject.Find("LoadButton").GetComponent<Button>();
        button.onClick.AddListener(LoadLevelStart);
        var dropdown = GameObject.Find("D_LevelContext").GetComponent<Dropdown>();
        var enumSource = Enum.GetNames(typeof(LevelContext));
        foreach(var item in enumSource)        
            dropdown.options.Add(new Dropdown.OptionData(item));        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadLevelStart()
    {
        var text = GameObject.Find("D_LevelLoadTextBox").GetComponentInChildren<InputField>();
        LevelObjectManager.Context = (LevelContext)GameObject.Find("D_LevelContext").GetComponent<Dropdown>().value;
        var levelName = text.text;
        LevelObjectManager.ChangeLevel(levelName);
    }
}
