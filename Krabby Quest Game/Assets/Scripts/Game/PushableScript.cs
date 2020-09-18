using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PushableScript : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    Player Spongebob;
    TileMovingObjectScript MovementScript;

    public event EventHandler<MoveEventArgs> OnPushing;

    /// <summary>
    /// Floats in water
    /// </summary>
    public bool CanFloat
    {
        get; set;
    } = true;
    /// <summary>
    /// 0 = Spongebob Only, 1 = Patrick Only, 2 = Anyone
    /// </summary>
    public PlayerEnum ExclusivePushMode
    {
        get; set;
    } = PlayerEnum.ANYONE;
    public bool MovementAllowed
    {
        get; set;
    } = true;
    public bool CanDestory
    {
        get; set;
    } = false;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        MovementScript = BlockComponent.Parent.AddComponent<TileMovingObjectScript>();
        MovementScript.TileX = BlockComponent.WorldTileX;
        MovementScript.TileY = BlockComponent.WorldTileY;
        MovementScript.Target = gameObject;
        TileMovingObjectScript.MoveableMoving += Jetstream_SpongebobPlayerPositionChanging;
        if (BlockComponent.DataBlock.GetParameterByName("CanFloat", out var parameter))
            CanFloat = bool.Parse(parameter.Value);
        if (BlockComponent.DataBlock.GetParameterByName("ExclusiveMode", out parameter))
            ExclusivePushMode = (PlayerEnum)int.Parse(parameter.Value);
        if (BlockComponent.DataBlock.GetParameterByName("CanDestory", out parameter))
            CanDestory = bool.Parse(parameter.Value);
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Jetstream_SpongebobPlayerPositionChanging;
    }

    private void Jetstream_SpongebobPlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == MovementScript.TileX && e.ToTile.y == MovementScript.TileY)
        {
            if ((sender as TileMovingObjectScript).TryGetComponent<GooberBehavior>(out var goober)) // is a goober
            {
                if (CanDestory)
                    Destroy(gameObject);
                return; // prevent goober pushing box
            }
            PlayerEnteredTile(e);
        }
    }

    private void PlayerEnteredTile(MoveEventArgs e)
    {        
        if (!MovementAllowed || MovementScript.IsMoving)
        {
            return;
        }
        if (ExclusivePushMode != PlayerEnum.ANYONE)
        {
            var player = Player.CurrentPlayer == ExclusivePushMode;
            if (!player)
            {
                e.BlockMotion = true;
                e.BlockMotionSender = gameObject.name;
                return;
            }
        }
        var args = new MoveEventArgs()
        {
            FromTile = new Vector2Int(MovementScript.TileX, MovementScript.TileY),
            ToTile = MovementScript.GetTileFromDirection(e.Direction)
        };
        OnPushing?.Invoke(this, args);
        if (args.BlockMotion)
        {
            Debug.LogWarning("Movement Canceled for: " + gameObject.name + " by: " + args.BlockMotionSender);
            return;
        }
        MovementScript.MotionSpeed = e.MotionSpeed;
        bool blockMotion = false;
        blockMotion = !MovementScript.MoveInDirection(e.Direction);
        e.BlockMotion = blockMotion;
        if (!blockMotion)        
            GetComponent<SoundLoader>().Play(1);                    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
