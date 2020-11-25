using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooberBehavior : MonoBehaviour
{
    TileMovingObjectScript MovementScript
    {
        get; set;
    }
    public SRotation Direction;
    public bool IsDestoryed
    {
        get; private set;
    }
    public bool CanMove
    {
        get; private set;
    }
    float timeSinceDestroyed, timeUntilDelete = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartProjectile(Vector2Int Position, SRotation Direction)
    {
        MovementScript = GetComponent<TileMovingObjectScript>();
        MovementScript.SpecialObjectIgnore = true;
        MovementScript.JumpToTile(Position.x, Position.y);
        this.Direction = Direction;
        CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (MovementScript == null) return;
        if (!MovementScript.IsMoving && CanMove)
        {
            bool blockMotion = false;
            switch (Direction)
            {
                case StinkyFile.SRotation.NORTH:
                    blockMotion = !MovementScript.WalkToTile(MovementScript.TileX, MovementScript.TileY - 1);
                    break;
                case StinkyFile.SRotation.SOUTH:
                    blockMotion = !MovementScript.WalkToTile(MovementScript.TileX, MovementScript.TileY + 1);
                    break;
                case StinkyFile.SRotation.EAST:
                    blockMotion = !MovementScript.WalkToTile(MovementScript.TileX + 1, MovementScript.TileY);
                    break;
                case StinkyFile.SRotation.WEST:
                    blockMotion = !MovementScript.WalkToTile(MovementScript.TileX - 1, MovementScript.TileY);
                    break;
            }
            if (blockMotion)
            {
                IsDestoryed = true;
                CanMove = false;
                GetComponent<Renderer>().enabled = false; // make invisible
            }
        }
        if (timeSinceDestroyed > timeUntilDelete)
            Destroy(gameObject);
        else if (IsDestoryed)
            timeSinceDestroyed += Time.deltaTime;
    }
}
