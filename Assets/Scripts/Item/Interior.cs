using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public static class Interior {

    #region enter / exit
    public static void Enter(Item item) {

        if (!item.HasProp("tilesetId")) {
            int id = TileSet.tileSets.Count;
            item.SetProp($"tilesetId / value:{id}");
            // Create tile set
            InitTileSet(item);
        }

        int tilesetId = item.GetProp("tilesetId").GetNumValue();

        TileSet.world.startCoords = Player.Instance.coords;
        TileSet.ChangeTileSet(tilesetId);
    }
    public static void Exit() {
        TileSet.ChangeTileSet(TileSet.world.id);
    }
    #endregion

    static void InitTileSet(Item item) {

        /// Create tile set 
		TileSet tileSet = new TileSet();
        tileSet.width = TileSet.world.width;
        tileSet.height = TileSet.world.height;
        tileSet.startCoords = tileSet.Center;
        tileSet.timeScale = 4;
        int tileSetId = item.GetProp("tilesetId").GetNumValue();
        tileSet.id = tileSetId;

        TileSet.tileSets.Add(tileSet);

        // Create room types
        var rooms = item.GetProp("rooms");
        var tileNames = rooms.GetPart("tiles").content.Split('\n').ToList();
        // Create hallway
        var hallway_Coords = tileSet.Center;
        string hallwaySpec = Spec.GetCat("shape").GetRandomSpec();

        int b = 0;
        while (tileNames.Count > 0) {

            // add new hallway tile
            var newHallwayTile = Tile.Create(new Tile.Info(hallway_Coords, tileSetId), "hallway",hallwaySpec );

            tileSet.Add(hallway_Coords, newHallwayTile);

            // set entrance door
            if (b ==  0) {
                var entrance = newHallwayTile.CreateChildItem("entrance");
            }

            for (int x = -1; x < 2; x += 2) {
                var newRoomCoords = newHallwayTile.coords + new Coords(x, 0);

                int rnd = Random.Range(0, tileNames.Count);
                var tileName = tileNames[rnd];
                var newRoomTile = Tile.Create(new Tile.Info(newRoomCoords, tileSetId), tileName);
                tileNames.RemoveAt(rnd);
                tileSet.Add(newRoomCoords, newRoomTile);
                if (tileNames.Count == 0) {
                    break;
                }
            }
            hallway_Coords += new Coords(0, 1);
            if ( b >= 100) {
                Debug.LogError($"infinite loop break on interor");
                break;
            }
        }

    }
}
