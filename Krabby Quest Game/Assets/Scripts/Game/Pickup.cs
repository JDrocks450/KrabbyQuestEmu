using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public static Dictionary<string, (int amountLeft, int amountTotal)> MajorPickups = new Dictionary<string, (int, int)>();
    const float CollectTime = .15f;

    string pickupName;
    DataBlockComponent BlockComponent;
    bool Collected = false, isPickingUp = false;
    float pickingUpTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += TileMovingObjectScript_MoveableMoving;
        if (BlockComponent.DataBlock.GetParameterByName("Major Item", out var param))
        {
            if (MajorPickups.TryGetValue(param.Value, out var info))
                MajorPickups[param.Value] = (info.amountLeft, info.amountTotal + 1);
            else MajorPickups.Add(param.Value, (0, 1));
            pickupName = param.Value;
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= TileMovingObjectScript_MoveableMoving;
        if (MajorPickups.TryGetValue(pickupName, out var info))
            MajorPickups[pickupName] = (info.amountLeft + 1, info.amountTotal);
        Collected = true;
    }

    private void TileMovingObjectScript_MoveableMoving(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            if (!(sender as TileMovingObjectScript).TryGetComponent<Player>(out _)) // is not a player
            {
                e.BlockMotion = true;
                e.BlockMotionSender = gameObject.name;
            }
            else
            {
                e.BlockMotion = false;
                isPickingUp = true;
            }
        }
    }

    private void PlayerEnteredTile()
    {
        if (Collected) return;
        var audioLoader = GetComponent<SoundLoader>();
        audioLoader.Play(0);
        Destroy(gameObject);        
        Collected = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickingUp)
        {
            pickingUpTime += Time.deltaTime;
            if (pickingUpTime >= CollectTime)
                PlayerEnteredTile();
        }
    }
}
