using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public Coords startCoords;

    public Story story;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the GetMainItem frame Update
    void Start() {

        SpecLoader.Instance.Load();
        VerbLoader.Instance.Load();
        ContentLoader.Instance.Load();
        ItemLoader.Instance.Load();

        MapTexture.Instance.CreateMapFromTexture();

        Player.Instance = ItemData.Generate_Special("player") as Player;

        WorldData.Init();

        Tile.SetCurrent(TileSet.GetCurrent.GetTile(startCoords));

        Player.Instance.coords = startCoords;
        Player.Instance.Move(startCoords);

        ItemParser.NewParser();

        //ZombieManager.Instance.Parse();


    }
}
