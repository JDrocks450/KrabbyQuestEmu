﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        string fileName = Path.Combine(TextureLoader.AssetDirectory, "music", "res3.ogg");
        WWW data = new WWW(fileName);
        while (!data.isDone) { }
        AudioClip ac = data.GetAudioClipCompressed(false, AudioType.OGGVORBIS) as AudioClip;
        var source = GameObject.Find("Music").GetComponent<AudioSource>();
        source.clip = ac;
        source.Play();        
    }

    public void GoToMapScreen()
    {
        SceneManager.LoadScene("MapScreen");
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}