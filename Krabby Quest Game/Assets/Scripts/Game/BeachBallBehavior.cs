using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBallBehavior : MonoBehaviour
{
    PushableScript PushableScript;
    TileMovingObjectScript MoveScript;
    SoundLoader soundEffects;

    float bounceTime = 0f, bounceTimeout = .7f;
    SRotation Direction;
    bool bouncing;

    // Start is called before the first frame update
    void Start()
    {
        PushableScript = GetComponent<PushableScript>();
        PushableScript.OnPushing += OnPushing;        
        MoveScript = GetComponent<TileMovingObjectScript>();
        soundEffects = GetComponent<SoundLoader>();
    }

    private void OnDestroy()
    {        
        PushableScript.OnPushing -= OnPushing;     
    }

    private void OnPushing(object sender, MoveEventArgs e)
    {
        if (Player.CurrentPlayer == Assets.Scripts.Game.PlayerEnum.SPONGEBOB)
        {
            MoveScript.MoveInDirection(e.Direction);
            Direction = e.Direction;
            bouncing = true;
        }
        else
        {
            soundEffects.Play(0);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PushableScript.MovementAllowed)
            bouncing = false;
        if (bouncing && !MoveScript.IsMoving)
        {
            if (!MoveScript.MoveInDirection(Direction))
                bouncing = false;            
        }
        if (bouncing)
        {
            if (bounceTime >= bounceTimeout)
            {
                bounceTime = 0;
                soundEffects.Play(1);
            }
            bounceTime += Time.deltaTime;
        }
    }
}
