using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBlockComponent : MonoBehaviour
{
    public LevelDataBlock DataBlock
    {
        get; set;
    }
    public int WorldTileX { get; set; }
    public int WorldTileY { get; set; }
    public bool ModelLoaded { get; set; }
    public bool TextureLoaded { get; set; }
    public Vector2Int WorldTilePosition => new Vector2Int(WorldTileX, WorldTileY);
    public GameObject Parent { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
