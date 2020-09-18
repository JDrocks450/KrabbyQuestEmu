using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        if (BlockComponent.DataBlock.GetParameterByName("Spawnpoint", out var param))
        {
            Player player = null;
            switch (param.Value.ToLower())
            {
                case "spongebob":
                    player = LoadPlayer("Spongebob");                    
                    break;
                case "patrick":
                    player = LoadPlayer("Patrick");
                    break;
            }
            player.JumpToTile(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        }        
    }

    Player LoadPlayer(string player)
    {
        var Spongebob = GameObject.Find(player).GetComponent<Player>();
        Spongebob.PlayerName = player.ToLower();
        var data = Spongebob.gameObject.AddComponent<DataBlockComponent>();
        data.DataBlock = BlockComponent.DataBlock;
        Spongebob.transform.GetChild(1).gameObject.AddComponent<ModelLoader>();
        return Spongebob;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
