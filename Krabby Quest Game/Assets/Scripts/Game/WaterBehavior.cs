using Assets.Components.World;
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
    public bool IsCovered;
    bool wallsDisabled = false, isplayerfloating = false;
    private Vector2Int Position;
    private string GUID;
    object floating;

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out BlockComponent))
        {
            var component = GetComponentInParent<DataBlockComponent>();
            Position = new Vector2Int(component.WorldTileX, component.WorldTileY);
            BlockComponent = component;
            SoundLoader loader = gameObject.AddComponent<SoundLoader>();
            loader.LoadAll(BlockComponent.DataBlock);
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
        TileMovingObjectScript.MoveableMoving += TileMovingObjectScript_MoveableMoving;
    }

    private void TileMovingObjectScript_MoveableMoving(object sender, MoveEventArgs e)
    {
        if (e.BlockMotion) return;
        if (IsCovered || IsBoxFloating) return;
        if (e.ToTile.x == Position.x && e.ToTile.y == Position.y)
        {
            if ((sender as TileMovingObjectScript).TryGetComponent<Player>(out _) || 
                (sender as TileMovingObjectScript).TryGetComponent<EnemyBehavior>(out _))
                e.BlockMotion = true;
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoved -= Jetstream_SpongebobPlayerPositionChanged;
        TileMovingObjectScript.MoveableMoving -= TileMovingObjectScript_MoveableMoving;

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
        else if (isplayerfloating && floating.Equals(sender)) // destroy raft
        {
            isplayerfloating = false;
            IsCovered = false;
            transform.parent.GetChild(1).gameObject.SetActive(false); // hide raft
            var soundLoader = GetComponent<SoundLoader>();
            soundLoader.Play(0); 
        }
    }

    bool MoveableEnteredTile(TileMovingObjectScript Moveable)
    {        
        if (Moveable.Target.TryGetComponent<PushableScript>(out var box)) // is a box?
        {
            var animator = box.GetComponentInChildren<Animator>();
            if ((box.CanFloat && !IsBoxFloating)  // if this box can float (wooden) and there isn't already one floating...
                || (!box.CanFloat && IsBoxSunken && !IsBoxFloating)) // OR the box cannot float (steel) and there is already a sunken box in the water and there isn't already a box floating...
            {
                animator.Play("Floating");
                IsBoxFloating = true;
                box.MovementAllowed = false;
                GetComponent<SoundLoader>().Play(0);                
            }
            else if (!box.CanFloat && !IsBoxSunken && !IsBoxFloating)
            {
                IsBoxSunken = true;
                box.MovementAllowed = false;                
                GetComponent<SoundLoader>().Play(0);
                animator.Play("Sunken");
            }            
        }
        else if (Moveable.Target.TryGetComponent<Player>(out var player) && IsCovered)
        {
            if (!isplayerfloating)
            {
                isplayerfloating = true;
                floating = Moveable;
            }
        }
        else return false;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelObjectManager.IsDone && !wallsDisabled)
        {
            DisableWalls();
            wallsDisabled = true;
        }     
        World.Current.ForceCollisionFree(BlockComponent.WorldTileX, BlockComponent.WorldTileY); // free this space always
    }
}
