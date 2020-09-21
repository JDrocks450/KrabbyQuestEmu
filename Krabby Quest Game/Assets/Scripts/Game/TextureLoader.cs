using B83.Image.BMP;
using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TextureLoader : MonoBehaviour
{
    static Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
    static Dictionary<string, Texture2D> LoadedContent = new Dictionary<string, Texture2D>();
    static Dictionary<string, LevelDataBlock> TemplatedItems = new Dictionary<string, LevelDataBlock>();
    IEnumerable<AssetDBEntry> textures;
    LevelDataBlock Data;
    DataBlockComponent TileComponent;
    private static LevelDataBlock _floor;
    private static Material DefaultMaterial;
    public static string AssetDirectory    
    {
        get; set;
    }
    bool Loaded = false;
    public bool LookIntoParent = false;
    public bool ForceTemplate = false;

    private void Awake()
    {
        if (!TryGetComponent(out TileComponent) && LookIntoParent && !ForceTemplate)
            TileComponent = GetComponentInParent<DataBlockComponent>();
        if (TileComponent?.TextureLoaded ?? false) 
            return;
        Data = TileComponent?.DataBlock;
        if (DefaultMaterial == null)
            DefaultMaterial = (Material)Instantiate(Resources.Load("Materials/Object Material"));
        if (Data == null || ForceTemplate)
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
        if (TileComponent != null)
            TileComponent.TextureLoaded = true;
        Loaded = true;
    }
    void Start()
    {
        if (!Loaded)
            Awake(); // some objects cannot load instantly - wait until the first frame update to do this loading
        DestroyImmediate(this);
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
        else if (name == "NorthPlane" || name == "EastPlane" || name == "SouthPlane" || name == "WestPlane"
            || name == "Bottom") // water tile
        {
            TemplatedItems.Add(name, LevelObjectManager.Level.IntegralData.FirstOrDefault(x => x.Name == "BIKINI_WATER" || x.Name == "WATER_GOO_LAGOON"));
            return TemplatedItems[name];
        }
        return default;   
    }
    private void ApplyTextureMaterial(Renderer Object, int TextureIndex) => ApplyTextureMaterial(Object, textures.ElementAt(TextureIndex).FileName);
    private void ApplyTextureMaterial(Renderer Object, string TextureName)
    {
        if (!MaterialCache.TryGetValue(TextureName, out Material material))
        {
            material = null;
            if (Data.GetParameterByName("Material", out var value))
            {
                string materialpath = "Materials/" + value.Value;
                material = (Material)Instantiate(Resources.Load(materialpath));
            }
            if (material == null)
            {
                material = Instantiate(DefaultMaterial);
                material.name = TextureName + " Material";
            }
            if (material.mainTexture == null)
            {
                string path = Path.Combine(AssetDirectory, TextureName);
                Data.GetParameterByName("TransparentColor", out var colorInfo);
                material.mainTexture = RequestTexture(path, colorInfo?.Value ?? default);
                MaterialCache.Add(TextureName, material);
            }
        }
        Object.material = material;
    }

    public static Texture2D RequestTexture(string path, string TransparentColor = default, bool includeAssetDir = false)
    {
        GameInitialization.Initialize();
        if (includeAssetDir)
            path = Path.Combine(AssetDirectory, path);
        if (LoadedContent.TryGetValue(path, out var texture))
            return texture;
        if (path.ToLower().EndsWith("bmp"))
        {
            var bmp = new BMPLoader().LoadBMP(path);
            if (TransparentColor != default)
            {
                var colorInfo = TransparentColor;
                string[] separated = colorInfo.Split(',');
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
            if (request.error != null)
                Debug.LogWarning("[TextureLoader]: " + request.error);
            LoadedContent.Add(path, request.texture);
            return request.texture;
        }
    }   
    void CreateWall()
    {
        int TopIndex = 0, WallIndex = 1;
        switch (LevelObjectManager.Context)
        {
            case LevelContext.BEACH:
                TopIndex = 2;
                WallIndex = 3;
                break;
            case LevelContext.FIELDS:
                TopIndex = 5;
                WallIndex = 4;
                break;
            case LevelContext.KELP:
                TopIndex = 7;
                WallIndex = 6;
                break;
            case LevelContext.CAVES:
                TopIndex = 9;
                WallIndex = 8;
                break;
        }
        ApplyTextureMaterial(GetComponent<Renderer>(), WallIndex);
        ApplyTextureMaterial(transform.GetChild(0).GetComponent<Renderer>(), TopIndex);
    }
    private void CreateFloor()
    {
        int index = (int)LevelObjectManager.Context - 2;
        var value = Data.Parameters.Values.FirstOrDefault(x => x.Name == "Tex_Context_" + index)?.Value ?? null;
        if (value != null)        
            index = int.Parse(value);        
        if (index > textures.Count() - 1)
            index = 0;
        ApplyTextureMaterial(GetComponent<Renderer>(), index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
