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

    // tmp list for giving objets ( doors ... ) adjectives, because they_re not meant to repeat them selves
    //List<Adjective> adjectives;

    public static bool InsideInterior()
    {
        return GetCurrent != null;
    }

    public static void NewInterior ( Tile tile)
    {
        tile.SetType(tile.type);

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
        Direction dir = Direction.East;

        if (Player.Instance.coords.x < 0)
        {
            dir = Direction.West;
        }

        Coords tCoords = TileSet.map.playerCoords + (Coords)dir;

        Tile tile = TileSet.map.GetTile(tCoords);

        if (tile == null)
        {
            Phrase.Write("interior_exteriorDescription_blocked");
        }
        else
        {
            Phrase.Write("/interior_exteriorDescription_visible/" + tile.GetDescription());
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
        Player.Instance.Move(Direction.None);
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
            Phrase.Write("interior_getout_blocked");
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

        Player.Instance.Move(Direction.None);

        TimeManager.GetInstance().ChangeMovesPerHour(10);

    }
    #endregion

    public void Genererate() {

        /// create tile set 
		tileSet = new TileSet();
        tileSet.width = TileSet.map.width;
        tileSet.height = TileSet.map.height;

        // create room types
        List<Tile.Type> roomTypes = new List<Tile.Type> ();
		Tile.Type type = Tile.Type.LivingRoom;

        /// debug interior : create fix list of rooms
        roomTypes.Add(Tile.Type.Bathroom);
        roomTypes.Add(Tile.Type.Bedroom);
        roomTypes.Add(Tile.Type.ChildBedroom);
        roomTypes.Add(Tile.Type.Kitchen);
        roomTypes.Add(Tile.Type.Toilet);

        // create hallway
        Coords hallway_Coords = tileSet.Center;
        Coords hallway_Dir = new Coords(0,1);
        int a = 0;

		while ( roomTypes.Count > 0 ) {

            // add new hallway tile
			Tile newHallwayTile = new Tile (hallway_Coords);
			newHallwayTile.SetType (Tile.Type.Hallway);

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
                Item doorItem = Item.CreateNew("porte");
                doorItem.AddProperty("entrance", "true");
                // il n'y a plus réellement de porte dehors en fait non ?
                //doorItem.word.SetAdjective(TileSet.map.GetTile(TileSet.map.playerCoords).items[0].word.GetAdjective);
                doorItem.AddProperty("direction", "to south");

                newHallwayTile.AddItem(doorItem);
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

				Tile newRoomTile = new Tile(coords);
				Tile.Type roomType = roomTypes [Random.Range (0, roomTypes.Count)];
				newRoomTile.SetType (roomType);

				roomTypes.Remove (roomType);

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

        InitStoryTiles();

        // ADDING DOORS
        AddDoors();

	}

    void InitStoryTiles()
    {
        if (coords == ClueManager.Instance.bunkerCoords)
        {
            //Debug.Log("creating letter");
            int i = Random.Range(1, tileSet.tiles.Count);
            Item letter_item = Item.GetDataItem("lettre");
            tileSet.tiles.Values.ElementAt(i).items.Add(letter_item);
        }

        /*if (coords == ClueManager.Instance.bunkerCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item bunkerItem = Item.FindByName("tableau");
            tileSet.tiles.Values.ElementAt(i).items.Add(bunkerItem);
        }

        if (coords == ClueManager.Instance.clueCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item clueItem = Item.FindByName("radio");
            tileSet.tiles.Values.ElementAt(i).items.Add(clueItem);
        }*/

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
        Direction[] surr = new Direction[4] {
                        Direction.North, Direction.West, Direction.South, Direction.East
                    };

        List<Adjective> adjectives = Adjective.GetAll("objet");

        foreach (var dir in surr)
        {
            Coords c = tile.coords + (Coords)dir;
            Tile adjTile = tileSet.GetTile(c);

            string currentDoorDirection = "";
            string adjacentDoorDirection = "";

            if (adjTile != null && adjTile.enclosed)
            {
                switch (dir)
                {
                    case Direction.North:
                        currentDoorDirection = "to north";
                        adjacentDoorDirection = "to south";
                        break;
                    case Direction.East:
                        currentDoorDirection = "to east";
                        adjacentDoorDirection = "to west";
                        break;
                    case Direction.South:
                        currentDoorDirection = "to south";
                        adjacentDoorDirection = "to north";
                        break;
                    case Direction.West:
                        currentDoorDirection = "to west";
                        adjacentDoorDirection = "to east";
                        break;
                    case Direction.None:
                        break;
                    default:
                        break;
                }

                // adjectives
                if (adjectives.Count == 0)
                {
                    adjectives = Adjective.GetAll("objet");
                }
                Adjective adjective = adjectives[Random.Range(0, adjectives.Count)];
                adjectives.Remove(adjective);

                // current tile door
                if (tile.items.Find(x => x.word.text == "porte" && x.GetProperty("direction").GetContent() == currentDoorDirection) == null)
                {
                    Item doorItem = Item.CreateNew("porte");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.AddProperty("direction", currentDoorDirection);
                    tile.AddItem(doorItem);
                }

                // adjacent tile door
                if (adjTile.items.Find(x => x.word.text == "porte" && x.GetProperty("direction").GetContent() == adjacentDoorDirection) == null )
                {
                    Item doorItem = Item.CreateNew("porte");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.AddProperty("direction", adjacentDoorDirection);
                    adjTile.AddItem(doorItem);
                }

                //Debug.Log("door name : " + currentTileDoorItemName + " adjacent door name : " + adjTileDoorItemName);
            }

        }
    }
}
