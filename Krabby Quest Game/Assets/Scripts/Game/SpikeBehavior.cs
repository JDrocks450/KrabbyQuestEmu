using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpikeBehavior : MonoBehaviour
{
    const float SPIKE_TIME_UP = 1f;

    float[] SpikeTimes =
    {
        2f,
        2f,
        2f,
        7f
    };
    int sproutType = 0;
    float currentTime;
    public bool Up
    {
        get; private set;
    } = true;
    Animator animator;
    AnimationLoader animations;
    private DataBlockComponent block;
    /// <summary>
    /// the object standing on the current tile
    /// </summary>
    object standingInside;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();        
        animations = GetComponentInChildren<AnimationLoader>();    
        animations.PlayAnimationSequence("Spike");
        block = GetComponentInParent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += TileMovingObjectScript_MoveableMoving;
        TileMovingObjectScript.MoveableMoved += TileMovingObjectScript_MoveableMoved;
        var number = block.DataBlock.Name.Last().ToString();
        sproutType = int.Parse(number);
    }

    private void TileMovingObjectScript_MoveableMoved(object sender, MoveEventArgs e)
    {
        if (e.ToTile == block.WorldTilePosition)
            standingInside = sender;             
    }

    private void TileMovingObjectScript_MoveableMoving(object sender, MoveEventArgs e)
    {
        if (e.ToTile == block.WorldTilePosition)
            e.BlockMotion = Up;   
        else if (standingInside != null && standingInside.Equals(sender))
            standingInside = null;   
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= TileMovingObjectScript_MoveableMoving;
        TileMovingObjectScript.MoveableMoved -= TileMovingObjectScript_MoveableMoved;
    }

    // Update is called once per frame
    void Update()
    {
        if (!LevelObjectManager.IsDone) return;
        if (Up)
        {
            if (standingInside != null)
                KillStandingObject();
            if (currentTime > SPIKE_TIME_UP)
            {
                animator.Play("SpikeDown");
                Up = false;
                currentTime = 0;
            }
            else currentTime += Time.deltaTime;
        }
        else
        {
            if (currentTime > SpikeTimes[sproutType - 1]) 
            {
                animator.Play("SpikeUp");
                //GetComponent<SoundLoader>().Play(0);
                Up = true;
                currentTime = 0;
            }
            else currentTime += Time.deltaTime;
        }
    }

    private void KillStandingObject()
    {
        var standingInside = (this.standingInside as TileMovingObjectScript).gameObject;
        if (standingInside.TryGetComponent<Player>(out var player)) // is player
        {
            Player.KillAllPlayers();
            this.standingInside = null;
        }
        else if (standingInside.TryGetComponent<PushableScript>(out var box))
        {
            if (box.CanDestory)
                box.Destroy();
        }
    }
}
