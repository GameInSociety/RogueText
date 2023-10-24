using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interior {

    public Coords coords;

    public static Interior Current;

    public TileSet tileSet;

    public static float chanceLockedInterior = 0f;
    //public static float chanceClosedDoor = 0.2f;
    public static float chanceCreateRoom = 1f;
    //public static float chanceCreateRoom = 0.65f;
    public static float chanceHallwayTurn = 0.5f;

    // tmp list for giving objets ( doors ... ) adjectives, because they_re not meant repeat them selves
    //List<Adjective> adjectives;

    public static bool InsideInterior() {
        return Current != null;
    }

    #region enter / exit
    public void Enter() {
        TileSet.map.playerCoords = Player.Instance.coords;

        Current = this;

        TileSet.SetCurrent(tileSet);

        MapTexture.Instance.UpdateInteriorMap();

        Player.Instance.coords = tileSet.Center;
        Player.Instance.Move(Cardinal.None);
        //DisplayDescription.Instance.UpdateDescription();

        TimeManager.Instance.ChangeMovesPerHour(4);

    }

    public void Exit() {
        Player.Instance.coords = TileSet.map.playerCoords;

        Current = null;

        TileSet.SetCurrent(TileSet.map);

        Player.Instance.Move(Cardinal.None);

        TimeManager.Instance.ChangeMovesPerHour(10);
    }
    #endregion

    public void Genererate(Item item) {

        /// create tile set 
		tileSet = new TileSet();
        tileSet.width = TileSet.map.width;
        tileSet.height = TileSet.map.height;

        var rooms = item.properties.FindAll(x => x.type == "room");

        /*foreach (var room in rooms)
        {
            Debug.Log("room : " + room.name);
        }*/

        // create room types
        var tileNames = new List<string>
        {
            "bathroom",
            "bedroom",
            "children's room",
            "kitchen",
            "toilets"
        };

        // create hallway
        var hallway_Coords = tileSet.Center;
        var hallway_Dir = new Coords(0, 1);
        var a = 0;

        while (tileNames.Count > 0) {

            // add new hallway tile
            var newHallwayTile = Tile.create(hallway_Coords, "hallway");

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

                var dirItem = newHallwayTile.addItem("back");
                var tileDoor = dirItem.addItem("door");
                _ = tileDoor.addProperty("direction / back");
            }

            // check if room appears
            if (Random.value < chanceCreateRoom) {

                var side = new Coords(hallway_Dir.x, hallway_Dir.y);
                side.Turn();

                var coords = newHallwayTile.coords + side
                    ;

                if (tileSet.tiles.ContainsKey(coords)) {
                    continue;
                }

                var tileName = tileNames[Random.Range(0, tileNames.Count)];
                var newRoomTile = Tile.create(coords, tileName);

                _ = tileNames.Remove(tileName);

                tileSet.Add(coords, newRoomTile);
            }

            hallway_Coords += hallway_Dir;

            if (Random.value < chanceHallwayTurn) {
                hallway_Dir.Turn();
            }

            ++a;

        }

    }
}
