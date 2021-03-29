using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrabbyQuestObjectLoader : MonoBehaviour
{
    static Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();

    public enum KQLoaderMode
    {
        Name,
        GUID
    }
    public string BlockName, BlockGUID;
    public KQLoaderMode Mode;
    public bool AutoLoad = true;
    private bool loaded;
    public bool RemoveAnimation = true;
    public Material ApplyMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        if (AutoLoad)
            Load();
    }

    private void Load()
    {
        if (loaded) return;
        GameInitialization.Initialize();
        LevelDataBlock block = default;
        switch (Mode)
        {
            case KQLoaderMode.GUID:
                block = LevelDataBlock.LoadFromDatabase(BlockGUID, out bool success);
                break;
            case KQLoaderMode.Name:
                block = LevelDataBlock.LoadFromDatabase(BlockName);
                break;
        }
        if (block != null)
        {
            var newObject = CreateKrabbyQuestObject(block, 0,0, ApplyMaterial);
            newObject.SetActive(true);
            var lTransform = newObject.transform;
            var scale = newObject.transform.localScale;
            lTransform.parent = transform;
            lTransform.localPosition = new Vector3(0, 0, 0);
            lTransform.localRotation = new Quaternion();
            lTransform.localScale = scale;
            if (RemoveAnimation)
            {
                var anim = newObject.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    if (Application.isPlaying)
                        Destroy(anim);
                }
            }
            newObject.name = block.Name;
            Debug.Log(block.Name + " loaded");
        }
        loaded = true;
    }

    public static GameObject ResourceLoad(string Name)
    {
        if (LoadedPrefabs.TryGetValue(Name, out var resource))
            return resource;
        try
        {
            var obj = Resources.Load(Name) as GameObject;
            obj.SetActive(false);
            LoadedPrefabs.Add(Name, obj);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error loading Object: " + Name + ". Object was not Defined in Resources/Objects, yet references a Prefab using Parameters.");
            return null;
        }        
        return LoadedPrefabs[Name];
    }

    /// <summary>
    /// Creates the object without any additional scripts from the given block info
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public static GameObject CreateKrabbyQuestObject(LevelDataBlock block,int X = 0, int Y = 0, Material ApplyMaterial = default)
    {
        Debug.Log("Creating object " + block.Name);
        void applyLevelBlock(GameObject obj)
        {            
            var blockComponent = obj.AddComponent<DataBlockComponent>();
            blockComponent.DataBlock = block;
            blockComponent.WorldTileX = X;
            blockComponent.WorldTileY = Y;
            blockComponent.Parent = obj;
        }
        GameObject returnVal = default;
        BlockParameter parameter = default;
        if (block.GetParameterByName("FLOOR", out parameter))
            returnVal = Instantiate(ResourceLoad("Objects/GroundTileObject")); // create a floor
        else if (block.GetParameterByName("WALL", out parameter))
            switch (parameter.Value)
            {
                case "Low":
                    returnVal = Instantiate(ResourceLoad("Objects/LowWallObject"));
                    break;
                case "Medium":
                    returnVal = Instantiate(ResourceLoad("Objects/MidWallObject"));
                    break;
                case "High":
                    returnVal = Instantiate(ResourceLoad("Objects/HighWallObject"));
                    break;
            }
        else if (block.GetParameterByName("Prefab", out parameter))
        {
            var prefab = ResourceLoad("Objects/" + parameter.Value);
            if (prefab != null)
            {
                returnVal = Instantiate(prefab);
                var loader = returnVal.GetComponentInChildren<ModelLoader>();
                if (loader != null)
                    loader.ApplyMaterial = ApplyMaterial;
            }
        }
        else if (block.GetParameterByName("ServiceObject", out _))
        {
            switch (block.BlockLayer)
            {
                case BlockLayers.Decoration:
                    returnVal = Instantiate(ResourceLoad("Objects/AnonymousObject"));
                    break;
                case BlockLayers.Integral:
                    returnVal = Instantiate(ResourceLoad("Objects/AnonymousIntegralObject"));
                    break;
            }
        }
        else if (block.Name?.Contains("THROWER") ?? false)
            returnVal = Instantiate(ResourceLoad("Objects/TentacleCannon"));
        else if (block.Name?.StartsWith("SPROUT") ?? false)
            returnVal = Instantiate(ResourceLoad("Objects/TentacleSpike"));
        else if (block.HasModel || block.GetParameterByName("ApplyTemplater", out _))
        {
            returnVal = Instantiate(ResourceLoad("Objects/EmptyObject"));
            var loader = returnVal.AddComponent<ModelLoader>();
            loader.ApplyMaterial = ApplyMaterial;
        }
        if (returnVal == null) return null;
        returnVal.name = block.Name;
        applyLevelBlock(returnVal);
        return returnVal;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
