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

public class LevelObjectManager : MonoBehaviour    
{
    public static Vector2 Grid_Size = new Vector2(2,2);
    IEnumerable<AssetDBEntry> textures;
    string AssetDirectory = TextureLoader.AssetDirectory;  
    
    /// <summary>
    /// The current <see cref="StinkyParser"/> instance -- do not use this unless necessary!!
    /// </summary>
    public static StinkyParser Parser;
    public static StinkyLevel Level => Parser.OpenLevel;

    StinkyFile.BlockLayers CurrentLoadingLayer;

    int X, Y;
    bool levelLoadingComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        LevelDataBlock.BlockDatabasePath = "Assets/Resources/blockdb.xml";
        AssetDBEntry.AssetDatabasePath = "Assets/Resources/texturedb.xml";
        StinkyParser.LoadLevelFile(Path.Combine(AssetDirectory, "levels", "5.lv5"), out Parser);
        LoadNext();
    }

    // Update is called once per frame
    void Update()
    {
        if (!levelLoadingComplete)
            for(int i = 0; i < 10; i++)
            LoadNext();
    }

    bool LoadNext()
    {
        if (levelLoadingComplete)
            return false;
        LevelDataBlock block = (CurrentLoadingLayer == BlockLayers.Integral ? Level.IntegralData : Level.DecorationData)[Y * Level.Columns + X];
        var GObject = CurrentLoadingLayer == BlockLayers.Integral ? GetObject(block) : GetDecoration(block);
        if (GObject != null)
        {
            GObject.transform.position = new Vector3(-X * Grid_Size.x, GObject.transform.position.y, Y * Grid_Size.y);
            GObject.transform.rotation = Quaternion.identity;
            float angle = 0;
            switch (block.Rotation)
            {
                case SRotation.NORTH: angle = 0; break;
                case SRotation.EAST: angle = 90; break;
                case SRotation.WEST: angle = -90; break;
                case SRotation.SOUTH: angle = 180; break;                    
            }
            GObject.transform.RotateAround(GObject.transform.position, Vector3.up, angle);
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
        return (GameObject)Instantiate(Resources.Load("Objects/UnknownObject"));
    }

    GameObject GetObjectByParameter(LevelDataBlock block)
    {
        BlockParameter parameter = default;
        GameObject returnVal = default; // the object being created
        if (block.GetParameterByName("FLOOR", out parameter))
            returnVal = (GameObject)Instantiate(Resources.Load("Objects/GroundTileObject")); // create a floor
        else if (block.GetParameterByName("WALL", out parameter))
            switch (parameter.Value)
            {
                case "Low":
                    returnVal = (GameObject)Instantiate(Resources.Load("Objects/LowWallObject"));
                    break;
                case "Medium":
                    returnVal = (GameObject)Instantiate(Resources.Load("Objects/MidWallObject"));
                    break;
                case "High":
                    returnVal = (GameObject)Instantiate(Resources.Load("Objects/HighWallObject"));
                    break;
            }
        else if (block.GetParameterByName("Prefab", out parameter) && parameter.Value != "WoodenSign")
            returnVal = (GameObject)Instantiate(Resources.Load("Objects/" + parameter.Value));
        else if (block.GetParameterByName("ServiceObject", out _))
            switch (block.BlockLayer)
            {
                case BlockLayers.Decoration:
                    returnVal = (GameObject)Instantiate(Resources.Load("Objects/AnonymousObject"));
                    break;
                case BlockLayers.Integral:
                    returnVal = (GameObject)Instantiate(Resources.Load("Objects/AnonymousIntegralObject"));
                    break;
            }
            
        else if (block.HasModel)
        {
            returnVal = (GameObject)Instantiate(Resources.Load("Objects/EmptyObject"));
            returnVal.AddComponent<ModelLoader>();
        }
        if (returnVal == null)
            return returnVal;
        
        if (block.GetParameterByName("Script", out parameter))
            returnVal.AddComponent(Type.GetType(parameter.Value, true));
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
