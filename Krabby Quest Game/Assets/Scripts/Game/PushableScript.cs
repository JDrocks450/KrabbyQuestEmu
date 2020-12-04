using Assets.Components.World;
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
    /// <summary>
    /// Used when boxes slide into eachother on ICE. This is set by objects having the BOX_ForcePush parameter set to true.
    /// </summary>
    public bool ForcePush { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        MovementScript = gameObject.AddComponent<TileMovingObjectScript>();
        MovementScript.Target = gameObject;
        MovementScript.JumpToTile(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        MovementScript.TilePositionChanged += MovementScript_TilePositionChanged;
        TileMovingObjectScript.MoveableMoving += Jetstream_SpongebobPlayerPositionChanging;
        if (BlockComponent.DataBlock.GetParameterByName("CanFloat", out var parameter))
            CanFloat = bool.Parse(parameter.Value);
        if (BlockComponent.DataBlock.GetParameterByName("ExclusiveMode", out parameter))
            ExclusivePushMode = (PlayerEnum)int.Parse(parameter.Value);
        if (BlockComponent.DataBlock.GetParameterByName("CanDestory", out parameter))
            CanDestory = bool.Parse(parameter.Value);
        if (!CanDestory)
            MovementScript.CanMoveOverWorldReservedTiles = false;
    }

    private void MovementScript_TilePositionChanged(object sender, MoveEventArgs e)
    {
        if (World.Current.TryGetBlockAt(StinkyFile.BlockLayers.Integral, e.ToTile.x, e.ToTile.y, out var block))
        {
            if (block.GetParameterByName<bool>("BOX_ForcePush", out var param))
                ForcePush = param.Value; // check for BOX_ForcePush flag and set as necessary. Whenever the box fully moves to a new tile, this is updated to that tile's value.
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Jetstream_SpongebobPlayerPositionChanging;               
    }

    public void Destroy()
    {
        if (TryGetComponent<BeachBallBehavior>(out var bhav))
        {
            bhav.Destroy();
            return;
        }
        GetComponent<SoundLoader>().Play(2);
        if (gameObject.transform.childCount > 0)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false); // make visual component disappear
            OnDestroy(); // unsubscribe from all events to be safe
            GetComponentInChildren<ParticleLoader>()?.Play();
        }
        else GameObject.Destroy(gameObject);
    }

    private void Jetstream_SpongebobPlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.BlockMotion) return;
        if (!MovementAllowed) return;
        if (e.ToTile.x == MovementScript.TileX && e.ToTile.y == MovementScript.TileY)
        {
            if ((sender as TileMovingObjectScript).TryGetComponent<GooberBehavior>(out var goober)) // is a goober
            {
                if (CanDestory)
                    Destroy(); // destroy this destructable box
                Destroy(goober.gameObject); //the goober must also be destroyed
                return; // prevent goober pushing box
            }
            if ((sender as TileMovingObjectScript).TryGetComponent<Player>(out _)) //ignore pushes that aren't players
                PlayerEnteredTile(e);
            else if ((sender as TileMovingObjectScript).TryGetComponent<PushableScript>(out var pushable) && pushable.ForcePush)
                DoPush(e);
            else e.BlockMotion = true;
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
        DoPush(e);           
    }

    void DoPush(MoveEventArgs e)
    {
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
        MovementScript.MotionSpeed = e.MotionSpeed + .1f;
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
