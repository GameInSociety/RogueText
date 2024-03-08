using System.Collections;
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

        VerbLoader.Instance.Load();
        ContentLoader.Instance.Load();
        ItemLoader.Instance.Load();
        MapLoader.Instance.Load();

        //MapTexture.Instance.CreateMapFromTexture();

        Player.Instance = ItemData.Generate_Special("player") as Player;

        MapTexture.Instance.DisplayMap(TileSet.GetTileSet(0));

        WorldData.Init();

        Player.Instance.GetProp("coords").SetValue(Coords.CoordsToText(startCoords));
        var startTile = TileSet.world.GetTile(Coords.PropToCoords(Player.Instance.GetProp("coords"), 0));
        WorldData.SetAbstractItem("current tile", startTile);

        var startItem = WorldData.globalItems[0];
        var startSequence = "triggerEvent(OnStart)";
        WorldAction startAction = new WorldAction(startItem, new Tile.Info(), startSequence);
        startAction.Call();

        ItemParser.NewParser();
    }
    
}

