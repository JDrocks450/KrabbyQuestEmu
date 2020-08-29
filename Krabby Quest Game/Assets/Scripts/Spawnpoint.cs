using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        var BlockComponent = GetComponent<DataBlockComponent>();
        var Spongebob = GameObject.Find("Spongebob").GetComponent<Player>();
        Spongebob.JumpToTile(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
