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
        var hallway_Dir = new Coords(0, 1);
        var a = 0;

        string hallwaySpec = Spec.GetCat("shape").GetRandomSpec();

        while (tileNames.Count > 0) {

            // add new hallway tile
            var newHallwayTile = Tile.Create(new Tile.Info(hallway_Coords, tileSetId), "hallway",hallwaySpec );

            if (tileSet.tiles.ContainsKey(hallway_Coords)) {
                hallway_Coords += hallway_Dir;
                ++a;
                continue;
            }

            tileSet.Add(hallway_Coords, newHallwayTile);

            // set entrance door
            if (a == 0) {
                /*Item doorItem = Item.CreateInTile(newHallwayTile, "door");
                // stating that it goes south so it displays "behind you" when entering the interior
                // pas ouf, ça changer avec la description des propriétes
                doorItem.CreateProperty("dir / direction / south");*/

                var orientation = Humanoid.Orientation.back;
                var newDoor = newHallwayTile.CreateChildItem("door");
                newDoor.SetProp($"orientation / description:on the {orientation} / search:{orientation} / after word:yes");
                newDoor.SetProp("entrance / description:entrance");
                newDoor.SetProp("definite");
            }

            // check if room appears
            if (Random.value < 0.9f) {

                var side = new Coords(hallway_Dir.x, hallway_Dir.y);
                side.Turn();

                var coords = newHallwayTile.coords + side;

                if (tileSet.tiles.ContainsKey(coords))
                    continue;

                int rnd = Random.Range(0, tileNames.Count);
                var tileName = tileNames[rnd];
                var newRoomTile = Tile.Create(new Tile.Info(coords, tileSetId), tileName);
                tileNames.RemoveAt(rnd);

                tileSet.Add(coords, newRoomTile);
            }

            hallway_Coords += hallway_Dir;

            if (Random.value < 0.5f)
                hallway_Dir.Turn();

            ++a;

        }

    }
}
