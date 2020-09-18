using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using StinkyFile;
using System.IO;
using System;
using Assets;
using B83.Image.BMP;
using System.Xml.Linq;
using System.Threading.Tasks;
using UnityEngine.Audio;

public class LevelObjectManager : MonoBehaviour    
{
    public static Vector2 Grid_Size = new Vector2(2,2);
    IEnumerable<AssetDBEntry> textures;
    static string AssetDirectory => TextureLoader.AssetDirectory;
    static Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();
    
    /// <summary>
    /// The current <see cref="StinkyParser"/> instance -- do not use this unless necessary!!
    /// </summary>
    public static StinkyParser Parser = new StinkyParser();
    /// <summary>
    /// The current open level
    /// </summary>
    public static StinkyLevel Level;

    /// <summary>
    /// The current level context
    /// </summary>
    public static LevelContext Context
    {
        get; set;
    }

    StinkyFile.BlockLayers CurrentLoadingLayer;
    int X, Y;
    bool levelLoadingComplete = false, isLoadingLevel = false;
    public static string LoadLevelName = null;
    static bool sceneUnloading = false;

    public static bool IsDone
    {
        get; private set;
    } = false;
    bool showingTitleCard = false, canShowTitleCard = true;
    float titleCardTimer = 0f;
    static LevelObjectManager CurrentObjectManager;
    static AudioSource LevelMusic;
    // Start is called before the first frame update
    void Start()
    {
        LevelDataBlock.BlockDatabasePath = "Assets/Resources/blockdb.xml";
        AssetDBEntry.AssetDatabasePath = "Assets/Resources/texturedb.xml";
        XDocument doc = XDocument.Load(AssetDBEntry.AssetDatabasePath);
        TextureLoader.AssetDirectory = doc.Root.Element("WorkspaceDirectory").Value;   
        LevelMusic = GameObject.Find("Level Music Provider").GetComponent<AudioSource>();
        IsDone = false;
        if (LoadLevelName != null)        
            LoadLevel();        
    }

    public static void ChangeLevel(string levelName)
    {                        
        LoadLevelName = levelName;
        var operation = SceneManager.LoadSceneAsync("Game");
        operation.completed += delegate
        {
            //sceneUnloading = true;
            //var unloadOp = SceneManager.UnloadSceneAsync(0);
            //unloadOp.completed += delegate { sceneUnloading = false; };
        };               
    }

