using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour {

	public static Player Instance;

	public Direction direction;
    public Orientation currentOrientation;
    public List<Orientation> currentOrientations;

    // STATES
	public int health = 0;
    public int maxHealth = 10;

    /// <summary>
    /// STATs
    /// </summary>
    public Stats stats;

	// COORDS
	public Coords prevCoords = new Coords(-1,-1);
	public Coords coords = new Coords (-1,-1);

    public Coords startCoords;

    public Player()
    {

    }

    void Awake()
    {
        Instance = this;
    }

    public void Init () {

        // pick a random interior to start in from off
        /*int startInteriorID = Random.Range(0, Interior.interiors.Count);
        coords = Interior.interiors.Values.ElementAt(startInteriorID).coords;*/

        // debug start at same place all the time
        coords = startCoords;

        // equipement
        Equipment equipment = new Equipment();
        equipment.Init();

        // stats
        stats = new Stats();

		PlayerActionManager.onPlayerAction+= HandleOnAction;

        Move(Direction.None);
    }

    void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.Move:
			Move ((Direction)PlayerAction.GetCurrent.GetValue(0));
			break;
		case PlayerAction.Type.MoveRel:
			Move (GetDirection((Orientation)PlayerAction.GetCurrent.GetValue(0)));
			break;
            case PlayerAction.Type.MoveToTargetItem:
                MoveToTargetItem();
                break;
            case PlayerAction.Type.Look:
			break;
            case PlayerAction.Type.UseDoor:
                UseDoor();
                break;
		case PlayerAction.Type.Enter:
			EnterCurrentInterior ();
			break;
            case PlayerAction.Type.ExitByWindow:
            Interior.GetCurrent.ExitByWindow();
            break;
        case PlayerAction.Type.GoOut:
                GoOut();
			break;
		default:
			break;
		}
	}

    void UseDoor()
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        // check if locked
        if (targetItem.HasProperty("locked"))
        {
            if (targetItem.GetProperty("locked").GetContent() == "true")
            {
                Phrase.Write("door_locked");
                return;
            }
        }

        // check if has direction ( for interiors )
        if (!targetItem.HasProperty("entrance"))
        {
            switch (targetItem.GetProperty("direction").GetContent())
            {
                case "to north":
                    Move(Direction.North);
                    break;

                case "to south":
                    Move(Direction.South);
                    break;

                case "to east":
                    Move(Direction.East);
                    break;

                case "to west":
                    Move(Direction.West);
                    break;
                default:
                    break;
            }
        }
        else
        {
            EnterCurrentInterior();

            //Debug.LogError("door doesn't have param direction");
        }
    }

    void EnterCurrentInterior ()
	{
        if (Interior.GetCurrent == null)
        {
            Interior.Get(coords).Enter();
        }
        else
        {
            Interior.GetCurrent.ExitByDoor();
        }
    }

    void GoOut()
    {
        if (Interior.GetCurrent != null)
        {
            if ( Player.Instance.coords == Coords.Zero)
            {
                Interior.Get(coords).ExitByDoor();
            }
            else
            {
                Move(Player.Orientation.Back);
                //Phrase.Write("Vous n'êtes pas près de l'entrée");
            }
        }
        else
        {
            Move(Player.Orientation.Back);
        }
    }

    #region movement
    public void Move(Player.Orientation orientation)
    {
        Move(GetDirection(orientation));
    }

    public void Move ( Direction dir ) {

        // setting next coords
        Coords targetCoords = coords + (Coords)dir;

        // cancel if path blocked
        if (!CanMoveForward(targetCoords))
        {
            return;
        }

        StartCoroutine(MoveCoroutine(dir));

    }
    IEnumerator MoveCoroutine(Direction dir)
    {
        // first of all, advance time ( and items states etc... )
        TimeManager.GetInstance().AdvanceTime();

        // ?
        if (coords.x > 0)
        {
            prevCoords = coords;
        }

        // change current coords
        coords += (Coords)dir;

        // set preivous & current tile
        Tile.SetPrevious (Tile.GetCurrent);
        Tile.SetCurrent (TileSet.current.GetTile(coords));

        // generate tile items
        if (Tile.GetCurrent.visited == false)
        {
            Tile.GetCurrent.GenerateItems();
        }

        // because the game starts with no movement
        if (dir == Direction.None)
            dir = Direction.North;

        // set new direction
        direction = dir;


        DisplayDescription.Instance.UpdateDescription();

        MapTexture.Instance.UpdateFeedbackMap();

        Tile.GetCurrent.visited = true;

        yield return new WaitForEndOfFrame();

    }

    public void ClearDescription()
    {
        
    }

    public void MoveToTargetItem()
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        foreach (var tileGroup in TileGroupDescription.tileGroups)
        {
            if ( tileGroup.tile.tileItem.SameTypeAs(targetItem))
            {
                if ( InputInfo.GetCurrent.orientation != Orientation.None)
                {
                    currentOrientation = InputInfo.GetCurrent.orientation;

                    if (tileGroup.orientations.Contains(currentOrientation))
                    {
                        Direction dir = GetDirection(currentOrientation);
                        Move(dir);
                    }
                    else
                    {
                        Phrase.Write("movement_blocked");
                    }
                    return;
                }
                else
                {
                    currentOrientation = tileGroup.orientations[0];
                    Direction dir = GetDirection(currentOrientation);
                    Move(dir);
                    return;
                }
               
            }
        }
    }
    #endregion

    bool CanMoveForward (Coords c)
	{
		Tile targetTile = TileSet.current.GetTile (c);

		if ( targetTile == null ) {
			Phrase.Write ("blocked_void");
			return false;
		}

		switch (targetTile.type) {
		case Tile.Type.Hill:
			Phrase.Write ("blocked_hill");
			return false;
		case Tile.Type.Mountain:
			Phrase.Write ("blocked_mountain");
			return false;
		case Tile.Type.Sea:
			Phrase.Write ("blocked_sea");
			return false;
		case Tile.Type.Lake:
			Phrase.Write ("blocked_lake");
			return false;
		case Tile.Type.River:
			Phrase.Write ("blocked_river");
			return false;
		default:
			break;
		}

		return true;
	}

	public Direction GetDirection ( Orientation facing ) {

		int a = (int)direction + (int)facing;
		if ( a >= 8 ) {
			a -= 8;
		}

		return (Direction)a;
	}
    public Orientation GetFacing(Direction dir)
    {

        int a = (int)dir - (int)direction;
        if (a < 0)
        {
            a += 8;
        }

        return (Orientation)a;
    }

    public enum Orientation
    {
        Front,
        FrontRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        FrontLeft,

        None,

        Current,
    }
    public void Save()
    {
        
    }

}

public class Stats
{
    public enum Type
    {
        Strengh,
        Dexterity,
        Charisma,
        Constitution,
    }

    public int[] values = new int[4];

    public int GetStat(Type t)
    {
        return values[(int)t];
    }
}