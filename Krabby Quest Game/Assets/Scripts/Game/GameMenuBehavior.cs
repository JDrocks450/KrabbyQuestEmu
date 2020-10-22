using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Version Number").GetComponent<Text>().text = $"Version {Application.version} - Unity Version {Application.unityVersion} - Jeremy GlaZebrook"; // if you change this im killing u :)
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMap() => LevelObjectManager.SignalLevelCompleted(false);

    public void SkipLevel() => LevelObjectManager.SignalLevelCompleted(true);

    public void RestartLevel() => LevelObjectManager.ReloadLevel();
}
