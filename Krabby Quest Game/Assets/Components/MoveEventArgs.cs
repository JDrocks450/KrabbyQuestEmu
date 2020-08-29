using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MoveEventArgs
{
    public Vector2Int FromTile, ToTile;
    public int Player = 0;
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
}
