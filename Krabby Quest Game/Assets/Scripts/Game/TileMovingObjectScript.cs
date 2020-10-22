using Assets.Scripts.Game;
using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for movement between tiles
/// </summary>
public class TileMovingObjectScript : MonoBehaviour
{
    /// <summary>
    /// When Spongebob fully moves from one world tile onto another
    /// </summary>
    public event EventHandler<MoveEventArgs> TilePositionChanged;
    /// <summary>
    /// When Spongebob begins to move from one world tile to another
    /// </summary>
    public event EventHandler<MoveEventArgs> TilePositionChanging;
    public event EventHandler<MoveEventArgs> MotionCanceled;

    /// <summary>
    /// When any moving object fully moves from one world tile onto another
    /// </summary>
    public static event EventHandler<MoveEventArgs> MoveableMoved;
    /// <summary>
    /// When any moving object begins to move from one world tile to another
    /// </summary>
    public static event EventHandler<MoveEventArgs> MoveableMoving;
    /// <summary>
    /// When any moving object begins to move from one world tile to another
    /// </summary>
    public static event EventHandler<MoveEventArgs> OnObjectMotionCanceled;

    public GameObject Target
    {
        get; set;
    }

    /// <summary>
    /// The speed of the movement in units/sec
    /// </summary>
    public float MotionSpeed
    {
        get
        {
            if (temporaryMotionSpeed == default)
                return _motionspeed;
            else return temporaryMotionSpeed;
        }
        set
        {
            _motionspeed = value;
        }
    }
    /// <summary>
    /// things like conveyors give spongebob a temporary boost in speed
    /// </summary>
    float temporaryMotionSpeed = default;
    /// <summary>
    /// The current TilePosition of the object in the X-Direction
    /// </summary>
    public int TileX { get; set; }
    /// <summary>
    /// The current TilePosition of the object in the Y-Direction
    /// </summary>
    public int TileY { get; set; }
    public Vector2Int TilePosition => new Vector2Int(TileX, TileY);
    /// <summary>
    /// Is the object currently moving
    /// </summary>
    public bool IsMoving { get; private set; }
    /// <summary>
    /// This object can move through anything
    /// </summary>
    public bool NoClip { get; set; } = false;

    public PlayerEnum Player { get; set; } = PlayerEnum.ANYONE;

    public Vector2Int DestinationTile => new Vector2Int(walkTileX, walkTileY);

    bool isWalking = false;
    float walkingPercentage = 0f;
    Vector3 walkStartLocation, walkEndLocation;
    int walkTileX = 0, walkTileY = 0;
    private float _motionspeed = 5f;

    private Vector3 getDestination(int x, int y) => new Vector3(-x * LevelObjectManager.Grid_Size.x, transform.position.y, y * LevelObjectManager.Grid_Size.y);

    /// <summary>
    /// Moves directly to the given tile without animation regardless of <see cref="MotionSpeed"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void JumpToTile(int x, int y)
    {
        TileX = x;
        TileY = y;
        transform.position = getDestination(x, y);
        var args = new MoveEventArgs()
        {
            FromTile = new Vector2Int(0,0),
            ToTile = new Vector2Int(x, y)
        };
        TilePositionChanged?.Invoke(this, args);
        MoveableMoved?.Invoke(this, args);
    }

    /// <summary>
    /// Walks to the specified tile if the motion is allowed
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>True if motion is allowed</returns>
    public bool WalkToTile(int x, int y, float overrideMotionSpeed = default)
    {
        isWalking = true;
        walkingPercentage = 0f;
        walkStartLocation = transform.position;
        walkEndLocation = getDestination(x, y);
        walkTileX = x;
        walkTileY = y;
        IsMoving = true;
        temporaryMotionSpeed = overrideMotionSpeed;
        var args = new MoveEventArgs()
            {
                FromTile = new Vector2Int(TileX, TileY),
                ToTile = new Vector2Int(walkTileX, walkTileY),
                MotionSpeed = MotionSpeed,
                Player = Player
            };
        TilePositionChanging?.Invoke(this, args);
        if (!args.BlockMotion && !NoClip)
            MoveableMoving?.Invoke(this, args);
        if (args.BlockMotion && !NoClip)
        {
            isWalking = false;
            IsMoving = false;
            MotionCanceled?.Invoke(this, args);
            OnObjectMotionCanceled?.Invoke(this, args);
            Debug.LogWarning("Movement Canceled for: " + gameObject.name + " by: " + args.BlockMotionSender);            
            return false;
        }
        return true;
    }

    public Vector2Int GetTileFromDirection(SRotation Direction, int Tiles = 1)
    {
        switch (Direction)
        {
            case StinkyFile.SRotation.NORTH:
                return new Vector2Int(TileX, TileY - Tiles);

            case StinkyFile.SRotation.SOUTH:
                return new Vector2Int(TileX, TileY + Tiles);

            case StinkyFile.SRotation.EAST:
                return new Vector2Int(TileX + Tiles, TileY);

            case StinkyFile.SRotation.WEST:
                return new Vector2Int(TileX - Tiles, TileY);
        }
        return default;
    }

    public bool MoveInDirection(SRotation Direction, int Tiles = 1)
    {
        var destination = GetTileFromDirection(Direction, Tiles);
        return WalkToTile(destination.x, destination.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Target == null)
            Target = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            transform.position = Vector3.Lerp(walkStartLocation, walkEndLocation, walkingPercentage);
            walkingPercentage += MotionSpeed * Time.deltaTime;
            if (walkingPercentage >= 1f)
            {
                isWalking = false;
                IsMoving = false;
                var from = new Vector2Int(TileX, TileY);
                TileX = walkTileX;
                TileY = walkTileY;
                var args = new MoveEventArgs()
                    {
                        FromTile = from,
                        ToTile = new Vector2Int(walkTileX, walkTileY)
                    };
                TilePositionChanged?.Invoke(this, args);
                MoveableMoved?.Invoke(this, args);
            }
        }
    }
}
