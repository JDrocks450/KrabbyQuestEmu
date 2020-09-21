using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllowTileMovement : MonoBehaviour
{
    public DataBlockComponent BlockComponent { get; private set; }
    public static string[] AllowedPrefabs =
    {
        "Square Button" ,
        "Circle Button",
        "Gate",
        "Jetstream",
        "Bully",
        "Pickup"
    };

    public bool AllowMovement = false;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += Jetstream_SpongebobPlayerPositionChanged;
        if (BlockComponent.DataBlock.GetParameterByName("FLOOR", out _))            
            AllowMovement = true;
        if (BlockComponent.DataBlock.GetParameterByName("AllowMotion", out var movementParam))            
            AllowMovement = bool.Parse(movementParam.Value);
        if (BlockComponent.DataBlock.GetParameterByName("ServiceObject", out _))            
            AllowMovement = true;
        if (BlockComponent.DataBlock.GetParameterByName("Prefab", out var param))
        {
            if (AllowedPrefabs.Contains(param.Value))           
                AllowMovement = true;
        }
    }

    private void Jetstream_SpongebobPlayerPositionChanged(object sender, MoveEventArgs e)
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            if (e.BlockMotion) return;
            e.BlockMotion = !AllowMovement;
            e.BlockMotionSender = gameObject.name;
        }
    }

    void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Jetstream_SpongebobPlayerPositionChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}