using Assets.Components.World;
using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullyBehavior : MonoBehaviour
{
    public enum EnemyMode
    {
        IDLE,
        CHASE,
        PATROL
    }
    /// <summary>
    /// The current chasing mode of the enemy
    /// </summary>
    public EnemyMode CurrentMode = EnemyMode.PATROL;

    /// <summary>
    /// The current facing direction of the enemy
    /// </summary>
    public SRotation Rotation;
    
    public DataBlockComponent BlockComponent { get; private set; }
    
    public TileMovingObjectScript TileScript { get; private set; }
    public int TileX => TileScript.TileX;
    public int TileY => TileScript.TileY;
    public bool IsMoving => TileScript.IsMoving;

    /// <summary>
    /// Dictates whether this enemy should chase the player
    /// </summary>
    private bool shouldChasePlayer = false;
    /// <summary>
    /// The direction the player is in from this enemy
    /// </summary>
    private SRotation playerDirection;
    
    private AngleRotator Rotator;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        Rotation = BlockComponent.DataBlock.Rotation;
        transform.eulerAngles = new Vector3();
        TileScript = GetComponent<TileMovingObjectScript>();
        TileScript.JumpToTile(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        TileScript.MotionSpeed = 2f;
        TileScript.CanMoveOverWorldReservedTiles = false;
        Rotator = GetComponentInChildren<AngleRotator>();
        var loopSource = GetComponent<SoundLoader>().LoopStart("sb-bullyx", out _, true); // walking sound looped
        if (loopSource != null)
            loopSource.volume = .25f;
    }  

    /// <summary>
    /// Looks left or right of the character depending on what mode it's in
    /// </summary>
    /// <param name="lookLeft"></param>
    static SRotation GetRightLeft(SRotation Rotation, bool lookRight = true)
    {
        bool Mode = lookRight;
        switch (Rotation)
        {
            case SRotation.NORTH:
                return Mode ? SRotation.EAST : SRotation.WEST;                                
            case SRotation.EAST:
                return Mode ? SRotation.SOUTH : SRotation.NORTH;                
            case SRotation.SOUTH:
                return Mode ? SRotation.WEST : SRotation.EAST;                
            case SRotation.WEST:
                return Mode ? SRotation.NORTH : SRotation.SOUTH;                
        }
        return SRotation.EAST;
    }

    void FaceDirection(SRotation Direction) => Rotator.Rotate(Direction);

    bool MoveForward()
    {
        bool result = false;
        switch (Rotation)
        {
            case SRotation.NORTH:
                result = TileScript.WalkToTile(TileScript.TileX, TileScript.TileY - 1);
                FaceDirection(SRotation.NORTH);
                break;
            case SRotation.SOUTH:
                result = TileScript.WalkToTile(TileScript.TileX, TileScript.TileY + 1);
                FaceDirection(SRotation.SOUTH);
                break;
            case SRotation.WEST:
                result = TileScript.WalkToTile(TileScript.TileX - 1, TileScript.TileY);
                FaceDirection(SRotation.WEST);
                break;
            case SRotation.EAST:
                result = TileScript.WalkToTile(TileScript.TileX + 1, TileScript.TileY);
                FaceDirection(SRotation.EAST);
                break;
        }
        return result;
    }

    void UpdatePlayerLocation()
    {
        shouldChasePlayer = false;
        Player player = Player.Current;
        if (player == null) return;
        bool isLeft, isRight, isUp, isDown;
        isLeft = player.TileX < TileX;
        isRight = player.TileX > TileX;
        isUp = player.TileY < TileY;
        isDown = player.TileY > TileY;
        if ((isLeft || isRight) && (!isUp & !isDown)) // Player is lined up in the X direction
        {
            if (isLeft) playerDirection = SRotation.WEST;
            if (isRight) playerDirection = SRotation.EAST;            
            shouldChasePlayer = true;
        }
        if ((isUp || isDown) && (!isLeft & !isRight)) // Player is lined up in the Y direction
        {
            if (isUp) playerDirection = SRotation.NORTH;
            if (isDown) playerDirection = SRotation.SOUTH;
            shouldChasePlayer = true;
        }
        if (playerDirection == GetBehind(Rotation)) // if the player is behind them, they wont notice
            shouldChasePlayer = false;
    }

    /// <summary>
    /// Line of sight checking in a certain direction until a certain point
    /// </summary>
    /// <returns></returns>
    bool LOSCheck(SRotation Direction, int FromX, int FromY, int ToX, int ToY)
    {
        int directionFrom = FromX, directionTo=ToX;
        switch (Direction)
        {
            case SRotation.NORTH:
            case SRotation.SOUTH: // set the endpoint to Y direction
                directionFrom = FromY;
                directionTo = ToY;
                break;
            case SRotation.WEST:
            case SRotation.EAST: // set the endpoint to X direction
                directionFrom = FromX;
                directionTo = ToX;
                break;
        }
        int currentPosX = FromX, currentPosY = FromY;
        World current = World.Current;
        if (current == null) return false;
        int amountOfTimes = Math.Abs(directionTo - directionFrom);
        for(int i = 0; i < amountOfTimes; i++)
        {
            int change = Direction == SRotation.NORTH || Direction == SRotation.WEST ? -1 : 1; // the change per loop, -1 for left and up
            if (!current.TryGetBlockAt(BlockLayers.Integral, currentPosX, currentPosY, out var blockData))
                return false;
            bool isPassable = false;
            if (blockData.GetParameterByName("AllowMotion", out var blockParameter) && blockParameter.Value.ToLower() == "true")
                isPassable = true;
            if (blockData.GetParameterByName("FLOOR", out blockParameter) && blockParameter.Value.ToLower() == "true") // ran into a non-passable object
                isPassable = true;
            if (blockData.GetParameterByName("Bully_Ignore", out blockParameter) && blockParameter.Value.ToLower() == "true")
                isPassable = true;
            if (i > 0 && i < amountOfTimes)
            {
                if (isPassable && !current.IsTileReserved(currentPosX, currentPosY))
                    isPassable = true;
                else isPassable = false;
            }
            if (!isPassable) return false;            
            switch (Direction)
            {
                case SRotation.NORTH:
                case SRotation.SOUTH: // apply change in Y direction
                    currentPosY += change;
                    if (currentPosY == ToY) return true;
                    break;
                case SRotation.WEST:
                case SRotation.EAST: // apply change in X direction
                    currentPosX += change;
                    if (currentPosX == ToX) return true;
                    break;
            }            
        }
        return true;
    }

    bool ShouldChase
    {
        get
        {
            return shouldChasePlayer;
        }
    }

    void Patrol()
    {
        TileScript.MotionSpeed = 2f;
        if (!TileScript.IsMoving)
        {            
            bool[] attemptedRotations = new bool[] { false, false, false, false };            
            SRotation startingRotation = Rotation;
            for (int i = 0; i < 3; i++) // three tries to try walking
            {
                attemptedRotations[(int)Rotation] = true;
                if (MoveForward()) // try moving forward
                    break;             
                if (i != 2)
                    Rotation = GetRightLeft(startingRotation, i == 0); // check right, then left
                else // else look behind
                {
                    Rotation = GetBehind(startingRotation);
                    if (!MoveForward()) // try to go backwards, if not, then freeze he's stuck
                    {
                        CurrentMode = EnemyMode.IDLE;
                        break;
                    }
                }
            }
        }
    }

    private SRotation GetBehind(SRotation rotation)
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

    void Chase()
    {
        TileScript.MotionSpeed = 4f;
        if (!TileScript.IsMoving)
        {
            if (!ShouldChase) // stop chasing when they're not in view
            {
                CurrentMode = EnemyMode.PATROL;
                return;
            }
            if (!MoveForward()) // motion blocked
            {
                if (!StartChase()) // player is still lined up                
                    CurrentMode = EnemyMode.PATROL;
            }
        }
    }

    bool StartChase()
    {
        var player = Player.Current;
        if (player == null) return false;
        if (ShouldChase && LOSCheck(playerDirection, TileX, TileY, player.TileX, player.TileY))
        {
            CurrentMode = EnemyMode.CHASE;
            Rotation = playerDirection;
            GetComponent<SoundLoader>().Play("sb-bully");
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerLocation();
        switch (CurrentMode)
        {
            case EnemyMode.PATROL:                
                if (StartChase())                
                    goto case EnemyMode.CHASE;    
                else 
                    Patrol();
                break;
            case EnemyMode.CHASE:
                Chase();
                if (CurrentMode == EnemyMode.PATROL && !ShouldChase) // mode switched during Chase()
                    goto case EnemyMode.PATROL; // make sure to complete state on frame
                break;
        }
    }
}
