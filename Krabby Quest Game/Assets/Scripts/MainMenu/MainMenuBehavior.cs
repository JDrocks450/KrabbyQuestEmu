using Assets.Components;
using StinkyFile.Save;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        GameObject.Find("Version Number").GetComponent<Text>().text = $"Version {Application.version} - Unity Version {Application.unityVersion} - Jeremy GlaZebrook"; // if you change this im killing u :)
        string fileName = Path.Combine(TextureLoader.AssetDirectory, "music", "res3.ogg");
        WWW data = new WWW(fileName);
        while (!data.isDone) { }
        AudioClip ac = data.GetAudioClipCompressed(false, AudioType.OGGVORBIS) as AudioClip;
        var source = GameObject.Find("Music").GetComponent<AudioSource>();
        source.clip = ac;
        source.Play();
    }

    public void CloseBETAPrompt()
    {
        GameObject.Find("Canvas").transform.GetChild(3).gameObject.SetActive(false);
    }

    public void CloseSaveSelect()
    {
        SoundLoader.Play("sb-type.wav", false).volume = .5f;
        GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(false);
    }

    public void OpenSaveSelect()
    {
        SoundLoader.Play("sb-type.wav", false).volume = .5f;
        GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(true);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void GoToMapScreen(SaveFile Current)
    {
        SoundLoader.Play("sb-type.wav", false).volume = .5f;
        SaveFileManager.SetCurrentSaveFile(Current);
        Current.FullLoad();
        SceneManager.LoadScene("MapScreen");
    } 

    // Update is called once per frame
    void Update()
    {
        //rotate skybox
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * .5f);
    }
}
