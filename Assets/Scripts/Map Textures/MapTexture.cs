using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    }

    public TileInfo[] tileInfos;
    public int testcolorx = 0;
    public int testcolory = 0;

    private void Awake() {
        Instance = this;
    }

    public void SaveTextureToFile(Texture2D textureToSave, string fileName) {
        var bytes = textureToSave.EncodeToPNG();
        var file = File.Open(Application.dataPath + "/" + fileName, FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }

    public void UpdateFeedbackMap() {
        return;
        for (var x = 0; x < feedbackMap_Texture.width; x++) {
            for (var y = 0; y < feedbackMap_Texture.height; y++) {
                Paint(new Coords(x, y), Color.clear);
            }
        }

        foreach (var item in ZombieManager.Instance.zombies) {
            // horde visuazilation
            /*Color pixelColor = mainMap_Texture.GetPixel(item.coords.x, item.coords.y);
            pixelColor.r = 1f;
            pixelColor.a += 0.05f;*/

            Paint(item.coords, Color.red);
        }

        Paint(Player.Instance.coords, Color.blue);
        //Paint(ClueManager.Instance.clueCoords, Color.cyan);
        RefreshTexture();
    }

    public void UpdateInteriorMap() {
        for (var x = 0; x < interiorMap_Texture.width; x++) {
            for (var y = 0; y < interiorMap_Texture.height; y++) {
                interiorMap_Texture.SetPixel(x, y, Color.black);
            }
        }

        foreach (var coords in TileSet.current.tiles.Keys) {
            interiorMap_Texture.SetPixel(coords.x, coords.y, Color.green);
        }

        interiorMap_Texture.Apply();
        interiorMap_Image.sprite = Sprite.Create(interiorMap_Texture, new Rect(0, 0, mainMap_Texture.width, mainMap_Texture.height), Vector2.one * 0.5f);
    }

    #region read map from texture
    public void CreateMapFromTexture() {
        TileSet.map = new TileSet();
        TileSet.SetCurrent(TileSet.map);

        var w = mainMap_Texture.width;
        var h = mainMap_Texture.height;

        TileSet.map.width = w;
        TileSet.map.height = h;

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                var coords = new Coords(x, y);

                var pixelColor = mainMap_Texture.GetPixel(x, y);

                if (pixelColor == Color.black) {
                    continue;
                }

                for (var i = 0; i < tileInfos.Length; i++) {

                    // check for color match

                    var tileColor = tileInfos[i].color;

                    if (
                        ColorUtility.ToHtmlStringRGB(tileColor) == ColorUtility.ToHtmlStringRGB(pixelColor)
                        ) {
                        var tileInfo = tileInfos[i];

                        var newTile = Tile.create(coords, tileInfo.name);

                        // get tile type from color
                        TileSet.map.Add(coords, newTile);

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
