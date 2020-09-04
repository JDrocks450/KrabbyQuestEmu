using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableScript : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    Player Spongebob;
    TileMovingObjectScript MovementScript;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        MovementScript = BlockComponent.Parent.AddComponent<TileMovingObjectScript>();
        MovementScript.TileX = BlockComponent.WorldTileX;
        MovementScript.TileY = BlockComponent.WorldTileY;
        BlockComponent.gameObject.AddComponent<TextureLoader>();
        Spongebob = GameObject.Find("Spongebob").GetComponent<Player>();
        Spongebob.PlayerPositionChanging += Jetstream_SpongebobPlayerPositionChanging;
    }

    private void Jetstream_SpongebobPlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == MovementScript.TileX && e.ToTile.y == MovementScript.TileY)
            PlayerEnteredTile(e);        
    }

    private void PlayerEnteredTile(MoveEventArgs e)
    {
        switch (e.Direction)
        {
            case StinkyFile.SRotation.NORTH:
                MovementScript.WalkToTile(MovementScript.TileX, MovementScript.TileY - 1);
                break;
            case StinkyFile.SRotation.SOUTH:
                MovementScript.WalkToTile(MovementScript.TileX, MovementScript.TileY + 1);
                break;
            case StinkyFile.SRotation.EAST:
                MovementScript.WalkToTile(MovementScript.TileX + 1, MovementScript.TileY);
                break;
            case StinkyFile.SRotation.WEST:
                MovementScript.WalkToTile(MovementScript.TileX - 1, MovementScript.TileY);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
