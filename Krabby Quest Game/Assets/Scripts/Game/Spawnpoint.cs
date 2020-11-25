using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    static bool[] LoadedPlayers = new bool[2] { false, false };
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
        var playerIndex = player == "Spongebob" ? 0 : 1;
        if (LoadedPlayers[playerIndex])
        {
            Debug.LogWarning($"Multiple {player}s have been spawned in the map. This may have not been intended.");
        }
        var Spongebob = GameObject.Find(player).GetComponent<Player>();
        if (Spongebob == null)
        {
            Debug.LogWarning($"Player: {player} was not found... creating player now");
            Spongebob = ((GameObject)Instantiate(Resources.Load("Objects/Spongebob"))).GetComponent<Player>();
        }
        Spongebob.PlayerName = player.ToLower();
        var data = Spongebob.gameObject.AddComponent<DataBlockComponent>();
        data.DataBlock = BlockComponent.DataBlock;
        Spongebob.transform.GetChild(1).gameObject.AddComponent<ModelLoader>();
        LoadedPlayers[playerIndex] = true;
        return Spongebob;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.CurrentPlayer == Assets.Scripts.Game.PlayerEnum.ANYONE) return;
        if (LevelObjectManager.IsDone && !LoadedPlayers[(int)Player.CurrentPlayer]) // switch to player that isn't null -- happens when patrick is the only playable character for example
        {
            switch (Player.CurrentPlayer)
            {
                case Assets.Scripts.Game.PlayerEnum.SPONGEBOB:
                    if (LoadedPlayers[1])
                        Player.CurrentPlayer = Assets.Scripts.Game.PlayerEnum.PATRICK;
                    break;
                case Assets.Scripts.Game.PlayerEnum.PATRICK:
                    if (LoadedPlayers[0])
                        Player.CurrentPlayer = Assets.Scripts.Game.PlayerEnum.SPONGEBOB;
                    break;
            }
        }
    }
}
