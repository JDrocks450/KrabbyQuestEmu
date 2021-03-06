﻿using Assets.Components;
using StinkyFile;
using StinkyFile.Save;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map3DLoader : MonoBehaviour
{
    Texture2D markerSheet;
    Sprite[] UnlockedNotCompleted = new Sprite[7], Locked, Completed;
    Vector2 TopLeft = new Vector2(15, -15), BotRight = new Vector2(-5, 5);
    GameObject CurrentLevelIndicator, MarkerTemplate;
    Transform UIGroup;
    MapWaypointParser mparser;
    SaveFile currentSave;
    public StinkyLevel CurrentlySelected
    {
        get; private set;
    }

    // Start is called before the first frame update
    void Start()
    {        
        GameInitialization.Initialize();
        if (SaveFileManager.IsFileOpened)
        {
            currentSave = SaveFileManager.Current;
            currentSave.RefreshStats(); // get the latest level statistics
        }
        var transform = this.transform;
        var objectMaterial = Resources.Load("Materials/Object Material") as Material;
        UIGroup = transform.GetChild(5); // the map UI group
        //apply textures
        for (int i = 0; i < 4; i++)
        {
            var image = transform.GetChild(i).GetComponent<Renderer>();
            var sprite = TextureLoader.RequestTexture($"Graphics/map{4-i}.bmp", null, true);
            image.material = Instantiate(objectMaterial);
            image.material.mainTexture = sprite;
        }
        MarkerTemplate = transform.GetChild(4).gameObject; // marker sample
        LoadWaypoints();
        CurrentLevelIndicator = GetCursor();        
        ChangeSelectedLevel(mparser.Levels.Keys.ElementAt(0));
    }

    void SetUIText()
    {
        if (SaveFileManager.IsFileOpened != true) return;
        UIGroup.GetChild(0).GetComponent<TextMesh>().text = "Score: " + currentSave.SaveFileInfo.TotalScore; // score
        UIGroup.GetChild(1).GetComponent<TextMesh>().text = "Levels: " + currentSave.SaveFileInfo.CompletedLevels; // levels
        UIGroup.GetChild(2).GetComponent<TextMesh>().text = "Spatulas: " + currentSave.SaveFileInfo.Spatulas; // spatulas
    }

    void LoadWaypoints()
    {
        if (markerSheet == null)
            markerSheet = TextureLoader.RequestTexture("Graphics/levelsigns3.bmp", "0,0,0", true);
        mparser = new MapWaypointParser(TextureLoader.AssetDirectory, new StinkyParser());
        mparser.LoadAll();        
        foreach (var waypoint in mparser.Levels)
        {
            PlaceMarker(waypoint.Key, waypoint.Value);
        }
    }

    public GameObject GetCursor()
    {
        //create current level marker
        var indicator = Instantiate(MarkerTemplate, transform, false) as GameObject;
        var renderer = indicator.GetComponent<SpriteRenderer>();
        renderer.sprite =
            Sprite.Create(
                TextureLoader.RequestTexture("Graphics/levelsigns2b.bmp", "0,0,0", true),
                new Rect(0, 0, 64, 64), new Vector2(.5f,.5f));
        renderer.flipX = false;
        indicator.GetComponent<Animator>().Play("Cursor");
        indicator.name = "Cursor";
        return indicator;
    }

    public static Rect GetTextureSource(float x, float y, float w, float h) => new Rect(x, 256 - h - y, w, h);

    public GameObject PlaceMarker(StinkyLevel level, SPoint position)
    {
        var marker = Instantiate(MarkerTemplate.transform, transform, false);
        marker.name = level.Name + " Marker";
        var sprite = UnlockedNotCompleted[(int)level.Context];
        if (sprite == null)
        {
            Rect source = default;
            bool sourceUpdated = false;
            if (SaveFileManager.IsFileOpened)
            {
                var saveFileInfo = level.GetSaveFileInfo(currentSave);
                if (saveFileInfo.WasPerfect)
                {
                    source = GetTextureSource(0, 64, 64, 64); // Original Krabby Patty
                    sourceUpdated = true;
                }
                else if (saveFileInfo.WasSuccessful)
                {
                    source = GetTextureSource(65, 5, 60, 52); // Original Krabby Patty
                    sourceUpdated = true;
                }
                else if (saveFileInfo.IsAvailable)
                {
                    source = GetTextureSource(0, 0, 64, 64); // Original Krabby Patty
                    sourceUpdated = true;
                }                                
            }
            if (!sourceUpdated)
                switch (level.Context)
                {
                    default:
                        source = GetTextureSource(65, 5, 60, 52); // Original Krabby Patty
                        break;
                    case LevelContext.BIKINI:
                        source = GetTextureSource(64, 64, 64, 64);// Bikini Flag
                        break;
                    case LevelContext.BEACH:
                        source = GetTextureSource(128, 0, 64, 64);// Beach Flag
                        break;
                    case LevelContext.FIELDS:
                        source = GetTextureSource(192, 0, 64, 64);// Jelly Flag
                        break;
                    case LevelContext.KELP:
                        source = GetTextureSource(128, 64, 64, 64);// Kelp Flag
                        break;
                    case LevelContext.CAVES:
                        source = GetTextureSource(192, 64, 64, 64);// Caves Flag
                        break;
                }
            sprite = Sprite.Create(markerSheet, source, new Vector2(.5f, .5f));
                       
        }
        marker.GetComponent<SpriteRenderer>().sprite = sprite;
        var point = ConvertToMapRelativePosition(position);
        marker.localPosition = new Vector3(point.x, marker.localPosition.y, point.y);
        marker.gameObject.SetActive(true);
        return marker.gameObject;
    }

    public Vector2 ConvertToMapRelativePosition(SPoint MapPosition)
    {
        double Xpercentage = MapPosition.X / 1024.0,
                Ypercentage = MapPosition.Y / 1024;
        float pointX = TopLeft.x - (float)(Xpercentage * (TopLeft.x + Mathf.Abs(BotRight.x)));
        float pointY = TopLeft.y + (float)(Ypercentage * (Mathf.Abs(TopLeft.y) + BotRight.y));
        return new Vector2(pointX - .35f, pointY + .25f);
    }

    public void ChangeSelectedLevel(StinkyLevel level)
    {
        if (level == null) return;
        var source = mparser.Levels;
        var selection = source.Where(x => x.Key.LevelWorldName == level.LevelWorldName);
        if (selection.Count() > 0)
        {
            var point = ConvertToMapRelativePosition(selection.First().Value);
            var marker = CurrentLevelIndicator.transform;
            marker.localPosition = new Vector3(point.x, 0.15f, point.y);
            CurrentlySelected = level;
        }
        SetUIText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
