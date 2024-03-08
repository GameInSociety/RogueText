using UnityEngine;
using UnityEngine.UI;

public class MapTexture : MonoBehaviour {

    public static MapTexture Instance;

    public Image mainMap_Image;
    public Texture2D mainMap_Texture;
    public Image feedbackMap_Image;
    public Texture2D feedbackMap_Texture;

    public int range = 1;
    private int scale = 0;

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
    }

    public void UpdateFeedbackMap() {
        feedbackMap_Image.sprite = Sprite.Create(feedbackMap_Texture, new Rect(0, 0, mainMap_Texture.width, mainMap_Texture.height), Vector2.one * 0.5f);
        for (var x = 0; x < feedbackMap_Texture.width; x++) {
            for (var y = 0; y < feedbackMap_Texture.height; y++)
                feedbackMap_Texture.SetPixel(x, y, Color.clear);
        }

        var c = Coords.PropToCoords(Player.Instance.GetProp("coords"));
        feedbackMap_Texture.SetPixel(c.x,  c.y, Color.blue);
        feedbackMap_Texture.Apply();
    }

}
