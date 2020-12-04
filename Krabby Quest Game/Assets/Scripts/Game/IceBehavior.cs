using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBehavior : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    float animTimeSeconds;
    Material jetstreamMaterial;
    private float totalTime = .5f;
    SoundLoader soundEffects;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoved += Jetstream_SpongebobPlayerPositionChanged;
        soundEffects = GetComponent<SoundLoader>();
        soundEffects.ExclusiveSoundMode = true;
    }

    private void Jetstream_SpongebobPlayerPositionChanged(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
            PlayerEnteredTile(sender as TileMovingObjectScript, e.Direction);
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoved -= Jetstream_SpongebobPlayerPositionChanged;
    }

    bool TryMove(TileMovingObjectScript TileObject, StinkyFile.SRotation Direction)
    {
        bool motionResult = TileObject.MoveInDirection(Direction, 1, 7);
        if (motionResult)
        {
            soundEffects.Play(0);
            var rotator = TileObject.GetComponentInChildren<AngleRotator>();
            if (rotator != null)
                rotator.Rotate(Direction);
        }
        return motionResult;
    }

    void PlayerEnteredTile(TileMovingObjectScript TileObject, StinkyFile.SRotation Direction)
    {
        if (TileObject.SpecialObjectIgnore) return;       
        if (!TryMove(TileObject, Direction)) // try moving forward
        {
            if (!TryMove(TileObject, TileMovingObjectScript.GetBehindDirection(Direction))) // try flipping around
                Player.KillAllPlayers();
        }        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
