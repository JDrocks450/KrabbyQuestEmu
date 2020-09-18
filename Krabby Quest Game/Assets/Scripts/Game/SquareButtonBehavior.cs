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

    object pressingButton;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += Spongebob_PlayerPositionChanging;
        if (gameObject.name.Contains("CIRCLE"))
            CanUnpush = true;
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Spongebob_PlayerPositionChanging;
    }

    private void Spongebob_PlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            Pushed = true;
            var animator = GetComponentInChildren<Animator>();
            animator.Play("Pushed"); // play press anim
            GetComponentInChildren<Light>().enabled = false; // turn off the light
            BlockComponent.DataBlock.GetParameterByName("Button Color", out var param);
            OnPress?.Invoke(this, param.Value);
            GateBehavior.SendMessage(GateBehavior.GateMsg.Close, param.Value);
            pressingButton = sender;
        }
        else if (Pushed && CanUnpush && pressingButton.Equals(sender))
        {
            Pushed = false;
            var animator = GetComponentInChildren<Animator>();
            animator.Play("Unpushed"); // play unpress anim
            GetComponentInChildren<Light>().enabled = true; // turn on the light
            BlockComponent.DataBlock.GetParameterByName("Button Color", out var param);
            OnUnpress?.Invoke(this, param.Value);
            GateBehavior.SendMessage(GateBehavior.GateMsg.Open, param.Value);
            pressingButton = sender;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
