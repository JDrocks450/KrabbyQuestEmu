using StinkyFile;
using StinkyFile.Save;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Components.World
{
    public struct WorldPickupDefinition
    {
        public float AmountCollected;
        public float AmountTotal;

        public WorldPickupDefinition(float amountCollected, float amountTotal) : this()
        {
            AmountCollected = amountCollected;
            AmountTotal = amountTotal;
        }
    }

    public class World
    {
        static string AssetDirectory => TextureLoader.AssetDirectory;
        public static World Current
        {
            get; private set;
        }

        private bool[,] ReservedTiles;
        private GameObject[,] SenderReserveTiles;

        /// <summary>
        /// The absolute path to the level file
        /// </summary>
        public Uri FileName { get; }

        /// <summary>
        /// The current <see cref="StinkyParser"/> instance -- do not use this unless necessary!!
        /// </summary>
        public static StinkyParser Parser => GameInitialization.GlobalParser;

        public StinkyLevel Level { get; private set; }

        public LevelContext Context { get; private set; }

        /// <summary>
        /// The current completion status of the level
        /// </summary>
        public static LevelCompletionInfo CurrentCompletionInfo
        {
            get; private set;
        }

        /// <summary>
        /// The current pickups that are in this world, with their collection status
        /// </summary>
        public Dictionary<string, WorldPickupDefinition> Pickups { get; internal set; } = new Dictionary<string, WorldPickupDefinition>();

        /// <summary>
        /// Gets whether all the patties on this level have been collected by the player
        /// </summary>
        public bool AllPattiesCollected
        {
            get
            {
                if (Pickups.TryGetValue("PATTY", out var pattyinfo))
                {
                    return pattyinfo.AmountCollected == pattyinfo.AmountTotal;

                }
                return false;
            }
        }

        /// <summary>
        /// Loads the (*.lv5) level from the disc to a <see cref="World"/> object
        /// </summary>
        /// <param name="FullFileName"></param>
        public World(Uri FullFileName)
        {
            FileName = FullFileName;
            Load();
        }

        /// <summary>
        /// Loads the (*.lv5) level from the disc to a <see cref="World"/> object
        /// </summary>
        /// <param name="FullFileName">Relative file name in Workspace/Levels directory</param>
        public World(string LevelRelativeFileName) : this(new Uri(Path.Combine(AssetDirectory, "levels", LevelRelativeFileName), UriKind.Absolute))
        {

        }

        /// <summary>
        /// Sets the current world to this
        /// </summary>
        /// <param name="world"></param>
        public static void SetCurrent(World world) => Current = world;

        private void Load()
        {
            Level = Parser.LevelRead(FileName.LocalPath);
            ReservedTiles = new bool[Level.Columns, Level.Rows];
            SenderReserveTiles = new GameObject[Level.Columns, Level.Rows];
            Context = Level.Context;
            Parser.RefreshLevel(Level);
            Pickups.Clear();
            Player.CurrentPlayer = Assets.Scripts.Game.PlayerEnum.SPONGEBOB;
        }

        /// <summary>
        /// Applies the selected <see cref="LevelCompletionInfo"/>, or makes a new one if it is null.
        /// </summary>
        /// <param name="completionInfo">The level completion status to apply, null makes a new one</param>
        /// <param name="timeRemaining">The time remaining for this level</param>
        public void LoadSave(LevelCompletionInfo completionInfo = default, float timeRemaining = LevelCompletionInfo.DefaultTime)
        {
            if (completionInfo == default)
            {
                if (SaveFileManager.IsFileOpened)
                    CurrentCompletionInfo = Level.GetSaveFileInfo(SaveFileManager.Current);
                else 
                    CurrentCompletionInfo = new LevelCompletionInfo()
                    {
                        LevelName = Level.Name,
                        LevelWorldName = Level.LevelWorldName,
                        TimeRemaining = timeRemaining
                    };
            }
            else CurrentCompletionInfo = completionInfo;
            if (CurrentCompletionInfo != null)
            {
                CurrentCompletionInfo.TimeRemaining = timeRemaining;
                CurrentCompletionInfo.PattiesCollected = 0;
                CurrentCompletionInfo.BonusesCollected = 0;                
            }
            if (!Pickups.ContainsKey("TIME"))
                Pickups.Add("TIME", new WorldPickupDefinition(timeRemaining, timeRemaining));
        }

        /// <summary>
        /// Adds a pickup to the total tracked pickups in the level.
        /// </summary>
        /// <param name="pickupName"></param>
        public void AddPickup(string pickupName, int amount = 1, int DefaultAmountCollected = 0, int DefaultAmountTotal = 1)
        {
            if (Pickups.TryGetValue(pickupName, out var info))
                Pickups[pickupName] = new WorldPickupDefinition(info.AmountCollected, info.AmountTotal + amount);
            else Pickups.Add(pickupName, new WorldPickupDefinition(DefaultAmountCollected, DefaultAmountTotal));            
        }

        /// <summary>
        /// Removes an amount of pickups from the tracked ones in the World
        /// </summary>
        /// <param name="pickupName">The name of the pickup</param>
        /// <param name="amount">The amount to remove</param>
        public void RemovePickup(string pickupName, float amount = 1)
        {
            if (Pickups.TryGetValue(pickupName, out var info))
            {
                if (amount > info.AmountTotal + info.AmountCollected)
                    amount = info.AmountTotal - info.AmountCollected;
                Pickups[pickupName] = new WorldPickupDefinition(info.AmountCollected + amount, info.AmountTotal);
            }
        }

        /// <summary>
        /// Sets the remaining time in seconds
        /// </summary>
        /// <param name="SecondsRemaining"></param>
        public void UpdateTimeRemaining(int SecondsRemaining)
        {
            if (Pickups.ContainsKey("TIME"))
                Pickups["TIME"] = new WorldPickupDefinition(SecondsRemaining, Pickups["TIME"].AmountTotal);
            else AddPickup("TIME");
        }

        /// <summary>
        /// Finishes the level and fills in all completion fields for this World.
        /// </summary>
        /// <param name="SecondsLeft">The final amount of time left when the level was completed</param>
        /// <param name="Successful">Whether this should be considered successful or not.</param>
        public void Finish(int SecondsLeft, bool Successful)
        {
            if (CurrentCompletionInfo != null)
            {
                CurrentCompletionInfo.LevelName = Level.Name;
                CurrentCompletionInfo.LevelWorldName = Level.LevelWorldName;
                CurrentCompletionInfo.TimeRemaining = SecondsLeft;
                if (!CurrentCompletionInfo.WasSuccessful)
                    CurrentCompletionInfo.WasSuccessful = Successful;
                if (TryGetPickupInfo("PATTY", out var pattyinfo))
                {
                    CurrentCompletionInfo.PattiesCollected = (int)pattyinfo.AmountCollected;
                }
                if (TryGetPickupInfo("BONUS", out var bonusinfo))
                {
                    CurrentCompletionInfo.BonusesCollected = (int)bonusinfo.AmountCollected;
                    if (bonusinfo.AmountCollected == bonusinfo.AmountTotal)
                        CurrentCompletionInfo.WasPerfect = true;
                }
                else CurrentCompletionInfo.WasPerfect = true;

            }
            SaveFileManager.Current.UpdateInfo(CurrentCompletionInfo);
            SaveFileManager.Current.Save();
        }

        /// <summary>
        /// Attempts to get the pickup information of a pickup by name
        /// </summary>
        /// <param name="PickupName">The name of the pickup</param>
        /// <param name="Info">The return value</param>
        /// <returns></returns>
        public bool TryGetPickupInfo(string PickupName, out WorldPickupDefinition Info)
        {
            if (Pickups.TryGetValue(PickupName, out var info))
            {
                Info = info;
                return true;
            }
            Info = default;
            return false;
        }

        /// <summary>
        /// Verifies that the position provided is within the bounds of the World
        /// </summary>
        /// <param name="X">The X Position</param>
        /// <param name="Y">The Y Position</param>
        /// <returns></returns>
        public bool VerifyTilePosition(int X, int Y) => X < Level.Columns && Y < Level.Rows && X > 0 && Y > 0;

        /// <summary>
        /// Gets the <see cref="LevelDataBlock"/> at the specified TilePosition in the World.
        /// </summary>
        /// <param name="Layer">The layer to get blocks from</param>
        /// <param name="X">The X Position</param>
        /// <param name="Y">The Y Position</param>
        /// <returns></returns>
        public LevelDataBlock GetBlockAt(BlockLayers Layer, int X, int Y) => (Layer == BlockLayers.Integral ? Level.IntegralData : Level.DecorationData)[Y * Level.Columns + X];
        /// <summary>
        /// Gets the <see cref="LevelDataBlock"/> at the specified TilePosition in the World, if <see cref="VerifyTilePosition(int, int)"/> returns true
        /// </summary>
        /// <param name="Layer">The layer to get blocks from</param>
        /// <param name="X">The X Position</param>
        /// <param name="Y">The Y Position</param>
        /// <param name="BlockInfo">The Block data if found</param>
        /// <returns></returns>
        public bool TryGetBlockAt(BlockLayers Layer, int X, int Y, out LevelDataBlock BlockInfo)
        {
            BlockInfo = null;
            if (VerifyTilePosition(X, Y))
            {
                BlockInfo = GetBlockAt(Layer, X, Y);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if this tile is marked as reserved. 
        /// <para>See: <see cref="CollisionMapUpdate(bool, int, int)"/></para>
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public bool IsTileReserved(int X, int Y)
        {
            if (!VerifyTilePosition(X, Y))
                return false;
            return ReservedTiles[X, Y];
        }
        /// <summary>
        /// Checks if this tile is marked as reserved. 
        /// <para>See: <see cref="CollisionMapUpdate(bool, int, int)"/></para>
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public bool IsTileReservedByMe(GameObject sender, int X, int Y)
        {
            if (!VerifyTilePosition(X, Y))
                return false;
            if (SenderReserveTiles[X, Y] == sender)
                return ReservedTiles[X, Y];
            return false;
        }

        /// <summary>
        /// Marks this tile as reserved/unpassable
        /// </summary>
        /// <param name="Reserve">Should reserve or free the tile</param>
        /// <param name="TileX">The X Position</param>
        /// <param name="TileY">The Y Position</param>
        public bool CollisionMapUpdate(GameObject sender, bool Reserve, int TileX, int TileY)
        {
            if (!VerifyTilePosition(TileX, TileY))
                return false;
            if (IsTileReserved(TileX, TileY) && sender != SenderReserveTiles[TileX, TileY]) // the gameobject who reserved this tile needs to free it first
                return false;
            ReservedTiles[TileX, TileY] = Reserve;
            SenderReserveTiles[TileX, TileY] = sender;
            return true;
        }

        public void ForceCollisionFree(int TileX, int TileY)
        {
            ReservedTiles[TileX, TileY] = false;
            SenderReserveTiles[TileX, TileY] = null;
        }
    }
}
