using Assets.Components;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

internal static class GameInitialization
{
    static bool initialized = false;
    public static void Initialize()
    {
        if (initialized == true) return;
        LevelDataBlock.BlockDatabasePath = "Assets/Resources/blockdb.xml";
        AssetDBEntry.AssetDatabasePath = "Assets/Resources/texturedb.xml";
        MapWaypointParser.DBPath = "Assets/Resources/mapdb.xml";
        StinkyFile.Save.SaveFile.SaveFileDir = "Saves/";
        XDocument doc = XDocument.Load(AssetDBEntry.AssetDatabasePath);
        TextureLoader.AssetDirectory = doc.Root.Element("WorkspaceDirectory").Value;
        _ = FontCreator.KrabbyQuestFont;
        initialized = true;
    }
}