    void LoadLevel()
    {
        if (!IsDone)
        {
            CurrentObjectManager = this;
            PlayMusic(Context);
            var path = Path.Combine(AssetDirectory, "levels", LoadLevelName + ".lv5");
            Level = Parser.LevelRead(path);
            Context = Level.Context;
            Parser.RefreshLevel(Level);
            isLoadingLevel = true;
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
            case LevelContext.CAVES:
                file = "res6.ogg";
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
                    LoadNext();
            else IsDone = true;
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
    }

    bool LoadNext()
    {
        if (levelLoadingComplete)
            return false;
        LevelDataBlock block = (CurrentLoadingLayer == BlockLayers.Integral ? Level.IntegralData : Level.DecorationData)[Y * Level.Columns + X];
        var GObject = CurrentLoadingLayer == BlockLayers.Integral ? GetObject(block) : GetDecoration(block);
        if (GObject != null)
        {
            var transform = GObject.transform;
            transform.position = new Vector3(-X * Grid_Size.x, transform.position.y, Y * Grid_Size.y);
            transform.rotation = Quaternion.identity;
            float angle = 0;
            switch (block.Rotation)
            {
                case SRotation.NORTH: angle = 0;   break;
                case SRotation.EAST: angle = 90;   break;
                case SRotation.WEST: angle = -90;  break;
                case SRotation.SOUTH: angle = 180; break;                    
            }
            transform.RotateAround(transform.position, Vector3.up, angle);
            GObject.name = block.Name;
        }
        if (X >= Level.Columns-1)
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
        return true;
    }

    private bool TryGetByParameter(LevelDataBlock blockData, out GameObject byParameter)
    {
        byParameter = GetObjectByParameter(blockData);
        return byParameter != default;
    }

    private GameObject GetDecoration(LevelDataBlock block)
    {
        GameObject decoration = null;
        textures = block.GetReferences(AssetType.Texture);
        if (TryGetByParameter(block, out var byParameter))
            return byParameter; // try getting by parameter
        return default;
    }             

    GameObject GetObject(LevelDataBlock Data)
    {
        GameObject wallObject = null;        
        if (TryGetByParameter(Data, out var byParameter))
            return byParameter; // try getting by parameter
        return (GameObject)Instantiate(ResourceLoad("Objects/UnknownObject"));
    }

    GameObject ResourceLoad(string Name)
    {
        if (LoadedPrefabs.TryGetValue(Name, out var resource))
            return resource;
        LoadedPrefabs.Add(Name, Resources.Load(Name) as GameObject);
        return LoadedPrefabs[Name];
    }

    GameObject GetObjectByParameter(LevelDataBlock block)
    {
        BlockParameter parameter = default;
        GameObject returnVal = default; // the object being created
        if (block.GetParameterByName("FLOOR", out parameter))
            returnVal = (GameObject)Instantiate(ResourceLoad("Objects/GroundTileObject")); // create a floor
        else if (block.GetParameterByName("WALL", out parameter))
            switch (parameter.Value)
            {
                case "Low":
                    returnVal = Instantiate(ResourceLoad("Objects/LowWallObject"));
                    break;
                case "Medium":
                    returnVal = (GameObject)Instantiate(ResourceLoad("Objects/MidWallObject"));
                    break;
                case "High":
                    returnVal = (GameObject)Instantiate(ResourceLoad("Objects/HighWallObject"));
                    break;
            }
        else if (block.GetParameterByName("Prefab", out parameter))
            returnVal = (GameObject)Instantiate(ResourceLoad("Objects/" + parameter.Value));
        else if (block.GetParameterByName("ServiceObject", out _))
            switch (block.BlockLayer)
            {
                case BlockLayers.Decoration:
                    returnVal = (GameObject)Instantiate(ResourceLoad("Objects/AnonymousObject"));
                    break;
                case BlockLayers.Integral:
                    returnVal = (GameObject)Instantiate(ResourceLoad("Objects/AnonymousIntegralObject"));
                    break;
            }
        else if (block.Name?.Contains("THROWER") ?? false)
            returnVal = (GameObject)Instantiate(ResourceLoad("Objects/TentacleCannon"));
        else if (block.Name?.StartsWith("SPROUT") ?? false)
            returnVal = (GameObject)Instantiate(ResourceLoad("Objects/TentacleSpike"));
        else if (block.HasModel)
        {
            returnVal = (GameObject)Instantiate(ResourceLoad("Objects/EmptyObject"));
            returnVal.AddComponent<ModelLoader>();
        }        
        if (returnVal == null)
            return returnVal;
        if (block.HasSound && !returnVal.TryGetComponent<AudioSource>(out _))
        {
            returnVal.AddComponent<SoundLoader>().LoadAll(block); // load all sound effects related to the object
        }
        if (!returnVal.TryGetComponent<AllowTileMovement>(out _))
            returnVal.AddComponent<AllowTileMovement>();
        if (block.GetParameterByName("Script", out parameter))
            returnVal.AddComponent(Type.GetType(parameter.Value, true));
        if (block.Name.StartsWith("Message"))
            returnVal.AddComponent<MessageBehavior>();
        if (block.GetParameterByName("PosY", out parameter))
        {
            var pos = returnVal.transform.position;
            returnVal.transform.position = new Vector3(pos.x, float.Parse(parameter.Value), pos.z);
        }
        var blockComponent = returnVal.AddComponent<DataBlockComponent>();
        blockComponent.DataBlock = block;
        blockComponent.WorldTileX = X;
        blockComponent.WorldTileY = Y;
        blockComponent.Parent = returnVal;
        return returnVal;
    }
}
