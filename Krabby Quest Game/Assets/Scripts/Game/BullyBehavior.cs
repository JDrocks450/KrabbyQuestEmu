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
        bool Mode = Modes[(int)Rotation];
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
        Modes[(int)Rotation] = !Mode;
    }    

    void MoveForward()
    {
        switch (Rotation)
        {
            case SRotation.NORTH:
                TileScript.WalkToTile(TileScript.TileX, TileScript.TileY - 1);
                Rotator.Rotate(SRotation.NORTH);
                break;
            case SRotation.SOUTH:
                TileScript.WalkToTile(TileScript.TileX, TileScript.TileY + 1);
                Rotator.Rotate(SRotation.SOUTH);
                break;
            case SRotation.WEST:
                TileScript.WalkToTile(TileScript.TileX - 1, TileScript.TileY);
                Rotator.Rotate(SRotation.WEST);
                break;
            case SRotation.EAST:
                TileScript.WalkToTile(TileScript.TileX + 1, TileScript.TileY);
                Rotator.Rotate(SRotation.EAST);
                break;
        }       
    }

    // Update is called once per frame
    void Update()
    {
        if (!TileScript.IsMoving)
            MoveForward();
    }
}
