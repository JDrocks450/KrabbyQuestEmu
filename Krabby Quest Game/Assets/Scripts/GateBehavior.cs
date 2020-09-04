using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateBehavior : MonoBehaviour
{
    public DataBlockComponent BlockComponent { get; private set; }
    private Player Spongebob;
    string Color;
    public bool IsOpen
    {
        get; private set;
    } = false;
    bool ButtonLinked = false;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        Spongebob = GameObject.Find("Spongebob").GetComponent<Player>();
        Spongebob.PlayerPositionChanging += Spongebob_PlayerPositionChanging;
        BlockComponent.DataBlock.GetParameterByName("Color", out var Data);
        Color = Data.Value;        
    }

    private void ButtonPressed(object sender, string e)
    {
        IsOpen = true;
        GetComponentInChildren<Animator>().Play("Opened");
    }

    private void Spongebob_PlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ButtonLinked)
        {
            SquareButtonBehavior button = null;
            button = (GameObject.Find("BUTTON_SQUARE_" + Color))?.GetComponent<SquareButtonBehavior>();
            if (button != null)
                button.OnPress += ButtonPressed;
            else
            {
                button = (GameObject.Find("CIRCLE_BUTTON_" + Color))?.GetComponent<SquareButtonBehavior>();
                if (button != null)
                    button.OnPress += ButtonPressed;
            }
            ButtonLinked = button != null;
        }
    }
}
