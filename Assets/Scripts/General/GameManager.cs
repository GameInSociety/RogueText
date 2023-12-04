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

        PhraseLoader.Instance.Load();

        SpecLoader.Instance.Load();
        VerbLoader.Instance.Load();
        ItemLoader.Instance.Load();

        WorldData.Init();
        TimeManager.Init();
        WorldEvent.Init();

        MapTexture.Instance.CreateMapFromTexture();

        Player.Instance = ItemData.Generate_Special("player") as Player;

        Tile.SetCurrent(TileSet.GetCurrent.GetTile(startCoords));

        Player.Instance.Move(startCoords);

        ItemParser.NewParser();

        //ZombieManager.Instance.Init();


    }
}
