using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Coords startCoords;
    public Story story;
    public List<DebugHistory> debug_HISTOIRE = new List<DebugHistory>();
    [System.Serializable]
    public class DebugHistory {
        public DebugHistory (List<ItemLink.ItemHistory> _k) {
            history = _k;
        }
        public List<ItemLink.ItemHistory> history;
    }

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
    }
    
}

