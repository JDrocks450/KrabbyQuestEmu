using Assets.Components.GLB;
using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModelLoader : MonoBehaviour
{
    static Dictionary<string, Mesh> LoadedContent = new Dictionary<string, Mesh>();
    static Dictionary<string, (Mesh mesh, string textureFileName)> TemplatedItems = new Dictionary<string, (Mesh, string)>();
    static string AssetDirectory => TextureLoader.AssetDirectory;
    IEnumerable<AssetDBEntry> models;
    LevelDataBlock Data;
    DataBlockComponent TileComponent;
    private static LevelDataBlock _floor;
    public bool Loaded { get; set; }

    private void Awake()
    {
        if (Loaded) return;
        if (!TryGetComponent(out TileComponent))
            TileComponent = GetComponentInParent<DataBlockComponent>();
        if (TileComponent?.ModelLoaded ?? false) return;
        Data = TileComponent?.DataBlock;
        if (Data == null)
            return;
        bool overrideTextureSetting = false;
        if (Data.GetParameterByName("Primitive", out var param))
        {
            switch (param.Value)
            {
                case "Ellipse":
                    if (!gameObject.TryGetComponent<MeshRenderer>(out _))
                        _ = gameObject.AddComponent<MeshRenderer>();
                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();
                    if (filter == null)
                        filter = gameObject.AddComponent<MeshFilter>();
                    filter.mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh;
                    break;
            }
        }
        else
        {
            models = Data.GetReferences(AssetType.Model);
            var fileName = (models.Count() > 0) ? models.ElementAt(0).FileName : "";
            if (Data.GetParameterByName("Templater", out var name))
            {
                TemplatedItems.Remove(name.Value);
                TemplatedItems.Add(name.Value, (GetMesh(fileName), Data.GetFirstTextureAsset().FileName));
                return;
            }
            else if (Data.GetParameterByName("ApplyTemplater", out name))
            {
                if (TemplatedItems.TryGetValue(name.Value, out var tuple))
                {
                    ImportMesh(tuple.mesh);
                    var renderer = GetComponent<Renderer>();
                    renderer.material = Instantiate((Material)Resources.Load("Materials/Object Material"));
                    renderer.material.mainTexture = TextureLoader.RequestTexture(Path.Combine(AssetDirectory, tuple.textureFileName));
                    overrideTextureSetting = true; // texture set already dont set another one
                }
                else return;
            }
            else
            {
                if (!GameInitialization.Initialized)
                    GameInitialization.Initialize();
                var animTarget = AddGLBObject(fileName); // imports the mesh without templating

                //find the animation entry for this GLB model, if there is one
                var entry = AnimationDatabase.GetEntryByGLBPath(fileName);
                if (entry != null)
                {
                    //compile animations if necessary -- this adds the necessary animator used by the AnimationLoader
                    AnimationCompiler.GlobalAnimationCompiler.CompileAnimations(TextureLoader.AssetDirectory, entry.B3DFilePath, animTarget, out var animators);
                    //activate animation capability
                    var loader = gameObject.AddComponent<AnimationLoader>();
                    //allow the AnimationLoader to use the animator
                    loader.SetAnimator(animators.LastOrDefault());
                }
            }
        }        
        if (Data.HasTexture && !overrideTextureSetting)
            gameObject.AddComponent<TextureLoader>().InheritParent = true; // adds the texture loader to apply a texture to the model
        var localScale = transform.localScale;
        float scaleX = localScale.x, scaleY = localScale.y, scaleZ = localScale.z;
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
        TileComponent.ModelLoaded = true;
        Loaded = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!Loaded)
            Awake(); // in case you know?
        DestroyImmediate(this);
    }

    GameObject AddGLBObject(string fileName)
    {
        var glb = Siccity.GLTFUtility.Importer.LoadFromFile(Path.Combine(AssetDirectory, fileName));        
        glb.transform.parent = gameObject.transform;
        glb.transform.position = new Vector3();
        glb.transform.localScale = new Vector3(1,1,1);
        return glb;
    }

    void ImportMesh(string fileName)
    {
        if (!gameObject.TryGetComponent<MeshRenderer>(out _))
            _ = gameObject.AddComponent<MeshRenderer>();
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = GetMesh(fileName);
    }

    void ImportMesh(Mesh mesh)
    {
        if (!gameObject.TryGetComponent<MeshRenderer>(out _))
            _ = gameObject.AddComponent<MeshRenderer>();
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    Mesh GetMesh(string fileName)
    {
        if (LoadedContent.TryGetValue(fileName, out var mesh))
            return mesh;
        Mesh holderMesh = null;
        var gameObject = Siccity.GLTFUtility.Importer.LoadFromFile(Path.Combine(AssetDirectory, fileName));
        holderMesh = gameObject.GetComponentInChildren<MeshFilter>().mesh;
        if (holderMesh == null) throw new System.Exception("Model not loaded");
        Destroy(gameObject);
        LoadedContent.Add(fileName, holderMesh);
        return holderMesh;
    }

    Mesh GetMeshOBJ(string fileName)
    {
        if (LoadedContent.TryGetValue(fileName, out var mesh))
            return mesh;
        Mesh holderMesh = new Mesh();
        holderMesh = new Assets.ObjImporter().ImportFile(Path.Combine(AssetDirectory, fileName));
        LoadedContent.Add(fileName, holderMesh);
        return holderMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
