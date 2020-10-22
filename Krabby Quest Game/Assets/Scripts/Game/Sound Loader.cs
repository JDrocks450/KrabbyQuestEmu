using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundLoader : MonoBehaviour
{
    const int MAX_SOURCES = 5;
    static Dictionary<string, AudioClip> SoundEffects = new Dictionary<string, AudioClip>();
    static AudioSource[] Sources = new AudioSource[5];

    /// <summary>
    /// The sound effect will only be played if no other sound source is playing that sound effect in this mode
    /// </summary>
    public bool ExclusiveSoundMode
    {
        get; set;
    }
    /// <summary>
    /// All the audio clips this object references
    /// </summary>
    public string[] SoundReferences;
    /// <summary>
    /// All the resources related to this object are loaded
    /// </summary>
    public bool Loaded
    {
        get; private set;
    } = false;
    // Start is called before the first frame update
    void Start()
    {
        if (Sources[0] == null)
            Sources[0] = GameObject.Find("Sound FX").GetComponent<AudioSource>();
    }

    /// <summary>
    /// Loads all referenced sound effects into the Sound Cache
    /// </summary>
    /// <param name="DataBlock"></param>
    public void LoadAll(LevelDataBlock DataBlock)
    {
        List<string> references = new List<string>();
        foreach (var reference in DataBlock.GetReferences(AssetType.Sound))
        {
            references.Add(reference.DBName);
            if (SoundEffects.ContainsKey(reference.DBName))
                continue;
            var request = new WWW(Path.Combine(TextureLoader.AssetDirectory, reference.FileName));
            while (request.isDone != true) { }
            var audio = request.GetAudioClipCompressed();
            audio.name = reference.DBName;
            SoundEffects.Add(reference.DBName, audio);
        }
        SoundReferences = references.ToArray();
        Loaded = true;
    }

    public AudioClip GetAudio(string DBName) => SoundEffects[DBName];

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateSource(int index)
    {
        if (Sources[index] == null)
        {
            var source = Instantiate(Sources[0]);
            source.Stop();
            source.clip = null;
            Sources[index] = source;
        }
    }

    public void Play(int Index) => Play(SoundReferences[Index]);

    public void Play(string DBName) => Play(GetAudio(DBName));

    public void Play(AudioClip clip)
    {
        for (int i = 0; i < MAX_SOURCES; i++)
        {
            if (Sources[i] == null)
                CreateSource(i);
            var source = Sources[i];  
            if (ExclusiveSoundMode)
            {
                if (source.isPlaying && source.clip.name == clip.name)
                    break; // cancel this play request due to the sound already playing
            }
            if (!source.isPlaying || i == MAX_SOURCES - 1)
            {                
                source.clip = clip;
                source.Play();
                return;
            }
        }
    }
}
