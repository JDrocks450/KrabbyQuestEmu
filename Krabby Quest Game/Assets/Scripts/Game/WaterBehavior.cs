using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterBehavior : MonoBehaviour
{
    private DataBlockComponent BlockComponent;

    public bool IsBoxSunken
    {
        get; private set;
    } = false;
    public bool IsBoxFloating
    {
        get; private set;
    } = false;
    bool wallsDisabled = false;
    private Vector2Int Position;
    private string GUID;

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out BlockComponent))
        {
            var component = GetComponentInParent<DataBlockComponent>();
            Position = new Vector2Int(component.WorldTileX, component.WorldTileY);
        }
        else
        {
            Position = new Vector2Int(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
            GUID = BlockComponent.DataBlock.GUID;
        }
        var allowMovement = GetComponent<AllowTileMovement>();
        if (allowMovement != null)
            allowMovement.AllowMovement = true;
        TileMovingObjectScript.MoveableMoved += Jetstream_SpongebobPlayerPositionChanged;
        /*TileMovingObjectScript.MoveableMoving += (object s, MoveEventArgs e) =>
        {
            e.BlockMotion = false;
        };*/
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoved -= Jetstream_SpongebobPlayerPositionChanged;
    }

    void DisableWalls()
    {
        if (GUID == default)
            GUID = LevelObjectManager.Level.IntegralData.FirstOrDefault(z =>
            {
                switch (LevelObjectManager.Context)
                {
                    case LevelContext.BIKINI:
                        return z.Name == "BIKINI_WATER";
                    case LevelContext.BEACH:
                        return z.Name == "WATER_GOO_LAGOON";
                }
                return false;
            })?.GUID;
        if (GUID == null) return;
        var level = LevelObjectManager.Level;
        LevelDataBlock getByCoord(int X, int Y) => level.IntegralData[(Y * level.Columns) + X];
        int x = Position.x, y = Position.y;
        try
        {
            var north = getByCoord(x, y - 1);
            if (north.GUID == GUID
                || north.GUID == "O542113") // water or raft north
                transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }
        catch { };
        try
        {
            var east = getByCoord(x + 1, y);
            if (east.GUID == GUID
                 || east.GUID == "O542113") // water or raft east
                transform.GetChild(1).GetComponent<Renderer>().enabled = false;
        }
        catch { };
        try
        {
            var south = getByCoord(x, y + 1);
            if (south.GUID == GUID
                 || south.GUID == "O542113") // water or raft south
                transform.GetChild(3).GetComponent<Renderer>().enabled = false;
        }
        catch { };
        try
        {
            var west = getByCoord(x - 1, y);
            if (west.GUID == GUID
                 || west.GUID == "O542113") // water or raft west
                transform.GetChild(2).GetComponent<Renderer>().enabled = false;
        }
        catch { };
    }

    private void Jetstream_SpongebobPlayerPositionChanged(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == Position.x && e.ToTile.y == Position.y)
            MoveableEnteredTile(sender as TileMovingObjectScript);        
    }

    void MoveableEnteredTile(TileMovingObjectScript Moveable)
    {
        if (Moveable.Target.TryGetComponent<PushableScript>(out var box)) // is a box?
        {            
            var animator = box.GetComponentInChildren<Animator>();
            if ((box.CanFloat && !IsBoxFloating) || (!box.CanFloat && IsBoxSunken))
            {
                animator.Play("Floating");
                GetComponent<SoundLoader>().Play(0);
                IsBoxFloating = true;
                box.MovementAllowed = false;
            }
            else if (!box.CanFloat && !IsBoxSunken)
            {
                IsBoxSunken = true;
                box.MovementAllowed = false;
                GetComponent<SoundLoader>().Play(0);
                animator.Play("Sunken");                
            }            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelObjectManager.IsDone && !wallsDisabled)
        {
            DisableWalls();
            wallsDisabled = true;
        }
    }
}
