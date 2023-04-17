using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour {

	public static Player Instance;

    public Cardinal previousCardinal;
	public Cardinal currentCarnidal;

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

        Move(Cardinal.None);
    }

    void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.Move:
			Move ((Cardinal)PlayerAction.GetCurrent.GetValue(0));
			break;
		case PlayerAction.Type.MoveRel:
            Orientation moveOrientation = (Orientation)PlayerAction.GetCurrent.GetValue(0);
			Move (GetCardinal(moveOrientation));
                break;
            case PlayerAction.Type.OrientPlayer:
            Orientation lookOrientation = (Orientation)PlayerAction.GetCurrent.GetValue(0);
            Orient(lookOrientation);
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
                PhraseKey.Write("door_locked");
                return;
            }
        }

        // check if has direction ( for interiors )
        if (!targetItem.HasProperty("entrance"))
        {
            switch (targetItem.GetProperty("direction").GetContent())
            {
                case "to north":
                    Move(Cardinal.North);
                    break;

                case "to south":
                    Move(Cardinal.South);
                    break;

                case "to east":
                    Move(Cardinal.East);
                    break;

                case "to west":
                    Move(Cardinal.West);
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
        Move(GetCardinal(orientation));
    }

    public void Move ( Cardinal dir ) {

        // setting next coords
        Coords targetCoords = coords + (Coords)dir;

        // cancel if path blocked
        if (!CanMoveForward(targetCoords))
        {
            return;
        }

        StartCoroutine(MoveCoroutine(dir));

    }
    IEnumerator MoveCoroutine(Cardinal carninal)
    {
        // first of all, advance time ( and items states etc... )
        TimeManager.GetInstance().AdvanceTime();

        // ?
        if (coords.x > 0)
        {
            prevCoords = coords;
        }

        // change current coords
        coords += (Coords)carninal;

        // set preivous & current tile
        Tile.SetPrevious (Tile.GetCurrent);
        Tile.SetCurrent (TileSet.current.GetTile(coords));

        // generate tile items
        if (Tile.GetCurrent.visited == false)
        {
            Tile.GetCurrent.GenerateItems();
        }

        // because the game starts with no movement
        if (carninal == Cardinal.None)
            carninal = Cardinal.North;

        // set new direction
        currentCarnidal = carninal;
        DisplayDescription.Instance.UpdateDescription();

        MapTexture.Instance.UpdateFeedbackMap();

        Tile.GetCurrent.visited = true;

        yield return new WaitForEndOfFrame();

    }

    public void MoveToTargetItem()
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        foreach (var tileGroup in SurroundingTileManager.tileGroups)
        {
            if ( tileGroup.tile.tileItem.SameTypeAs(targetItem))
            {
                if ( InputInfo.GetCurrent.orientation != Orientation.None)
                {
                    SetDirection (GetCardinal(InputInfo.GetCurrent.orientation));

                    if (tileGroup.orientations.Contains(InputInfo.GetCurrent.orientation))
                    {
                        Cardinal dir = GetCardinal(InputInfo.GetCurrent.orientation);
                        Move(dir);
                    }
                    else
                    {
                        PhraseKey.Write("movement_blocked");
                    }
                    return;
                }
                else
                {
                    Cardinal dir = GetCardinal(tileGroup.orientations[0]);
                    Move(dir);
                    return;
                }
               
            }
        }
    }

    public void Orient(Orientation orientation)
    {
        PhraseKey.SetOverrideOrientation(orientation);
        PhraseKey.Write("position_orientPlayer");
        SetDirection(GetCardinal(orientation));
    }
    public void SetDirection(Cardinal cardinal)
    {
        previousCardinal = currentCarnidal;
        currentCarnidal = cardinal;
    }
    #endregion

    bool CanMoveForward (Coords c)
	{
		Tile targetTile = TileSet.current.GetTile (c);

		if ( targetTile == null ) {
			PhraseKey.Write ("blocked_void");
			return false;
		}

		switch (targetTile.type) {
		case Tile.Type.Hill:
			PhraseKey.Write ("blocked_hill");
			return false;
		case Tile.Type.Mountain:
			PhraseKey.Write ("blocked_mountain");
			return false;
		case Tile.Type.Sea:
			PhraseKey.Write ("blocked_sea");
			return false;
		case Tile.Type.Lake:
			PhraseKey.Write ("blocked_lake");
			return false;
		case Tile.Type.River:
			PhraseKey.Write ("blocked_river");
			return false;
		default:
			break;
		}

		return true;
	}

	public Cardinal GetCardinal ( Orientation orientation ) {

		int a = (int)currentCarnidal + (int)orientation;
		if ( a >= 8 ) {
			a -= 8;
		}

		return (Cardinal)a;
	}
    public Orientation GetOrientation(Cardinal cardinal)
    {

        int a = (int)cardinal - (int)currentCarnidal;
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