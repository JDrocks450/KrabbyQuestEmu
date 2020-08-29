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

    string AssetDirectory = "D:\\Projects\\Krabby Quest\\FileDump";
    IEnumerable<AssetDBEntry> textures;
    StinkyParser Parser;
    StinkyLevel Level => Parser.OpenLevel;

    StinkyFile.BlockLayers CurrentLoadingLayer;

    int X, Y;
    bool levelLoadingComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        Parser = new StinkyParser("D:\\Projects\\Krabby Quest\\res2.dat");
        LevelDataBlock.BlockDatabasePath = "Assets/Resources/blockdb.xml";
        AssetDBEntry.AssetDatabasePath = "Assets/Resources/texturedb.xml";
        Parser.LevelRead(Parser.LevelIndices[Parser.LevelIndices.Keys.ElementAt(1)]);
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

    private Texture2D RequestTexture(string path)
    {
        if (path.ToLower().EndsWith("bmp"))
        {
            var bmp = new BMPLoader().LoadBMP(path);
            return bmp.ToTexture2D();
        }
        else
        {
            var request = new WWW("file:///" + path);
            while (!request.isDone) { }
            return request.texture;
        }
    }

    private void ApplyTextureMaterial(Renderer Object, int TextureIndex)
    {
        var material = (Material)Instantiate(Resources.Load("Materials/Object Material"));
        string path = Path.Combine(AssetDirectory, textures.ElementAt(TextureIndex).FileName);        
        material.mainTexture = RequestTexture(path);
        Object.material = material;
    }
    private void ApplyTextureMaterial(Renderer Object, string TextureName)
    {
        var material = (Material)Instantiate(Resources.Load("Materials/Object Material"));
        string path = Path.Combine(AssetDirectory, TextureName);        
        material.mainTexture = RequestTexture(path);
        Object.material = material;
    }

    void CreateWall(GameObject wallObject, string ObjName)
    {
        ApplyTextureMaterial(wallObject.GetComponent<Renderer>(), 1);
        ApplyTextureMaterial(wallObject.transform.GetChild(0).GetComponent<Renderer>(), 0);
    }

    private GameObject CreateFloor(LevelDataBlock Data)
    {
        var gobject = (GameObject)Instantiate(Resources.Load("Objects/GroundTileObject"));
        ApplyTextureMaterial(gobject.GetComponent<Renderer>(), 0);
        return gobject;
    }

    GameObject GetObject(LevelDataBlock Data)
    {
        GameObject wallObject = null;
        textures = Data.GetReferences(AssetType.Texture);
        if (TryGetByParameter(Data, out var byParameter))
            return byParameter; // try getting by parameter
        switch (Data.Name)
        {
            case "WALL_LOW":                
                wallObject = (GameObject)Instantiate(Resources.Load("Objects/LowWallObject"));
                CreateWall(wallObject, Data.Name);
                return wallObject;
            case "WALL_MID":
                wallObject = (GameObject)Instantiate(Resources.Load("Objects/MidWallObject"));
                CreateWall(wallObject, Data.Name);
                return wallObject;
            case "WALL_HIGH":
                wallObject = (GameObject)Instantiate(Resources.Load("Objects/HighWallObject"));
                CreateWall(wallObject, Data.Name);
                return wallObject;
            default:
                {
                    return (GameObject)Instantiate(Resources.Load("Objects/UnknownObject"));
                }
                break;
        }
        return default;
    }

    GameObject GetObjectByParameter(LevelDataBlock block)
    {
        BlockParameter parameter = default;
        GameObject returnVal = default; // the object being created
        if (block.GetParameterByName("FLOOR", out parameter))
            returnVal = CreateFloor(block); // create a floor
        else if (block.GetParameterByName("Prefab", out parameter) && parameter.Value != "WoodenSign")
            returnVal = (GameObject)Instantiate(Resources.Load("Objects/" + parameter.Value));
        else if (block.GetParameterByName("ServiceObject", out _))
            returnVal = (GameObject)Instantiate(Resources.Load("Objects/AnonymousObject"));
        if (returnVal == null)
            return returnVal;
        if (block.GetParameterByName("FillTexture", out parameter))
            ApplyTextureMaterial(returnVal.GetComponent<Renderer>(), int.Parse(parameter.Value));
        if (block.GetParameterByName("Script", out parameter))
            returnVal.AddComponent(Type.GetType(parameter.Value, true));
        var blockComponent = returnVal.AddComponent<DataBlockComponent>();
        blockComponent.DataBlock = block;
        blockComponent.WorldTileX = X;
        blockComponent.WorldTileY = Y;
        blockComponent.Parent = returnVal;
        return returnVal;
    }
}
