using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
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

    private void PlayerEnteredTile()
    {
        GameObject.Destroy(BlockComponent.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
