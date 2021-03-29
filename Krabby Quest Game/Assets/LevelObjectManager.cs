using Assets.Components.World;
using StinkyFile;
using StinkyFile.Save;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class LevelObjectManager : MonoBehaviour    
{
    public static Vector2 Grid_Size = new Vector2(2,2);
    IEnumerable<AssetDBEntry> textures;
    static string AssetDirectory => TextureLoader.AssetDirectory;
    
    /// <summary>
    /// Every tile thats loaded is cached here to make cloning them less time consuming
    /// </summary>
    Dictionary<string, GameObject> ClonableObjects = new Dictionary<string, GameObject>();    

    /// <summary>
    /// The currently loading/open world
    /// </summary>
    public World CurrentWorld => World.Current;

    /// <summary>
    /// The current open level
    /// </summary>
    public static StinkyLevel Level => World.Current?.Level;

    /// <summary>
    /// The current level context
    /// </summary>
    public static LevelContext Context => World.Current?.Context ?? LevelContext.BIKINI;
    
    /// <summary>
    /// The current progress loading the level
    /// </summary>
    public static float LoadingPercentage
    {
        get; private set;
    }

    StinkyFile.BlockLayers CurrentLoadingLayer;
    int X, Y;
    bool levelLoadingComplete = false, isLoadingLevel = false;

    /// <summary>
    /// The filename of the level to load automatically when this object awakens
    /// </summary>
    public static string LoadLevelName
    {
        get; set;
    } = null;
    static bool sceneUnloading = false, levelSwapping = false;
    /// <summary>
    /// True if the level is completely loaded
    /// </summary>
    public static bool IsDone
    {
        get; private set;
    } = false;

    static TimeSpan levelTime = new TimeSpan();
    bool showingTitleCard = false, canShowTitleCard = true;
    float titleCardTimer = 0f;
    static LevelObjectManager CurrentObjectManager;
    static AudioSource LevelMusic;
    Stopwatch debug_loadWatch = new Stopwatch();
    GameObject screenBlocker;

    /// <summary>
    /// Automatically starts loading the level using <see cref="LoadLevelName"/>
    /// </summary>
    void Awake()
    {
        GameInitialization.Initialize();
        levelSwapping = false;
        LevelMusic = GameObject.Find("Level Music Provider").GetComponent<AudioSource>();
        IsDone = false;
        if (LoadLevelName != null)        
            LoadLevel();        
    }

    void LoadAll()
    {
        if (isLoadingLevel && !sceneUnloading)
        {
            if (!levelLoadingComplete)
                for (int i = 0; i < Level.Total; i++)
                    LoadNext();
            else IsDone = true;
        }  
    }

    public static void ReloadLevel() => DoLevelOutro(delegate { ChangeLevel(LoadLevelName); });

    public static void ChangeLevel(string levelName, bool force = false)
    {
        if (force)
            levelSwapping = false;
        if (levelSwapping)
            return; // ignore all requests to change level while it's swapping
        levelSwapping = true;
        LoadLevelName = levelName;
        SceneManager.LoadSceneAsync("Game");       
    }

    private static void DoLevelOutro(Action callback)
    {
        GameObject.Find("GameCamera").GetComponent<Animator>().Play("Level Exit");
        callback.Invoke();            
    }

    public static void SignalLevelCompleted(bool completed)
    {
        if (levelSwapping) return;
        levelSwapping = true;
        if (completed)        
            World.Current.Finish((int)levelTime.TotalSeconds, completed);        
        SceneManager.LoadSceneAsync("MapScreen");        
    }

    void LoadLevel(LevelCompletionInfo completionInfo = default)
    {
        if (!IsDone)
        {
            screenBlocker = GameObject.Find("ScreenBlocker");
            CurrentObjectManager = this;
            World.SetCurrent(new World(LoadLevelName + ".lv5")); // set the current world
            PlayMusic(Context); // play the level music           
            LoadingPercentage = 0; // update the percent complete of level layer loading
            isLoadingLevel = true; 
            int timeRemaining = World.Current.Level.LevelTime; // set the time remaining for the level
            levelTime = TimeSpan.FromSeconds(timeRemaining);
            World.Current.LoadSave(completionInfo); // load save information for the current world
            Player.CurrentPlayer = Assets.Scripts.Game.PlayerEnum.ANYONE;
            LoadNext();
        }
    }

    static void PlayMusic(LevelContext Context)
    {
        var source = LevelMusic;
        LevelMusic.Stop();
        string file = "res4.ogg";
        switch (Context)
        {
            case LevelContext.BEACH:
                file = "res5.ogg";
                break;
            case LevelContext.FIELDS:
                file = "res4.ogg";
                break;
            case LevelContext.KELP:
                file = "res6.ogg";
                break;
            case LevelContext.CAVES:
                file = "res5.ogg";
                break;
        }
        string fileName = Path.Combine(AssetDirectory, "music", file);
        WWW data = new WWW(fileName);
        while (!data.isDone) { }
        AudioClip ac = data.GetAudioClipCompressed(false, AudioType.OGGVORBIS) as AudioClip;
        ac.name = Enum.GetName(typeof(LevelContext), Context) + " Level Music";
        source.clip = ac;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLoadingLevel && !sceneUnloading)
        {
            if (!levelLoadingComplete)
                for (int i = 0; i < Level.Columns; i++)
                {
                    LoadNext();
                    if (X >= Level.Columns - 1)
                    {
                        X = 0;
                        Y++;
                        if (Y >= Level.Rows)
                        {
                            Y = 0;
                            if (CurrentLoadingLayer == BlockLayers.Integral)
                                CurrentLoadingLayer = BlockLayers.Decoration;
                            else
                                levelLoadingComplete = true;
                        }
                    }
                    else X++;
                }
            else if (!IsDone)
            {
                IsDone = true;
                OnLevelLoaded();
            }
        }
        if (!showingTitleCard && IsDone && canShowTitleCard)
        {
            showingTitleCard = true;
            canShowTitleCard = false;
            titleCardTimer = 0f;
            MessagePromptBehavior.ShowMessage(Level.Name);
        }
        if (showingTitleCard && titleCardTimer > 5.0f)
        {
            showingTitleCard = false;
            MessagePromptBehavior.HideMessage();
        }
        else if (showingTitleCard) titleCardTimer += Time.deltaTime;
        if (IsDone)
        {
            //Check if all pickups are picked up
            if (CurrentWorld.AllPattiesCollected)
                SignalLevelCompleted(true);   
            CurrentWorld.UpdateTimeRemaining((int)levelTime.TotalSeconds);   
            levelTime -= TimeSpan.FromSeconds(Time.deltaTime);
            if (levelTime < TimeSpan.Zero)
                Player.KillAllPlayers();
        }
    }

    void OnLevelLoaded()
    {
        SoundLoader.Play("sb-enter.wav", true); // play level open chime
        GameObject.Find("GameCamera").GetComponent<Animator>().Play("Level Open");
        screenBlocker.SetActive(false);
    }

    bool LoadNext()
    {
        if (levelLoadingComplete)
            return false;
        var integralParent = GameObject.Find("Integral").transform;
        var decoratationParent = GameObject.Find("Decoration").transform;
        LevelDataBlock block = CurrentWorld.GetBlockAt(CurrentLoadingLayer, X, Y);
        var GObject = CurrentLoadingLayer == BlockLayers.Integral ? GetObject(block) : GetDecoration(block);
        if (GObject != null)
        {
            var transform = GObject.transform;
            transform.position = new Vector3(-X * Grid_Size.x, transform.position.y, Y * Grid_Size.y);
            transform.rotation = Quaternion.identity;
            transform.parent = CurrentLoadingLayer == BlockLayers.Integral ? integralParent : decoratationParent;
            float angle = 0;
            switch (block.Rotation)
            {
                case SRotation.NORTH: angle = 0;   break;
                case SRotation.EAST: angle = 90;   break;
                case SRotation.WEST: angle = -90;  break;
                case SRotation.SOUTH: angle = 180; break;                    
            }
            transform.RotateAround(transform.position, Vector3.up, angle);            
        }      
        LoadingPercentage = (((Y * Level.Columns) + X) / ((float)Level.Total * 2)) + (CurrentLoadingLayer == BlockLayers.Decoration ? .5f : 0.0f);
        return true;
    }

    private bool TryGetByParameter(LevelDataBlock blockData, out GameObject byParameter)
    {
        byParameter = GetObjectByParameter(blockData);
        return byParameter != default;
    }

    private GameObject GetDecoration(LevelDataBlock block)
    {
        textures = block.GetReferences(AssetType.Texture);
        if (TryGetByParameter(block, out var byParameter))
            return byParameter; // try getting by parameter
        return default;
    }

    GameObject GetObject(LevelDataBlock Data)
    {
        if (TryGetByParameter(Data, out var byParameter))
            return byParameter; // try getting by parameter
        var obj = KrabbyQuestObjectLoader.ResourceLoad("Objects/UnknownObject");
        if (obj != null)
        {
            var retVal = Instantiate(obj);
            retVal.SetActive(true);
            return retVal;
        }
        return null;
    }        

    GameObject Copy(GameObject copyFrom)
    {
        var returnVal = copyFrom;
        var oldAnimLoader = returnVal.GetComponent<AnimationLoader>();
        if (oldAnimLoader == null)
            oldAnimLoader = returnVal.GetComponentInChildren<AnimationLoader>();
        returnVal = Instantiate(returnVal);
        if (oldAnimLoader != null)
        {
            var newAnimLoader = returnVal.GetComponent<AnimationLoader>();
            if (newAnimLoader == null)
                newAnimLoader = returnVal.GetComponentInChildren<AnimationLoader>();
            if (newAnimLoader != null)
                newAnimLoader.Copy(oldAnimLoader);
        }
        var mLoader = returnVal.GetComponentInChildren<ModelLoader>();
        if (mLoader != null)
            DestroyImmediate(mLoader);
        return returnVal;
    }

    GameObject GetObjectByParameter(LevelDataBlock block)
    {        
        BlockParameter parameter = default;
        if (!ClonableObjects.TryGetValue(block.GUID, out GameObject returnVal))
        {
            bool isClonable = true;
            returnVal = KrabbyQuestObjectLoader.CreateKrabbyQuestObject(block);
            if (returnVal == null)
                return returnVal;
            if (block.HasSound)
                returnVal.AddComponent<SoundLoader>().LoadAll(block); // load all sound effects related to the object               
            returnVal.SetActive(true);
            if (isClonable)
            {
                returnVal.SetActive(false);
                if (returnVal.TryGetComponent<TextureLoader>(out var tloader))
                    DestroyImmediate(tloader);
                var modelLoader = returnVal.GetComponentInChildren<ModelLoader>();
                if (modelLoader != null)
                    DestroyImmediate(modelLoader);
                ClonableObjects.Add(block.GUID, returnVal);
                returnVal = Copy(returnVal);
                Debug.LogWarning($"{block.Name} object was loaded and cached");                               
            }
            else Debug.LogWarning($"{block.Name} object was loaded and not cached");
        }
        else
        {
            returnVal = Copy(returnVal);
            Debug.LogWarning($"{block.Name} object was loaded from cache");
        }
        if (returnVal == null)
            return returnVal;
        var Component = returnVal.GetComponent<DataBlockComponent>();
        if (Component == null)
            Component = returnVal.AddComponent<DataBlockComponent>();
        Component.DataBlock = block;
        Component.WorldTileX = X;
        Component.WorldTileY = Y;
        Component.Parent = returnVal;
        returnVal.name = block.Name;         
        returnVal.SetActive(true);
        returnVal.SetActiveRecursively(true); // SetActive for some reason doesn't work as it should -- i guess comment this out once it actually works as it says.
        if (!returnVal.TryGetComponent<AllowTileMovement>(out _))
            returnVal.AddComponent<AllowTileMovement>();
        if (block.GetParameterByName("Script", out parameter))
            returnVal.AddComponent(Type.GetType(parameter.Value, true));
        if (block.HasMessageContent)
            returnVal.AddComponent<MessageBehavior>();
        if (block.GetParameterByName("PosY", out parameter))
        {
            var pos = returnVal.transform.position;
            returnVal.transform.position = new Vector3(pos.x, float.Parse(parameter.Value), pos.z);
        }
        return returnVal;
    }
}
