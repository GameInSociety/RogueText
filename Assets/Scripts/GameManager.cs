using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Coords startCoords;

    public Story story;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhraseLoader.Instance.Load();

        VerbLoader.Instance.Load();
        ItemLoader.Instance.Load();

        MapTexture.Instance.CreateMapFromTexture();

        Player.Instance = Item.Generate_Special("player") as Player;

        Tile.SetCurrent(TileSet.current.GetTile(startCoords));

        Player.Instance.Move(startCoords);

        ZombieManager.Instance.Init();

        Player.Instance.Move(Cardinal.None);


    }
}
