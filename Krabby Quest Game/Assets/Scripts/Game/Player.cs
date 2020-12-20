using Assets.Scripts.Game;
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

    public static Player Current { get; private set; }

    public static PlayerEnum CurrentPlayer
    {
        get; set;
    } = PlayerEnum.SPONGEBOB;
    static bool playerSwapping = false;
    static PlayerEnum swappingPlayer;

    public string PlayerName
    {
        get; set;
    }
    static bool dying = false;
    static float dyingTimer = 0f;
    AngleRotator rotator;

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
                _tileScript.Target = gameObject;
            }
            return _tileScript;
        }
    }

    public int TileX => TileMoveScript.TileX;
    public int TileY => TileMoveScript.TileY;
    public bool IsMoving => TileMoveScript.IsMoving;
    public Camera GameCam;
    public Transform Transform;

    public void JumpToTile(int x, int y) => TileMoveScript.JumpToTile(x, y);
    public void WalkToTile(int x, int y) => TileMoveScript.WalkToTile(x, y);

    // Start is called before the first frame update
    void Start()
    {        
        JumpToTile(TileX, TileY);
        GameCam = GameObject.Find("Camera").GetComponent<Camera>();
        Transform = transform;
        TileMoveScript.MotionSpeed = 3.5f;
        switch (PlayerName)
        {
            case "spongebob":
                TileMoveScript.Player = PlayerEnum.SPONGEBOB;
                break;
            case "patrick":
                TileMoveScript.Player = PlayerEnum.PATRICK;
                break;
        }        
        TileMovingObjectScript.MoveableMoving += TileMovingObjectScript_MoveableMoving;
        rotator = GetComponentInChildren<AngleRotator>();
    }

    private void TileMovingObjectScript_MoveableMoving(object sender, MoveEventArgs e)
    {
        if (e.ToTile.x == TileMoveScript.TileX && e.ToTile.y == TileMoveScript.TileY)
        {
            if ((sender as TileMovingObjectScript).GetComponent<Player>() != null) // is a player
            {
                e.BlockMotion = true;
                e.BlockMotionSender = gameObject.name;
            }
            else KillAllPlayers();
        }
    }

    private void OnDestroy()
    {
        TileMovingObjectScript.MoveableMoving -= TileMovingObjectScript_MoveableMoving;
    }

    public static void KillAllPlayers()
    {
        var animator = GameObject.Find("Spongebob").GetComponentInChildren<Animator>();
        animator.enabled = true;
        animator.Play("Die");
        animator = GameObject.Find("Patrick").GetComponentInChildren<Animator>();
        animator.enabled = true;
        animator.Play("Die");
        SoundLoader.Play("sb-death.wav", true);
        dying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (dying)
        {
            float deathTime = 2.5f;
            dyingTimer += Time.deltaTime;
            if (dyingTimer >= deathTime)
            {
                LevelObjectManager.ReloadLevel(); // reset the level
                dying = false;
                dyingTimer = 0f;
            }
            return; // prevent player control when dying.
        }
        _ = TileMoveScript; // force update of component
        PlayerEnum updatingPlayer = PlayerName == "spongebob" ? PlayerEnum.SPONGEBOB : PlayerEnum.PATRICK;
        bool currentPlayer = (PlayerName == "spongebob" && CurrentPlayer == PlayerEnum.SPONGEBOB) ||
                            (PlayerName == "patrick" && CurrentPlayer == PlayerEnum.PATRICK);     
        if (updatingPlayer == swappingPlayer && playerSwapping) // this frame is the update frame after swapping
            playerSwapping = false;
        if (currentPlayer)
        {
            Current = this;
            if (!IsMoving)
            {
                if (Input.GetKey(KeyCode.W)) // forward
                {
                    WalkToTile(TileX, TileY - 1);
                    rotator.Rotate(SRotation.NORTH);
                }
                else if (Input.GetKey(KeyCode.S)) // backward
                {
                    WalkToTile(TileX, TileY + 1);
                    rotator.Rotate(SRotation.SOUTH);
                }
                else if (Input.GetKey(KeyCode.A)) // left
                {
                    WalkToTile(TileX - 1, TileY);
                    rotator.Rotate(SRotation.WEST);
                }
                else if (Input.GetKey(KeyCode.D)) // right
                {
                    WalkToTile(TileX + 1, TileY);
                    rotator.Rotate(SRotation.EAST);
                }
                if (Input.GetKeyDown(KeyCode.Q))
                    TileMoveScript.NoClip = !TileMoveScript.NoClip; // no clip toggle
                if (Input.GetKeyDown(KeyCode.K))
                    KillAllPlayers(); // kill me now
            }
            if (Input.GetKeyDown(KeyCode.Space) && !playerSwapping)
            {
                swappingPlayer = CurrentPlayer;
                switch (CurrentPlayer) // switch current player
                {
                    case PlayerEnum.SPONGEBOB:
                        CurrentPlayer = PlayerEnum.PATRICK; // patrick
                        break;
                    case PlayerEnum.PATRICK:
                        CurrentPlayer = PlayerEnum.SPONGEBOB; // spongebob
                        break;
                }
                SoundLoader.Play("teleport.wav", false);
                playerSwapping = true;
            }
        }        
    }
}
