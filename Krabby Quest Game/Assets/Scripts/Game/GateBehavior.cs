using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GateBehavior;

public class GateBehavior : MonoBehaviour
{
    static HashSet<string> OpenGateColors = new HashSet<string>();
    public DataBlockComponent BlockComponent { get; private set; }
    private Player Spongebob;
    string Color;
    public bool IsPlayerInside { get; private set; }
    public bool IsOpen
    {
        get; private set;
    } = false;
    public enum GateMsg
    {
        Open,
        Close
    }

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += Spongebob_PlayerPositionChanging;
        BlockComponent.DataBlock.GetParameterByName("Color", out var Data);
        Color = Data.Value;
        if (IsOpen)
            OpenGateColors.Add(Color);
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= Spongebob_PlayerPositionChanging;        
    }

    private void Spongebob_PlayerPositionChanging(object sender, MoveEventArgs e)
    {
        bool isPlayer = (sender as TileMovingObjectScript).GetComponent<Player>();
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            if (!IsOpen)
            {
                e.BlockMotion = true;
                e.BlockMotionSender = gameObject.name;
            }
            else if (isPlayer)
                IsPlayerInside = true;
        }
        else if (isPlayer)        
            IsPlayerInside = false;        
    }  

    public static void SendMessage(GateMsg command, string color)
    {
        switch (command)
        {
            case GateMsg.Open:
                OpenGateColors.Add(color);
                break;
            case GateMsg.Close:
                OpenGateColors.Remove(color);
                break;
        }
    }

    public static void ClearGateFlags()
    {
        OpenGateColors.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        bool lastOpenState = IsOpen;
        IsOpen = OpenGateColors.Contains(Color);
        if (lastOpenState != IsOpen)
        {
            var animator = GetComponentInChildren<Animator>();
            if (IsOpen) // closed -> open
                animator.Play("Opened");
            else
            {
                animator.Play("Closed");
                if (IsPlayerInside)
                    Player.KillAllPlayers();
            }
        }
    }
}
