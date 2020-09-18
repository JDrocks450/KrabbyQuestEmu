using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetstream : MonoBehaviour
{
    DataBlockComponent BlockComponent;
    float animTimeSeconds;
    Material jetstreamMaterial;
    private float totalTime = .5f;
    GameObject Render;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        TileMovingObjectScript.MoveableMoved += Jetstream_SpongebobPlayerPositionChanged;
        var child = Render = transform.GetChild(0).gameObject;
        var component = child.AddComponent<DataBlockComponent>();
        component.DataBlock = BlockComponent.DataBlock;
        child.AddComponent<TextureLoader>();        
    }

    private void Jetstream_SpongebobPlayerPositionChanged(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == BlockComponent.WorldTileX && e.ToTile.y == BlockComponent.WorldTileY)
            PlayerEnteredTile(sender as TileMovingObjectScript);        
    }

    void PlayerEnteredTile(TileMovingObjectScript Spongebob)
    {
        var rotator = Spongebob.GetComponentInChildren<AngleRotator>();
        bool motionResult = false;
        switch (BlockComponent.DataBlock.Rotation)
        {
            case StinkyFile.SRotation.NORTH:
                motionResult = Spongebob.WalkToTile(Spongebob.TileX, Spongebob.TileY - 1);
                break;
            case StinkyFile.SRotation.SOUTH:
                motionResult = Spongebob.WalkToTile(Spongebob.TileX, Spongebob.TileY + 1);
                break;
            case StinkyFile.SRotation.WEST:
                motionResult = Spongebob.WalkToTile(Spongebob.TileX - 1, Spongebob.TileY);
                break;
            case StinkyFile.SRotation.EAST:
                motionResult = Spongebob.WalkToTile(Spongebob.TileX + 1, Spongebob.TileY);
                break;
        }
        GetComponent<SoundLoader>().Play(0);
        if (!motionResult && Spongebob.TryGetComponent<Player>(out _))
        {
            Player.KillAllPlayers();
            return;
        }
        if (rotator != null)
            rotator.Rotate(BlockComponent.DataBlock.Rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (jetstreamMaterial == null)
            jetstreamMaterial = Render.GetComponent<Renderer>().material;
        jetstreamMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(new Vector2(), new Vector2(0, -1), animTimeSeconds / totalTime));
        animTimeSeconds += Time.deltaTime;
        if (animTimeSeconds > totalTime)
            animTimeSeconds = 0;
    }
}
