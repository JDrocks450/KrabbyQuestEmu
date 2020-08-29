using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetstream : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    Player Spongebob;
    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        Spongebob = GameObject.Find("Spongebob").GetComponent<Player>();
        Spongebob.PlayerPositionChanged += Jetstream_SpongebobPlayerPositionChanged;
    }

    private void Jetstream_SpongebobPlayerPositionChanged(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
            PlayerEnteredTile();        
    }

    void PlayerEnteredTile()
    {
        switch (BlockComponent.DataBlock.Rotation)
        {
            case StinkyFile.SRotation.NORTH:
                Spongebob.WalkToTile(Spongebob.TileX, Spongebob.TileY - 1);
                break;
            case StinkyFile.SRotation.SOUTH:
                Spongebob.WalkToTile(Spongebob.TileX, Spongebob.TileY + 1);
                break;
            case StinkyFile.SRotation.WEST:
                Spongebob.WalkToTile(Spongebob.TileX - 1, Spongebob.TileY);
                break;
            case StinkyFile.SRotation.EAST:
                Spongebob.WalkToTile(Spongebob.TileX + 1, Spongebob.TileY);
                break;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
