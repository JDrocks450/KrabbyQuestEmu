using B83.Image.BMP;
using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TextureLoader : MonoBehaviour
{
    static Dictionary<string, Texture2D> LoadedContent = new Dictionary<string, Texture2D>();
    static Dictionary<string, LevelDataBlock> TemplatedItems = new Dictionary<string, LevelDataBlock>();
    IEnumerable<AssetDBEntry> textures;
    LevelDataBlock Data;
    DataBlockComponent TileComponent;
    private static LevelDataBlock _floor;
    public const string AssetDirectory = "D:\\Projects\\Krabby Quest\\Workspace";
    public bool LookIntoParent = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out TileComponent) && LookIntoParent)
            TileComponent = GetComponentInParent<DataBlockComponent>();
        Data = TileComponent?.DataBlock;
        if (Data == null)
            Data = ApplyTileData(); // try to apply templated data
        if (Data == null)
            return; // outright failure
        textures = Data.GetReferences(AssetType.Texture); // get referenced textures        
        if (Data.GetParameterByName("FLOOR", out _)) // floors
            CreateFloor();        
        else if (Data.GetParameterByName("WALL", out _)) // walls
            CreateWall();
        else if (Data.GetParameterByName("FillTexture", out var parameter)) // generic fill texture parameter info
            ApplyTextureMaterial(GetComponent<Renderer>(), int.Parse(parameter.Value));
        else 
            ApplyTextureMaterial(GetComponent<Renderer>(), 0);
    }

    /// <summary>
    /// Applies template tile information for objects of certain types -- GroundTileObject will be applied default floor parameters, etc.
    /// </summary>
    LevelDataBlock ApplyTileData()
    {
        if (TemplatedItems.TryGetValue(name, out var data))
            return data;
        if (name == "GroundTileObject")
        {
            TemplatedItems.Add(name, LevelObjectManager.Level.IntegralData.FirstOrDefault(x => x.Name == "SAND"));
            return TemplatedItems[name];
        }
        return default;   
    }
    private void ApplyTextureMaterial(Renderer Object, int TextureIndex) => ApplyTextureMaterial(Object, textures.ElementAt(TextureIndex).FileName);
    private void ApplyTextureMaterial(Renderer Object, string TextureName)
    {
        var materialpath = "Materials/Object Material";
        if (Data.GetParameterByName("Material", out var value))
            materialpath = "Materials/" + value.Value;
        var material = (Material)Instantiate(Resources.Load(materialpath));
        string path = Path.Combine(AssetDirectory, TextureName);        
        material.mainTexture = RequestTexture(path);        
        Object.material = material;
    }
    private Texture2D RequestTexture(string path)
    {
        if (LoadedContent.TryGetValue(path, out var texture))
            return texture;
        if (path.ToLower().EndsWith("bmp"))
        {
            var bmp = new BMPLoader().LoadBMP(path);
            if (Data.GetParameterByName("TransparentColor", out var colorInfo))
            {
                string[] separated = colorInfo.Value.Split(',');
                Color32 transColor = 
                    new Color32(byte.Parse(separated[0]), byte.Parse(separated[1]), byte.Parse(separated[2]), 255);                
                for(int i = 0; i < bmp.imageData.Length; i++)
                {
                    var color = bmp.imageData[i];
                    if (color.r == transColor.r && color.g == transColor.g && color.b == transColor.b)                    
                        bmp.imageData[i] = new Color32(0, 0, 0, 0);                    
                }
            }
            LoadedContent.Add(path, bmp.ToTexture2D());
            return LoadedContent[path];
        }
        else
        {
            var request = new WWW("file:///" + path);
            while (!request.isDone) { }
            LoadedContent.Add(path, request.texture);
            return request.texture;
        }
    }   
    void CreateWall()
    {
        ApplyTextureMaterial(GetComponent<Renderer>(), 1);
        ApplyTextureMaterial(transform.GetChild(0).GetComponent<Renderer>(), 0);
    }
    private void CreateFloor()
    {
        ApplyTextureMaterial(GetComponent<Renderer>(), 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
