using Assets.Components.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessageBehavior : MonoBehaviour
{
    private DataBlockComponent blockComponent;
    string MessageText;
    private bool messageShown;

    // Start is called before the first frame update
    void Start()
    {
        blockComponent = GetComponent<DataBlockComponent>();
        MessageText = blockComponent.DataBlock.GetMessageContent(World.Current.Level);
        TileMovingObjectScript.MoveableMoving += PlayerMoving;
        TileMovingObjectScript.MoveableMoved += PlayerMoved;
    }

    private void PlayerMoving(object sender, MoveEventArgs e)
    {
        bool isPlayer = (sender as TileMovingObjectScript).TryGetComponent<Player>(out _);
        if (!isPlayer)
            return;
        if (e.ToTile.x == blockComponent.WorldTileX && e.ToTile.y == blockComponent.WorldTileY)
        {
            SignOverlayBehavior.ShowSignMessage(this, MessageText);
            messageShown = true;
        }
        else if (messageShown)
        {
            SignOverlayBehavior.CloseSignMessage(this);
            messageShown = false;
        }
    }

    private void PlayerMoved(object sender, MoveEventArgs e)
    {
        bool isPlayer = (sender as TileMovingObjectScript).TryGetComponent<Player>(out _);
        if (!isPlayer)
            return;
        if (e.ToTile.x == blockComponent.WorldTileX && e.ToTile.y == blockComponent.WorldTileY)
        {
            SignOverlayBehavior.ShowSignMessage(this, MessageText);
            messageShown = true;
        }
        else if (messageShown)
        {
            SignOverlayBehavior.CloseSignMessage(this);
            messageShown = false;
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= PlayerMoving;
        TileMovingObjectScript.MoveableMoved -= PlayerMoved;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
