using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /// <summary>
    /// When Spongebob fully moves from one world tile onto another
    /// </summary>
    public event EventHandler<MoveEventArgs> PlayerPositionChanged;
    /// <summary>
    /// When Spongebob begins to move from one world tile to another
    /// </summary>
    public event EventHandler<MoveEventArgs> PlayerPositionChanging;

    TileMovingObjectScript _tileScript;
    TileMovingObjectScript TileMoveScript
    {
        get
        {
            if (_tileScript == null)
            {
                _tileScript = GetComponent<TileMovingObjectScript>();
                _tileScript.TilePositionChanged += (object s, MoveEventArgs e) => PlayerPositionChanged?.Invoke(s, e);
                _tileScript.TilePositionChanging += (object s, MoveEventArgs e) => PlayerPositionChanging?.Invoke(s, e);
            }
            return _tileScript;
        }
    }

    public int TileX => TileMoveScript.TileX;
    public int TileY => TileMoveScript.TileY;
    public bool IsMoving => TileMoveScript.IsMoving;

    public void JumpToTile(int x, int y) => TileMoveScript.JumpToTile(x, y);
    public void WalkToTile(int x, int y) => TileMoveScript.WalkToTile(x, y);

    // Start is called before the first frame update
    void Start()
    {        
        JumpToTile(TileX, TileY);    
    }

    // Update is called once per frame
    void Update()
    {
        _ = TileMoveScript; // force update of component
        if (!IsMoving)
        {
            if (Input.GetKeyDown(KeyCode.W)) // forward
            {
                WalkToTile(TileX, TileY - 1);
                //transform.Ro(Vector3.up, 180f);
            }
            else if (Input.GetKeyDown(KeyCode.S)) // backward
            {
                WalkToTile(TileX, TileY + 1);
                //transform.Rotate(Vector3.up, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.A)) // left
            {
                WalkToTile(TileX - 1, TileY);
                //transform.Rotate(Vector3.up, -90f);
            }
            else if (Input.GetKeyDown(KeyCode.D)) // right
            {
                WalkToTile(TileX + 1, TileY);
                //transform.Rotate(Vector3.up, 90);
            }
        }        
    }
}
