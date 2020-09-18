using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBehavior : MonoBehaviour
{
    const float LONG_THROW_TIME = 5.0f;
    
    DataBlockComponent BlockComponent;
    Object GooberPrefab;    

    float timeSinceLastThrow = 0f, throwTime = LONG_THROW_TIME;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        GooberPrefab = Resources.Load("Objects/Goober");
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceLastThrow > throwTime)
        { // throw goober
            var position = new Vector2Int(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
            var goober = (GameObject)Instantiate(GooberPrefab);
            goober.GetComponent<GooberBehavior>().StartProjectile(position, BlockComponent.DataBlock.Rotation);
            timeSinceLastThrow = 0f;
        }
        else timeSinceLastThrow += Time.deltaTime;
    }
}
