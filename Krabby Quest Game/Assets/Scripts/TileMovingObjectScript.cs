using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int TileX { get; set; }
    public int TileY { get; set; }
    public bool IsMoving { get; private set; }

    bool isWalking = false;
    float walkingPercentage = 0f;
    Vector3 walkStartLocation, walkEndLocation;
    int walkTileX = 0, walkTileY = 0;

    private Vector3 getDestination(int x, int y) => new Vector3(-x * LevelObjectManager.Grid_Size.x, transform.position.y, y * LevelObjectManager.Grid_Size.y);  

    public void JumpToTile(int x, int y) {
        TileX = x;
        TileY = y;
        transform.position = getDestination(x, y);    
    }

    public void WalkToTile(int x, int y)
    {
        isWalking = true;
        walkingPercentage = 0f;
        walkStartLocation = transform.position;
        walkEndLocation = getDestination(x, y);
        walkTileX = x;
        walkTileY = y;
        IsMoving = true;
        TilePositionChanging?.Invoke(this,
            new MoveEventArgs()
            {
                FromTile = new Vector2Int(TileX, TileY),
                ToTile = new Vector2Int(walkTileX, walkTileY)
            });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            transform.position = Vector3.Lerp(walkStartLocation, walkEndLocation, walkingPercentage);
            walkingPercentage += .02f;
            if (walkingPercentage >= 1f)
            {
                isWalking = false;
                IsMoving = false;
                var from = new Vector2Int(TileX, TileY);
                TileX = walkTileX;
                TileY = walkTileY;
                TilePositionChanged?.Invoke(this,
                    new MoveEventArgs()
                    {
                        FromTile = from,
                        ToTile = new Vector2Int(walkTileX, walkTileY)
                    });
            }
        }
    }
}
