using Assets.Components.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    World CurrentWorld => World.Current;
    const float CollectTime = .15f;

    GameObject pickupModel, particleObject;
    ParticleSystem particleSystem;
    string pickupName;
    DataBlockComponent BlockComponent;
    bool Collected = false, isPickingUp = false;
    float pickingUpTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoving += TileMovingObjectScript_MoveableMoving;
        pickupModel = transform.GetChild(0).gameObject;
        particleObject = transform.GetChild(1).gameObject;
        if (particleObject != default)
            particleSystem = particleObject.GetComponent<ParticleSystem>();
        if (BlockComponent.DataBlock.GetParameterByName("Major Item", out var param))
        {            
            pickupName = param.Value;
            CurrentWorld.AddPickup(pickupName);
            CurrentWorld.CollisionMapUpdate(gameObject, true, BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        }        
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= TileMovingObjectScript_MoveableMoving;
        //Collect();
    }

    private void Collect()
    {
        if (Collected) return;
        CurrentWorld.RemovePickup(pickupName);
        CurrentWorld.CollisionMapUpdate(gameObject, false, BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        Collected = true;
        pickupModel.SetActive(false);
        SpawnParticleEffect();
    }

    private void SpawnParticleEffect()
    {
        particleSystem.Play();
    }

    private void TileMovingObjectScript_MoveableMoving(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
        {
            if (!(sender as TileMovingObjectScript).TryGetComponent<Player>(out _)) // is not a player
            {
                e.BlockMotion = !Collected;
                e.BlockMotionSender = gameObject.name;
            }
            else
            {
                e.BlockMotion = false;
                isPickingUp = true;
            }
        }
    }

    private void PlayerEnteredTile()
    {
        if (Collected) return;
        var audioLoader = GetComponent<SoundLoader>();
        audioLoader.Play(0);
        Collect();       
        Collected = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickingUp)
        {
            pickingUpTime += Time.deltaTime;
            if (pickingUpTime >= CollectTime)
                PlayerEnteredTile();
        }
    }
}
