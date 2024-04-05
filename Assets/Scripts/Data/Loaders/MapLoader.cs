using System.Collections.Generic;
using UnityEngine;

public class MapLoader : DataDownloader
{
    public static MapLoader Instance;

    public int mapToLoad;

    public List<TileInfo> tileInfos = new List<TileInfo>();
    [System.Serializable]
    public class TileInfo {
        public string value;
        public string key;
        public Color color = Color.white;
    }

    private void Awake() {
        Instance = this;
    }

    public override void Load() {
        WorldEvent.worldEvents.Clear();
        PropertyDescription.propDescriptions.Clear();
        ContentLoader.groups.Clear();

        TileSet.tileSets.Clear();
        TileSet.tileSets.Add(new TileSet());

        base.Load();
    }

    public override void FinishLoading() {
        base.FinishLoading();
    }

    public override void GetCell(int row, List<string> cells) {
        base.GetCell(row, cells);
        for (int x = 0; x < cells.Count; x++) {
            // void
            if (string.IsNullOrEmpty(cells[x]) )
                continue;

            var returnIndex = cells[x].IndexOf('\n');
            bool HasItems = returnIndex > 0;
            var tileName = HasItems ? cells[x].Remove(returnIndex) : cells[x];

            var tileInfo = tileInfos.Find(x => x.key == tileName);
            if (tileInfo == null) {
                Debug.Log($"no tile info with name : {tileName}");
                continue;
            }

            int y = lineAmount - row - 1;
            var coords = new Coords(x, y);
            var newTile = Tile.Create(new Tile.Info(coords, 0), tileInfo.value);
            var cProp = newTile.AddProp(Coords.CoordsToProp(coords));
            TileSet.world.Add(coords, newTile);
            if (HasItems) {
                string[] itemList = cells[x].Remove(0, returnIndex + 1).Split('\n');
                foreach (string itemName in itemList) {
                    if ( itemName == "START") {
                        GameManager.Instance.startCoords = new Coords(x, y);
                        continue;
                    }
                    //newTile.CreateChildItem(itemName);
                }
            }
        }

    }
}
