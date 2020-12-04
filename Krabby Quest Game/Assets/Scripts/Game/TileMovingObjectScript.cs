using Assets.Components.World;
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
    public bool CanMoveOverWorldReservedTiles = true;
    /// <summary>
    /// Jetstreams, buttons will ignore this object
    /// </summary>
    public bool SpecialObjectIgnore = false;
    private bool unreservedWalkingTilePrev = false;

    public PlayerEnum Player { get; set; } = PlayerEnum.ANYONE;

    public Vector2Int DestinationTile => new Vector2Int(walkTileX, walkTileY);

    /// <summary>
    /// The amount of time to wait in between movement (in seconds)
    /// </summary>
    public float MotionCooldown { get; set; }
    /// <summary>
    /// The current waited amount of time since the last movement. If MotionCooldown is not set, this is always -1.
    /// </summary>
    public float CurrentMotionCooldownTime { get; private set; } = -1;
    /// <summary>
    /// If the current cooldown time is -1, motion is allowed. If this is false, <c>WalkToTile</c> will always return <c>false</c>
    /// </summary>
    public bool CooldownAllowedToMove => CurrentMotionCooldownTime == -1;

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
    /// Walks to the specified tile if the motion is allowed, and the current <see cref="CurrentMotionCooldownTime"/> is completed (-1).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>True if motion is allowed</returns>
    public bool WalkToTile(int x, int y, float overrideMotionSpeed = default)
    {
        //if (PreemptiveBlockMotion(x, y)) return false;  
        if (CurrentMotionCooldownTime != -1)
        {
            Debug.LogWarning($"Motion Refused for: {gameObject.name} as the motion cooldown (set in KrabbyQuestTools) is not yet completed: {CurrentMotionCooldownTime}/{MotionCooldown}");
            return false; // cannot walk until cooldown timer is completed.
        }
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
        if (!CanMoveOverWorldReservedTiles && !args.BlockMotion && !NoClip)
            if (World.Current.IsTileReserved(x, y))
            {
                args.BlockMotion = true;
                args.BlockMotionSender = "WORLD_RESERVEFLAG";
            }
        if (args.BlockMotion && !NoClip)
        {
            isWalking = false;
            IsMoving = false;
            MotionCanceled?.Invoke(this, args);
            OnObjectMotionCanceled?.Invoke(this, args);
            Debug.LogWarning("Movement Canceled for: " + gameObject.name + " by: " + args.BlockMotionSender);            
            return false;
        }
        World.Current.CollisionMapUpdate(gameObject, true, x, y);
        return true;
    }

    private void OnDestroy()
    {
        World.Current.CollisionMapUpdate(gameObject, false, TileX, TileY);
    }

    /// <summary>
    /// Uses <see cref="World"/> data to check if an object capable of blocking motion is around the player.
    /// </summary>
    /// <param name="ToX"></param>
    /// <param name="ToY"></param>
    /// <returns></returns>
    public bool PreemptiveBlockMotion(int ToX, int ToY)
    {
        if (World.Current.TryGetBlockAt(BlockLayers.Integral, ToX, ToY, out var info))
        {
            if (info.GetParameterByName("AllowMotion", out var value) && value.Value.ToLower() == "true")
                return true;
            return false;
        }
        else return false;
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

    public bool MoveInDirection(SRotation Direction, int Tiles = 1, float overrideMotionSpeed = default)
    {
        var destination = GetTileFromDirection(Direction, Tiles);
        return WalkToTile(destination.x, destination.y, overrideMotionSpeed);
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (Target == null)
            Target = gameObject;
        if (TryGetComponent<DataBlockComponent>(out var component)) // PARAMETER: MotionSpeed
        {
            if (component.DataBlock.GetParameterByName<float>("MotionSpeed", out var data))
                MotionSpeed = data.Value;
            if (component.DataBlock.GetParameterByName<float>("MotionCooldown", out data))
                MotionCooldown = data.Value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            transform.position = Vector3.Lerp(walkStartLocation, walkEndLocation, walkingPercentage);
            walkingPercentage += MotionSpeed * Time.deltaTime;
            if (walkingPercentage >= .5f) // 50% complete walking
            {
                if (TileX != walkTileX || TileY != walkTileY)
                {
                    if (!unreservedWalkingTilePrev)
                    {
                        unreservedWalkingTilePrev = true;
                        World.Current.CollisionMapUpdate(gameObject, false, TileX, TileY); // unreserve tile halfway through walking
                    }
                }
            }
            if (walkingPercentage >= 1f) // 100% done walking
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
                World.Current.CollisionMapUpdate(gameObject, true, TileX, TileY); // unreserve tile halfway through walking
                unreservedWalkingTilePrev = false;
                if (MotionCooldown > 0)
                    CurrentMotionCooldownTime = 0;
                else CurrentMotionCooldownTime = -1;
            }
        }
        else if (CurrentMotionCooldownTime >= 0)
        {
            CurrentMotionCooldownTime += Time.deltaTime;
            if (CurrentMotionCooldownTime > MotionCooldown)
                CurrentMotionCooldownTime = -1;
        }
    }

    public static SRotation GetBehindDirection(SRotation rotation)
    {
        switch (rotation) // get behind direction
        {
            case SRotation.EAST:
                return SRotation.WEST;                
            case SRotation.WEST:
                return SRotation.EAST;                
            case SRotation.SOUTH:
                return SRotation.NORTH;                
            case SRotation.NORTH:
                return SRotation.SOUTH;
        }
        return SRotation.EAST;    
    }
}
