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

    Dictionary<AudioClip, AudioSource> LoopingClips = new Dictionary<AudioClip, AudioSource>();
    public bool IsLoopingClips => false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    static AudioClip LoadFromFile(string FileName)
    {
        var name = Path.GetFileNameWithoutExtension(FileName);
        if (SoundEffects.TryGetValue(name, out var audio))
            return audio;
        var request = new WWW(Path.Combine(TextureLoader.AssetDirectory, FileName));
        while (request.isDone != true) { }
        return request.GetAudioClipCompressed();        
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
            var audio = LoadFromFile(reference.FileName);
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

    static void CreateSource(int index)
    {
        if (Sources[0] == null)
            Sources[0] = GameObject.Find("Sound FX").GetComponent<AudioSource>();
        if (Sources[index] == null)
        {
            var source = Instantiate(Sources[0]);
            source.Stop();
            source.clip = null;
            Sources[index] = source;
        }
    }

    public void Play(int Index) => Play(SoundReferences[Index]);

    public void Play(string DBName) => Play(GetAudio(DBName), ExclusiveSoundMode);

    public static AudioSource Play(AudioClip clip, bool isExclusive)
    {        
        for (int i = 0; i < MAX_SOURCES; i++)
        {
            if (Sources[i] == null)
                CreateSource(i);
            var source = Sources[i];  
            if (isExclusive)
            {
                if (source.isPlaying && source.clip.name == clip.name)
                    break; // cancel this play request due to the sound already playing
            }
            if (!source.isPlaying || i == MAX_SOURCES - 1)
            {
                source.loop = false;
                source.clip = clip;
                source.volume = 1;
                source.Play();
                return source;
            }
        }
        return null;
    }

    public static AudioSource Play(string RelativeFileName, bool isExclusive)
    {
        var audio = LoadFromFile("sound\\" + RelativeFileName);
        audio.name = Path.GetFileNameWithoutExtension(RelativeFileName);
        if (!SoundEffects.ContainsKey(audio.name))
            SoundEffects.Add(audio.name, audio);
        return Play(audio, isExclusive);
    }

    public AudioSource LoopStart(string DBName, out AudioClip clip, bool isExclusive = false)
    {
        clip = GetAudio(DBName);
        return LoopStart(clip, isExclusive);
    }
    public AudioSource LoopStart(AudioClip clip, bool isExclusive)
    {
        var source = Play(clip, isExclusive);
        if (source == null) return null; // play request cancelled
        source.loop = true;
        LoopingClips.Add(clip, source);
        return source;
    }
    public void LoopStop(AudioClip clip)
    {
        if (LoopingClips.ContainsKey(clip))
        {
            LoopingClips[clip].Stop();
            LoopingClips.Remove(clip);
        }
    }
}
