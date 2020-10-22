using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullyBehavior : MonoBehaviour
{
    public SRotation Rotation
    {
        get;set;
    }
    SRotation previousRotation;
    bool[] Modes = {
        true, true, true, true }; // NSEW
    public DataBlockComponent BlockComponent { get; private set; }
    public TileMovingObjectScript TileScript { get; private set; }
    private AngleRotator Rotator;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        Rotation = BlockComponent.DataBlock.Rotation;
        transform.eulerAngles = new Vector3();
        TileScript = GetComponent<TileMovingObjectScript>();
        TileScript.MotionCanceled += TileScript_MotionCanceled;
        TileScript.JumpToTile(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        TileScript.MotionSpeed = 2f;
        Rotator = GetComponentInChildren<AngleRotator>();
    }

    private void TileScript_MotionCanceled(object sender, MoveEventArgs e)
    {
        FlipRotation();
    }    

    void FlipRotation()
    {
        bool Mode = Modes[(int)Rotation];
        previousRotation = Rotation;
        switch (Rotation)
        {
            case SRotation.NORTH:
                Rotation = Mode ? SRotation.EAST : SRotation.WEST;                
                break;
            case SRotation.EAST:
                Rotation = Mode ? SRotation.SOUTH : SRotation.NORTH;
                break;
            case SRotation.SOUTH:
                Rotation = Mode ? SRotation.WEST : SRotation.EAST;
                break;
            case SRotation.WEST:
                Rotation = Mode ? SRotation.NORTH : SRotation.SOUTH;
                break;
        }
    }

    bool MoveForward()
    {
        bool result = false;
        switch (Rotation)
        {
            case SRotation.NORTH:
                result = TileScript.WalkToTile(TileScript.TileX, TileScript.TileY - 1);
                Rotator.Rotate(SRotation.NORTH);
                break;
            case SRotation.SOUTH:
                result = TileScript.WalkToTile(TileScript.TileX, TileScript.TileY + 1);
                Rotator.Rotate(SRotation.SOUTH);
                break;
            case SRotation.WEST:
                result = TileScript.WalkToTile(TileScript.TileX - 1, TileScript.TileY);
                Rotator.Rotate(SRotation.WEST);
                break;
            case SRotation.EAST:
                result = TileScript.WalkToTile(TileScript.TileX + 1, TileScript.TileY);
                Rotator.Rotate(SRotation.EAST);
                break;
        }
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (!TileScript.IsMoving)
        {
            if (!MoveForward())
            {
                Rotation = previousRotation;
                Modes[(int)Rotation] = !Modes[(int)Rotation];
                FlipRotation();
                MoveForward();
                Modes[(int)previousRotation] = !Modes[(int)previousRotation];
            }
        }
    }
}
