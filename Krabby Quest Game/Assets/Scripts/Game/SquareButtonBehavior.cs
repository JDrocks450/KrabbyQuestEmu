using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareButtonBehavior : MonoBehaviour
{
    public DataBlockComponent BlockComponent { get; private set; }
    private Player Spongebob;
    public event EventHandler<string> OnPress;
    public event EventHandler<string> OnUnpress;
    public bool Pushed
    {
        get; private set;
    } = false;
    public bool CanUnpush
    {
        get; private set;
    } = false;
    public string Color;
    object pressingButton;
    object lastMotionPressSender;
    bool lastMotionPushState;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoved += Spongebob_PlayerPositionChanging;
        TileMovingObjectScript.MoveableMoving += Spongebob_PlayerPositionChanging;
        TileMovingObjectScript.OnObjectMotionCanceled += OnMotionBlocked;
        if (gameObject.name.Contains("CIRCLE"))
            CanUnpush = true;
        if (BlockComponent.DataBlock.GetParameterByName("Button Color", out var param))
            Color = param.Value;
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Spongebob_PlayerPositionChanging;
        GateBehavior.SendMessage(GateBehavior.GateMsg.Close, Color);
    }

    private void Press(object sender, MoveEventArgs e)
    {
        Pushed = true;        
        OnPress?.Invoke(this, Color);
        GateBehavior.SendMessage(GateBehavior.GateMsg.Open, Color);
        pressingButton = sender;         
    }

    private void Unpress(object sender, MoveEventArgs e)
    {
        Pushed = false;
               
        OnUnpress?.Invoke(this, Color);
        GateBehavior.SendMessage(GateBehavior.GateMsg.Close, Color);
        pressingButton = null;        
    }

    private void Spongebob_PlayerPositionChanging(object sender, MoveEventArgs e)
    {
        TileMovingObjectScript otherObject = null;
        if (sender is TileMovingObjectScript)
        {
            if ((sender as TileMovingObjectScript).SpecialObjectIgnore)
                return;
            otherObject = sender as TileMovingObjectScript;
        }
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            if (!Pushed)
                Press(sender, e);
            e.SetRaisedTerrainFlag(.2f);
        }
        else if (Pushed && CanUnpush && pressingButton.Equals(sender))
            Unpress(sender, e);
    }

    void OnMotionBlocked(object sender, MoveEventArgs e)
    {
        if (e.FromTile == BlockComponent.WorldTilePosition)
        {
            if (sender is TileMovingObjectScript)
                if ((sender as TileMovingObjectScript).SpecialObjectIgnore)
                    return;
            Pushed = true; // ignore the last press as the motion was blocked
            pressingButton = lastMotionPressSender;
            Press(sender, e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Pushed != lastMotionPushState)
        {
            if (!Pushed)
            {
                var animator = GetComponentInChildren<Animator>();
                animator.Play("Unpushed"); // play unpress anim
                GetComponentInChildren<Light>().enabled = true; // turn on the light     
            }
            else
            {
                var animator = GetComponentInChildren<Animator>();
                animator.Play("Pushed"); // play press anim
                GetComponentInChildren<Light>().enabled = false; // turn off the light
            }
            var sound = GetComponent<SoundLoader>();
            sound.ExclusiveSoundMode = true;
            sound.Play(0);
        } 
        lastMotionPushState = Pushed;
    }
}
