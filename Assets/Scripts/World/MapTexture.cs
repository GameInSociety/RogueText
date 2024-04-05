using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapTexture : MonoBehaviour {

    public static MapTexture Instance;

    public Image mainMap_Image;
    public Texture2D mainMap_Texture;
    public Image feedbackMap_Image;
    public Texture2D feedbackMap_Texture;

    public MapFeedback feedback_Prefab;
    public List<MapFeedback> feedback_Pool = new List<MapFeedback>();
    int feedback_CurrentIndex;
    public float feedback_decal = 15f;

    public int range = 1;
    private int scale = 0;
    public float mult = 130f;

    [System.Serializable]
    public struct TileInfo {
        public string name;
        public Color color;
        public Property prop;
    }

    public TileInfo[] tileInfos;
    public int testcolorx = 0;
    public int testcolory = 0;

    private void Awake() {
        Instance = this;
    }

    public void DisplayMap() {
        DisplayMap(TileSet.GetCurrent);
    }
    public void DisplayMap(TileSet tileSet) {
        mainMap_Texture = new Texture2D(tileSet.width, tileSet.height);
        mainMap_Texture.filterMode = FilterMode.Point;
        mainMap_Image.sprite = Sprite.Create(mainMap_Texture, new Rect(0, 0, tileSet.width, tileSet.height), Vector2.one * 0.5f);
        for (var x = 0; x < tileSet.width; x++) {
            for (var y = 0; y < tileSet.height; y++) {
                var tile = tileSet.GetTile(new Coords(x, y));
                if (tile == null) {
                    // void
                    mainMap_Texture.SetPixel(x, y, Color.black);
                    continue;
                }
                var name = tile.debug_name;
                var tileInfo = System.Array.Find(tileInfos, t => t.name == name);
                if ( string.IsNullOrEmpty(tileInfo.name)) {
                    Debug.LogError($"[MAP TEXTURE] : could't find a tile with name : {name}");
                    continue;
                }
                mainMap_Texture.SetPixel(x, y, tileInfo.color);
            }
        }
        mainMap_Texture.Apply();

        UpdateFeedbackMap();

        for (var x = 0;x < tileSet.width; x++) {
            for(var y = 0;y < tileSet.height; y++) {
                var c = new Coords(x, y);
                var tile = tileSet.GetTile(c);
                var tc = Coords.PropToCoords(tile.GetProp("coords"));
                DisplayFeedback(tc, $"{tile.debug_name}\n({tc.ToString()})", Color.white);
            }
        }
    }

    public void UpdateFeedbackMap() {

        feedback_CurrentIndex = 0;

        foreach (var item in feedback_Pool) {
            item.Hide();
        }

        var playerCoords = Coords.PropToCoords(Player.Instance.GetProp("coords"));
        DisplayFeedback(playerCoords, "player", Color.blue);

        foreach (var tile in TileSet.GetCurrent.tiles.Values.ToList()) {
            if ( tile.HasItem("undead")) {
                var undead = tile.GetItem("undead");
                var undeadCoords = Coords.PropToCoords(undead.GetProp("coords"));
                DisplayFeedback(undeadCoords, "undead", Color.green);
            }
        }

    }

    void DisplayFeedback(Coords coords, string title, Color color) {
        if ( feedback_CurrentIndex >= feedback_Pool.Count)
            feedback_Pool.Add(Instantiate(feedback_Prefab, transform));

        var mapFeedback = feedback_Pool[feedback_CurrentIndex];
        mapFeedback.Display(coords, title, color);

        float x = coords.x * mult + feedback_decal;
        float y = coords.y * mult + feedback_decal;
        Vector2 v = new Vector2(x, y);

        var sameCoordsFeedbacks = feedback_Pool.FindAll(x => x.coords == coords);
        for ( var i = 0; i < sameCoordsFeedbacks.Count; i++) {
            v.x += i * feedback_decal;
            sameCoordsFeedbacks[i].rectTransform.anchoredPosition = v;
        }

        feedback_CurrentIndex++;
    }

}
