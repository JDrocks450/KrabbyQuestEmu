﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageBehavior : MonoBehaviour
{
    private DataBlockComponent blockComponent;
    int messageIndex;
    private bool messageShown;

    // Start is called before the first frame update
    void Start()
    {
        blockComponent = GetComponent<DataBlockComponent>();
        messageIndex = int.Parse(new string(blockComponent.DataBlock.Name.Where(x => char.IsDigit(x)).ToArray())) - 1;
        TileMovingObjectScript.MoveableMoving += PlayerMoved;
    }

    private void PlayerMoved(object sender, MoveEventArgs e)
    {
        bool isPlayer = (sender as TileMovingObjectScript).TryGetComponent<Player>(out _);
        if (!isPlayer)
            return;
        if (e.ToTile.x == blockComponent.WorldTileX && e.ToTile.y == blockComponent.WorldTileY)
        {
            MessagePromptBehavior.ShowMessage(LevelObjectManager.Level.Messages[messageIndex]);
            messageShown = true;
        }
        else if (messageShown)
        {
            messageShown = false;
            MessagePromptBehavior.HideMessage();
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= PlayerMoved;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
