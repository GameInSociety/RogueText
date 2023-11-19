using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class Interior : Item {

    public int tilesetId = -1;

    #region enter / exit
    public void Enter() {
        if ( tilesetId< 0 ) {
            // create tile set
            InitTileSet();
        }
        // save coords
        TileSet.world.playerCoords = Player.Instance.coords;

        TileSet.ChangeTileSet(tilesetId);
        Player.Instance.Move(Cardinal.None);

    }
    #endregion

    public void InitTileSet() {

        /// create tile set 
		TileSet tileSet = new TileSet();
        tileSet.width = TileSet.world.width;
        tileSet.height = TileSet.world.height;
        tileSet.playerCoords = tileSet.Center;
        tileSet.timeScale = 4;

        tilesetId = TileSet.tileSets.Count;
        TileSet.tileSets.Add(tileSet);

        // create room types
        var rooms = GetProp("rooms");
        var tileNames = rooms.getPart("tiles").text.Split('\n').ToList();
        foreach (var tileName in tileNames) {
            Debug.Log(tileName);
        }

        // create hallway
        var hallway_Coords = tileSet.Center;
        var hallway_Dir = new Coords(0, 1);
        var a = 0;

        string hallwaySpec = Spec.GetCat("color").GetRandomSpec();

        while (tileNames.Count > 0) {

            // add new hallway tile
            var newHallwayTile = Tile.create(hallway_Coords, "hallway",hallwaySpec );

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
                newDoor.setSpec($">on the {orientation}", orientation.ToString(), orientation.ToString());
                newDoor.setSpec("entrance");
                newDoor.AddProp("definite");
            }

            // check if room appears
            if (Random.value < 0.9f) {

                var side = new Coords(hallway_Dir.x, hallway_Dir.y);
                side.Turn();

                var coords = newHallwayTile.coords + side;

                if (tileSet.tiles.ContainsKey(coords))
                    continue;

                int rnd = Random.Range(0, tileNames.Count);
                var tileName = tileNames[rnd].Remove(0, 7);
                var newRoomTile = Tile.create(coords, tileName);
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
