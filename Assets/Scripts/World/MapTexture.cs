using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MapTexture : MonoBehaviour {

    public static MapTexture Instance;

    public Image mainMap_Image;
    public Texture2D mainMap_Texture;

    public Image feedbackMap_Image;
    public Texture2D feedbackMap_Texture;

    public Image interiorMap_Image;
    public Texture2D interiorMap_Texture;

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

    public void UpdateFeedbackMap() {
        for (var x = 0; x < feedbackMap_Texture.width; x++) {
            for (var y = 0; y < feedbackMap_Texture.height; y++) {
                Paint(new Coords(x, y), Color.clear);
            }
        }

        Paint(Player.Instance.coords, Color.blue);
        RefreshTexture();
    }

    public void UpdateInteriorMap() {
        for (var x = 0; x < interiorMap_Texture.width; x++) {
            for (var y = 0; y < interiorMap_Texture.height; y++) {
                interiorMap_Texture.SetPixel(x, y, Color.black);
            }
        }

        foreach (var coords in TileSet.GetCurrent.tiles.Keys) {
            TileInfo t = System.Array.Find(tileInfos, x => x.name == TileSet.GetCurrent.GetTile(coords).debug_name);
            interiorMap_Texture.SetPixel(coords.x, coords.y, t.color);

        }

        interiorMap_Texture.Apply();
        interiorMap_Image.sprite = Sprite.Create(interiorMap_Texture, new Rect(0, 0, mainMap_Texture.width, mainMap_Texture.height), Vector2.one * 0.5f);
    }

    #region read map from texture
    public void CreateMapFromTexture() {
        TileSet.tileSets.Add(new TileSet());

        var w = mainMap_Texture.width;
        var h = mainMap_Texture.height;

        TileSet.world.width = w;
        TileSet.world.height = h;

        var props = new List<string>();
        for (int i = 0; i < tileInfos.Length; i++) {
            var data = ItemData.GetItemData(tileInfos[i].name);
            var dif = data.properties.Find(x => x.name == "dif");
            if (dif != null) {
                props.Add(dif.GetDescription());
            }
        }

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                var coords = new Coords(x, y);

                var pixelColor = mainMap_Texture.GetPixel(x, y);

                // void
                if (pixelColor == Color.black)
                    continue;

                for (var i = 0; i < tileInfos.Length; i++) {

                    // check for color match

                    var tileColor = tileInfos[i].color;

                    if (
                        ColorUtility.ToHtmlStringRGB(tileColor) == ColorUtility.ToHtmlStringRGB(pixelColor)
                        ) {
                        var tileInfo = tileInfos[i];

                        string prop_str = props[i];
                        var newTile = Tile.Create(new Tile.Info(coords, 0), tileInfo.name, prop_str);
                        if( !string.IsNullOrEmpty(tileInfo.prop.name)) {
                            Debug.Log($"adding prop : {tileInfo.prop.name} to tile {newTile.debug_name}");
                            newTile.AddProp(tileInfo.prop);
                        }

                        // get tile type from color
                        TileSet.world.Add(coords, newTile);

                        /*if (newTile.HasProperty("interior"))
                        {
                            Interior.NewInterior(newTile);
                        }*/
                        break;
                    }
                }

            }
        }

    }
    #endregion


    #region texture
    public void ResetTexture() {
        var colors = new Color[scale * scale];
        for (var i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }
        feedbackMap_Texture.SetPixels(colors);
    }
    public void RefreshTexture() {
        feedbackMap_Texture.Apply();
        feedbackMap_Image.sprite = Sprite.Create(feedbackMap_Texture, new Rect(0, 0, mainMap_Texture.width, mainMap_Texture.height), Vector2.one * 0.5f);
    }
    public void Paint(Coords coords, Color c) {
        feedbackMap_Texture.SetPixel(coords.x, coords.y, c);
    }
    #endregion
}
