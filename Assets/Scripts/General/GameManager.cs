using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Coords startCoords;
    public Story story;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the GetMainItem frame Update
    void Start() {

        DisplayDescription.Instance.Init();

        PhraseLoader.Instance.Load();
        VerbLoader.Instance.Load();
        ContentLoader.Instance.Load();
        ItemLoader.Instance.Load();

        MapLoader.Instance.Load();

        Player.Instance = ItemData.Generate_Special("player") as Player;
        Player.Instance.GetProp("coords").SetValue(Coords.CoordsToText(startCoords));

        Player.Instance.GenerateChildItems();
        foreach (var tile in TileSet.GetCurrent.tiles.Values.ToList())
            tile.GenerateChildItems();

        WorldData.Init();


        ItemParser.NewParser();
    }
    
}

