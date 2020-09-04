using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModelLoader : MonoBehaviour
{
    static Dictionary<string, Mesh> LoadedContent = new Dictionary<string, Mesh>();
    static Dictionary<string, LevelDataBlock> TemplatedItems = new Dictionary<string, LevelDataBlock>();
    static string AssetDirectory => TextureLoader.AssetDirectory;
    IEnumerable<AssetDBEntry> models;
    LevelDataBlock Data;
    DataBlockComponent TileComponent;
    private static LevelDataBlock _floor;
    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out TileComponent))
            TileComponent = GetComponentInParent<DataBlockComponent>();
        Data = TileComponent?.DataBlock;
        if (Data == null)
            return;
        if (Data.GetParameterByName("Primitive", out var param))
        {
            switch (param.Value)
            {
                case "Ellipse":
                    gameObject.AddComponent<MeshRenderer>();
                    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                    filter.mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh;
                    break;
            }
        }
        else
        {
            models = Data.GetReferences(AssetType.Model);
            ImportMesh(models.ElementAt(0).FileName);
        }
        if (Data.HasTexture)
            gameObject.AddComponent<TextureLoader>().LookIntoParent = true; // adds the texture loader to apply a texture to the model
        float scaleX = 1f, scaleY = 1f, scaleZ = 1f;
        if (Data.GetParameterByName("uScale", out var scaleParam))
        {
            if (float.TryParse(scaleParam.Value, out var number))
            {
                scaleX = scaleY = scaleZ = number;
            }
        }
        if (Data.GetParameterByName("ScaleX", out scaleParam))
        {
            if (float.TryParse(scaleParam.Value, out float scalar))
                scaleX = scalar;
        }
        if (Data.GetParameterByName("ScaleY", out scaleParam))
        {
            if (float.TryParse(scaleParam.Value, out float scalar))
                scaleY = scalar;
        }
        if (Data.GetParameterByName("ScaleZ", out scaleParam))
        {
            if (float.TryParse(scaleParam.Value, out float scalar))
                scaleZ = scalar;
        }
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    void ImportMesh(string fileName)
    {
        Mesh holderMesh = new Mesh();
        holderMesh = new Assets.ObjImporter().ImportFile(Path.Combine(AssetDirectory, fileName));

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = holderMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
