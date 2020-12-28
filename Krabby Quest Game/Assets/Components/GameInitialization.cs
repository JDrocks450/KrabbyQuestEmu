using Assets.Components;
using Assets.Components.GLB;
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
    public static bool Initialized => initialized;
    public static StinkyParser GlobalParser = new StinkyParser();

    public static void Initialize()
    {
        if (initialized == true) return;
        LevelDataBlock.BlockDatabasePath = "Assets/Resources/blockdb.xml";
        AssetDBEntry.AssetDatabasePath = "Assets/Resources/texturedb.xml";
        AnimationDatabase.RelativeAnimationDatabasePath = "Assets/Resources/animDB.json";
        MapWaypointParser.DBPath = "Assets/Resources/mapdb.xml";        
        XDocument doc = XDocument.Load(AssetDBEntry.AssetDatabasePath);
        TextureLoader.AssetDirectory = doc.Root.Element("WorkspaceDirectory").Value;
        AnimationCompiler.GlobalAnimationCompiler = new AnimationCompiler();
        _ = FontCreator.KrabbyQuestFont;
        initialized = true;
    }
}
