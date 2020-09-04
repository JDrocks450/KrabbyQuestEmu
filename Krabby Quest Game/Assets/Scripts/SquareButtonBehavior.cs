using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareButtonBehavior : MonoBehaviour
{
    public DataBlockComponent BlockComponent { get; private set; }
    private Player Spongebob;
    public event EventHandler<string> OnPress;
    public bool Pushed
    {
        get; private set;
    } = false;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        Spongebob = GameObject.Find("Spongebob").GetComponent<Player>();
        Spongebob.PlayerPositionChanging += Spongebob_PlayerPositionChanging;
    }

    private void Spongebob_PlayerPositionChanging(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            var animator = GetComponentInChildren<Animator>();
            animator.Play(Pushed ? "Unpushed" : "Pushed"); // play press anim
            GetComponentInChildren<Light>().enabled = false; // turn off the light
            BlockComponent.DataBlock.GetParameterByName("Button Color", out var param);
            OnPress?.Invoke(this, param.Value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
