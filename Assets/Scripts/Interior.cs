using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interior {

	public Coords coords;

    public static Interior GetCurrent;

	public static Dictionary<Coords, Interior> interiors= new Dictionary<Coords, Interior>();

    public TileSet tileSet;

    public static float chanceLockedInterior = 0f;
    public static float chanceEnclosedRoom = 1f;
    //public static float chanceClosedDoor = 0.2f;
    public static float chanceCreateRoom = 1f;
    //public static float chanceCreateRoom = 0.65f;
    public static float chanceHallwayTurn = 0.5f;

    // tmp list for giving objets ( doors ... ) adjectives, because they_re not meant repeat them selves
    //List<Adjective> adjectives;

    public static bool InsideInterior()
    {
        return GetCurrent != null;
    }

    public static void NewInterior ( Tile tile)
    {
        Interior newInterior = new Interior();

        newInterior.coords = tile.coords;
        interiors.Add(tile.coords, newInterior);

        //Debug.Log("addid interior of tile : " + tile.GetName() + " at " + tile.coords.ToString());
    }

	public static Interior Get (Coords coords)
	{
		return interiors[coords];
	}

    public static void DescribeExterior()
    {
        Cardinal dir = Cardinal.East;

        if (Player.Instance.coords.x < 0)
        {
            dir = Cardinal.West;
        }

        Coords tCoords = TileSet.map.playerCoords + (Coords)dir;

        Tile tile = TileSet.map.GetTile(tCoords);

        if (tile == null)
        {
            TextManager.WritePhrase("interior_exteriorDescription_blocked");
        }
        else
        {
            TextManager.WritePhrase("/interior_exteriorDescription_visible/" + tile.GetDescription());
        }

    }

    #region enter / exit
    public void Enter()
    {
        TileSet.map.playerCoords = Player.Instance.coords;

        GetCurrent = this;

        if (tileSet == null)
        {
            Genererate();
        }

        TileSet.SetCurrent(tileSet);

        MapTexture.Instance.UpdateInteriorMap();

        Player.Instance.coords = tileSet.Center;
        Player.Instance.Move(Cardinal.None);
        //DisplayDescription.Instance.UpdateDescription();

        TimeManager.GetInstance().ChangeMovesPerHour(4);

    }

    public void ExitByWindow()
    {
        //Coords tCoords = TileSet.map.playerCoords + (Coords)Player.Instance.direction;
        Coords tCoords = TileSet.map.playerCoords;

        Tile tile = TileSet.map.GetTile(tCoords);

        if ( tile!= null)
        {
            Player.Instance.coords = tCoords;
            Exit();
        }
        else
        {
            TextManager.WritePhrase("interior_getout_blocked");
        }
        
    }

    public void ExitByDoor()
    {
        Player.Instance.coords = TileSet.map.playerCoords;

        Exit();
    }

    void Exit()
    {
        Interior.GetCurrent = null;

        TileSet.SetCurrent(TileSet.map);

        Player.Instance.Move(Cardinal.None);

        TimeManager.GetInstance().ChangeMovesPerHour(10);

    }
    #endregion

    public void Genererate() {

        /// create tile set 
		tileSet = new TileSet();
        tileSet.width = TileSet.map.width;
        tileSet.height = TileSet.map.height;

        // create room types
        List<string> tileNames = new List<string>
        {
            "bathroom",
            "bedroom",
            "children's room",
            "kitchen",
            "toilets"
        };

        // create hallway
        Coords hallway_Coords = tileSet.Center;
        Coords hallway_Dir = new Coords(0,1);
        int a = 0;

		while ( tileNames.Count > 0 ) {

            // add new hallway tile
            Tile newHallwayTile = ItemManager.Instance.CreateTile(hallway_Coords, "hallway");

            if (tileSet.tiles.ContainsKey(hallway_Coords))
            {
                hallway_Coords += hallway_Dir;
                ++a;
                continue;
            }

			tileSet.Add (hallway_Coords, newHallwayTile);

            // set entrance door
            if (a == 0)
            {
                Item doorItem = ItemManager.Instance.CreateInTile(newHallwayTile, "door");
                doorItem.CreateProperty("type / entrance");
                // stating that it goes south so it displays "behind you" when entering the interior
                // pas ouf, ça changer avec la description des propriétes
                doorItem.CreateProperty("dir / direction / south");
            }

            // check if room appears
            if ( Random.value < chanceCreateRoom ) {

                Coords side = new Coords(hallway_Dir.x, hallway_Dir.y);
                side.Turn();

                Coords coords = newHallwayTile.coords + side
                    ;

                if (tileSet.tiles.ContainsKey(coords))
                {
                    continue;
                }

				string tileName = tileNames [Random.Range (0, tileNames.Count)];
                Tile newRoomTile = ItemManager.Instance.CreateTile(coords, tileName);

                tileNames.Remove (tileName);

                if (Random.value < chanceEnclosedRoom)
                    newRoomTile.enclosed = true;

                tileSet.Add ( coords, newRoomTile );
			}

            hallway_Coords += hallway_Dir;
            
            if ( Random.value < chanceHallwayTurn)
            {
                hallway_Dir.Turn();
            }

            ++a;

        }

        // ADDING DOORS
        AddDoors();

	}

    void AddDoors()
    {
        // reseting adjectives

        foreach (var tile in tileSet.tiles.Values)
        {
            AddDoors(tile);
        }
    }
    void AddDoors(Tile tile)
    {
        Cardinal[] surr = new Cardinal[4] {
                        Cardinal.North, Cardinal.West, Cardinal.South, Cardinal.East
                    };

        List<Adjective> adjectives = Adjective.GetAll("objet");

        foreach (var dir in surr)
        {
            Coords c = tile.coords + (Coords)dir;
            Tile adjacentTile = tileSet.GetTile(c);

            string currentDoorDirection = "";
            string adjacentDoorDirection = "";

            if (adjacentTile != null && adjacentTile.enclosed)
            {
                switch (dir)
                {
                    case Cardinal.North:
                        currentDoorDirection = "north";
                        adjacentDoorDirection = "south";
                        break;
                    case Cardinal.East:
                        currentDoorDirection = "east";
                        adjacentDoorDirection = "west";
                        break;
                    case Cardinal.South:
                        currentDoorDirection = "south";
                        adjacentDoorDirection = "north";
                        break;
                    case Cardinal.West:
                        currentDoorDirection = "west";
                        adjacentDoorDirection = "east";
                        break;
                    case Cardinal.None:
                        break;
                    default:
                        break;
                }

                // adjectives, on list so two object don't have the same ajective in a same tile
                // diffentiate them on input
                if (adjectives.Count == 0)
                {
                    adjectives = Adjective.GetAll("objet");
                }
                Adjective adjective = adjectives[Random.Range(0, adjectives.Count)];
                adjectives.Remove(adjective);

                // current tile door
                if (!tile.HasDoor(currentDoorDirection))
                {
                    Item doorItem = ItemManager.Instance.CreateInTile(tile, "door");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.CreateProperty("dir / direction / " + currentDoorDirection);
                }

                // adjacent tile door
                if (!adjacentTile.HasDoor(adjacentDoorDirection))
                {
                    Item doorItem = ItemManager.Instance.CreateInTile(adjacentTile, "door");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.CreateProperty("dir / direction / " + adjacentDoorDirection);
                }

                //Debug.Log("door name : " + currentTileDoorItemName + " adjacent door name : " + adjTileDoorItemName);
            }

        }
    }
}
