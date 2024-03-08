using System.Linq;
using UnityEngine;

public static class Interior {

    public static Item currentInterior;

    #region enter / exit
    public static void Enter(Item item) {
        Debug.LogError("enter here");

        /*currentInterior = item;
        TileSet.world.startCoords = Player.Instance.coords;
        TileSet.ChangeTileSet(GetTileSet(item).id);*/
    }
    public static void Exit() {
        Debug.LogError("exit here");
        /*currentInterior = null;
        TileSet.ChangeTileSet(TileSet.world.id);*/
    }
    #endregion

    public static TileSet InitTileSet(Item item, int id) {
        /// Create tile set 
		TileSet tileSet = new TileSet();
        tileSet.width = TileSet.world.width;
        tileSet.height = TileSet.world.height;

        // Create room types
        var rooms = item.GetProp("rooms");

        if ( rooms == null) {
            foreach (var prop in item.props) {
                Debug.Log($"prop : {prop.name}");
            }

                Debug.LogError($"item : {item.debug_name} has no prop ROOMS");
            return null;
        }
        var tileNames = rooms.GetPart("tiles").content.Split('\n').ToList();
        // Create hallway
        var startCoords = item.GetProp("start coords");
        var hallway_Coords = Coords.PropToCoords(startCoords);

        int b = 0;
        while (tileNames.Count > 0) {

            // add new hallway tile
            var newHallwayTile = Tile.Create(new Tile.Info(hallway_Coords, id), "hallway" );

            tileSet.Add(hallway_Coords, newHallwayTile);

            // set entrance door
            if (b ==  0) {
                var entrance = newHallwayTile.CreateChildItem("entrance");
            }

            for (int x = -1; x < 2; x += 2) {
                var newRoomCoords = newHallwayTile.coords + new Coords(x, 0);

                int rnd = Random.Range(0, tileNames.Count);
                var tileName = tileNames[rnd];
                var newRoomTile = Tile.Create(new Tile.Info(newRoomCoords, id), tileName);
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

        return tileSet;

    }
}
