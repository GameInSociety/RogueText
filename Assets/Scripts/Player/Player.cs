using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour {

	public static Player Instance;

	public Direction direction;

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
			Move ((Direction)PlayerAction.GetCurrent.values[0]);
			break;
		case PlayerAction.Type.MoveRel:
			Move (GetDirection((Orientation)PlayerAction.GetCurrent.values[0]));
			break;
            case PlayerAction.Type.MoveToTargetItem:
                MoveToTargetItem();
                break;
            case PlayerAction.Type.Look:
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
        case PlayerAction.Type.CheckStat:
            CheckStat();
            break;
		default:
			break;
		}
	}

    void EnterCurrentInterior ()
	{
        Item targetItem = InputInfo.GetCurrent.MainItem;

        // check if locked
        if (targetItem.HasProperty("locked"))
        {
            if (targetItem.GetProperty("locked").GetValue() == "true")
            {
                DisplayFeedback.Instance.Display("La porte est vérouillée... Il vous faut une clef");
                return;
            }
        }

        // check if has direction ( for interiors )
        if (targetItem.HasProperty("direction"))
        {
            switch (targetItem.GetProperty("direction").GetValue())
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
            if (Interior.GetCurrent == null)
            {
                Interior.Get(coords).Enter();
            }
            else
            {
                Interior.GetCurrent.ExitByDoor();
            }

            //Debug.LogError("door doesn't have param direction");
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
                //DisplayFeedback.Instance.Display("Vous n'êtes pas près de l'entrée");
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
                    Orientation orientation = InputInfo.GetCurrent.orientation;
                    Phrase.orientation = orientation;

                    if (tileGroup.orientations.Contains(orientation))
                    {
                        Direction dir = GetDirection(orientation);
                        Move(dir);
                    }
                    else
                    {
                        string str = Phrase.GetPhrase("movement_blocked");
                        DisplayFeedback.Instance.Display(str);
                    }
                    return;
                }
                else
                {
                    Orientation orientation = tileGroup.orientations[0];
                    Phrase.orientation = orientation;
                    Direction dir = GetDirection(orientation);
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
			DisplayFeedback.Instance.Display ("Vous ne pouvez pas aller par là");
			return false;
		}

		switch (targetTile.type) {
		case Tile.Type.Hill:
			DisplayFeedback.Instance.Display ("La coline est trop haute, impossible de passer");
			return false;
		case Tile.Type.Mountain:
			DisplayFeedback.Instance.Display ("La pente est trop raide, il faut faire demi tour");
			return false;
		case Tile.Type.Sea:
			DisplayFeedback.Instance.Display ("Le niveau de la mer est trop haut, impossible d'avancer");
			return false;
		case Tile.Type.Lake:
			DisplayFeedback.Instance.Display ("Le lac est trop profond, impossible d'avancer sans bateau");
			return false;
		case Tile.Type.River:
			DisplayFeedback.Instance.Display ("Le courant de la rivère est trop fort, impossible d'avancer");
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

    #region stats
    void CheckStat()
    {
        string str = PlayerAction.GetCurrent.contents[0];

        Stats.Type statType = Stats.Type.Strengh;

        switch (str)
        {
            case "STR":
                statType = Stats.Type.Strengh;
                break;
            case "DEX":
                statType = Stats.Type.Dexterity;
                break;
            case "CHA":
                statType = Stats.Type.Charisma;
                break;
            case "CON":
                statType = Stats.Type.Constitution;
                break;
            default:
                break;
        }

        if ( stats.GetStat(statType) < PlayerAction.GetCurrent.values[0])
        {
			PlayerActionManager.Instance.BreakAction ();
            DisplayFeedback.Instance.Display("Vous n'avez pas assez de : " + statType);
        }
    }
    #endregion

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