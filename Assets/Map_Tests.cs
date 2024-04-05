using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_Tests : MonoBehaviour
{
    public int scale = 10;

    public List<TestTile> tiles = new List<TestTile>();
    public TestTile prefab;

    public float tileSize = 100f;
    public float buffer = 2f;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("Delay", 0f);
    }

    void Delay() {
        for (int x = 0; x < TileSet.GetCurrent.width; x++) {
            for (int y = 0; y < TileSet.GetCurrent.height; y++) {
                Vector2 v = new Vector2(x * tileSize, y * tileSize);
                var testTile = Instantiate(prefab, transform);
                testTile.rectTransform.anchoredPosition = v;
                testTile.rectTransform.sizeDelta = Vector2.one * (tileSize - buffer);

                var tile = TileSet.GetCurrent.GetTile(new Coords(x, y));

                var ti = MapLoader.Instance.tileInfos.Find(x => x.value == tile.debug_name);

                var s = $"{tile.debug_name}\n({tile.GetProp("visibility")?.GetNumValue()})";
                testTile.Display(s, ti.color);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < TileSet.GetCurrent.width; x++) {
            for (int y = 0; y < TileSet.GetCurrent.height; y++) {
                Vector2 v = new Vector2(x * tileSize, y * tileSize);
                var testTile = Instantiate(prefab, transform);
                testTile.rectTransform.anchoredPosition = v;
                testTile.rectTransform.sizeDelta = Vector2.one * (tileSize - buffer);

                var tile = TileSet.GetCurrent.GetTile(new Coords(x, y));

                var ti = MapLoader.Instance.tileInfos.Find(x => x.value == tile.debug_name);

                var s = $"{tile.debug_name}\n({tile.GetProp("visibility")?.GetNumValue()})";
                testTile.Display(s, ti.color);

            }
        }

    }
}
