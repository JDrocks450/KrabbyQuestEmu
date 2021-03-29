using Assets.Scripts.Game;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The EventArgs for <see cref="TileMovingObjectScript"/> events
/// </summary>
public class MoveEventArgs
{
    private TileMovingObjectScript parent;
    internal MoveEventArgs(TileMovingObjectScript creator)
    {
        parent = creator;
    }

    /// <summary>
    /// Cancel the motion
    /// </summary>
    public bool BlockMotion = false;
    /// <summary>
    /// The name of the object that blocked the current motion
    /// </summary>
    public string BlockMotionSender;
    public Vector2Int FromTile, ToTile;
    /// <summary>
    /// If one of the other players is pushing it
    /// </summary>
    public PlayerEnum Player = PlayerEnum.SPONGEBOB;
    /// <summary>
    /// The speed of the other object
    /// </summary>
    public float MotionSpeed = 0f;
    /// <summary>
    /// The direction of the current motion
    /// </summary>
    public SRotation Direction
    {
        get
        {
            if (FromTile.x < ToTile.x)
                return SRotation.EAST;
            else if (FromTile.x > ToTile.x)
                return SRotation.WEST;
            else if (FromTile.y < ToTile.y)
                return SRotation.SOUTH;
            else return SRotation.NORTH;
        }
    }

    /// <summary>
    /// If called during a PositionChanging event, this will invoke the TileMovingObjectScript
    /// to be made aware that the tile being walked onto is raised off the ground by the parameter's value in units.
    /// <para>
    /// Note that this flag is automatically cleared the next time <see cref="TileMovingObjectScript.WalkToTile(int, int, float)"/> or <see cref="TileMovingObjectScript.JumpToTile(int, int)"/> are called.
    /// </para>    
    /// </summary>
    public void SetRaisedTerrainFlag(float raiseAmount)
    {
        parent.SetRaisedTerrainFlag(raiseAmount);
    }
}
