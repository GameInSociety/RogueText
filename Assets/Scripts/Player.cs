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
    /// hunger
    /// </summary>
    public int hunger_HoursToNextStep = 5;
    public int hunger_HourCount = 0;
	public int hunger_CurrentStep = 0;
    public int hunger_MaxStep = 20;

    /// <summary>
    /// thirst
    /// </summary>
    public int thirst_HoursToNextStep = 5;
    public int thirst_CurrentHour = 0;
	public int thirst_CurrentStep = 0;
    public int thirst_MaxStep = 10;

    /// <summary>
    /// sleep
    /// </summary>
    public int sleep_HoursToNextStep = 4;
    public int sleep_CurrentHour = 0;
	public int sleep_CurrentStep = 0;
	public int sleep_MaxStep = 40;

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

		ActionManager.onAction+= HandleOnAction;

        Move(Direction.None);
    }

    void HandleOnAction (Action action)
	{
		switch (action.type) {
		case Action.Type.Move:
			Move ((Direction)Action.GetCurrent.ints[0]);
			break;
		case Action.Type.MoveRel:
			Move (GetDirection((Orientation)Action.GetCurrent.ints[0]));
			break;
            case Action.Type.MoveToTargetItem:
                MoveToTargetItem();
                break;
            case Action.Type.Look:
			break;
		case Action.Type.Enter:
			EnterCurrentInterior ();
			break;
        case Action.Type.ExitByWindow:
            Interior.current.ExitByWindow();
            break;
		case Action.Type.GoOut:
			break;
		case Action.Type.Eat:
			Eat ();
			break;
        case Action.Type.DrinkAndRemove:
            DrinkAndRemove();
            break;
        case Action.Type.Drink:
			Drink ();
			break;
        case Action.Type.CheckStat:
            CheckStat();
            break;
            case Action.Type.Sleep:
			Sleep();
			break;
		default:
			break;
		}
	}

    void EnterCurrentInterior ()
	{
        if (Interior.current == null)
        {
            Interior.Get(coords).Enter();
        }
        else
        {
            Interior.current.ExitByDoor();
        }
	}

    #region movement
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
        // ?
        if (coords.x > 0)
        {
            prevCoords = coords;
        }

        // change current coords
        coords += (Coords)dir;

        // set preivous & current tile
        Tile.previous = Tile.current;
        Tile.current = TileSet.current.GetTile(coords);

        // generate tile items
        if (Tile.current.visited == false)
        {
            Tile.current.GenerateItems();
        }

        // because the game starts with no movement
        if (dir == Direction.None)
            dir = Direction.North;

        // set new direction
        direction = dir;

        Tile.Describe();

        TimeManager.Instance.AdvanceTime();

        MapTexture.Instance.UpdateMap();

        Tile.current.visited = true;

        yield return new WaitForSeconds(Transition.Instance.duration);

        Phrase.item = Tile.current.tileItem;
        string str = Phrase.GetPhrase("movement_feedback");
        DisplayFeedback.Instance.Display(str);


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
                        Phrase.item = tileGroup.tile.tileItem;
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

	#region states
	public void UpdateStates ()
	{
        //++sleep;
        ++thirst_CurrentHour;
        ++sleep_CurrentHour;
        ++hunger_HourCount;

        if ( hunger_HourCount == hunger_HoursToNextStep)
        {
            hunger_HourCount = 0;
            AddHunger(1);
        }

        if ( sleep_CurrentHour == sleep_HoursToNextStep)
        {
            sleep_CurrentHour = 0;
            AddSleep(1);
        }

        if ( thirst_CurrentHour == thirst_HoursToNextStep)
        {
            thirst_CurrentHour = 0;
            AddThirst(1);
        }

	}
    #endregion

    #region hunger
    void Eat ()
	{
        RemoveHunger(Action.GetCurrent.ints[0]);

        if (Action.GetCurrent.ints.Count > 1)
        {
            health -= Action.GetCurrent.ints[1];
        }

        string str = "";

        string name = "" + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None);

        if (hunger_CurrentStep == 3)
        {
            str = name + " ne vous a pas vraiment nourri, il va faloir manger dans peu de temps...";
        }
        else if (hunger_CurrentStep == 2)
        {
            str = name + " vous permet d'attendre quelques heures, mais ce n'était pas très consistent";
        }
        else if (hunger_CurrentStep == 1)
        {
            str = name + " ne vous rassasis pas mais vous êtes satisfait";
        }
        else
        {
            str = name + " vous rempli totalement le ventre, vous êtes rassasié";
        }

        hunger_HourCount = 0;

        DisplayFeedback.Instance.Display(str);

        Item.Remove(InputInfo.GetCurrent.MainItem);

	}
    public void RemoveHunger ( int i)
    {
        hunger_CurrentStep -= i;

        hunger_CurrentStep = Mathf.Clamp(hunger_CurrentStep, 0, 4);
    }
    public void AddHunger(int i)
    {
        hunger_CurrentStep += i;

        hunger_CurrentStep = Mathf.Clamp(hunger_CurrentStep, 0, 4);
    }
    #endregion

    #region sleep
    public void RemoveSleep(int i)
    {
        sleep_CurrentStep -= i;

        sleep_CurrentStep = Mathf.Clamp(sleep_CurrentStep, 0, 4);
    }
    public void AddSleep(int i)
    {
        sleep_CurrentStep += i;

        sleep_CurrentStep = Mathf.Clamp(sleep_CurrentStep, 0, 4);
    }
    void Sleep()
    {
        RemoveSleep(Action.GetCurrent.ints[0]);

        string str = "";

        if (sleep_CurrentStep == 3)
        {
            str = "Vous n'avez pas très bien dormis, mais récupérez un peu d'énergie";
        }
        else if (sleep_CurrentStep == 2)
        {
            str = "Vous avez dormis et récupérez un peu d'énergie";
        }
        else if (sleep_CurrentStep == 3)
        {
            str = "Après quelques heures de sommeil, vous vous sentez légèrement reposé";
        }
        else
        {
            str = "Vous vous sentez entièrement reposé";
        }

        sleep_CurrentHour = 0;

        DisplayDescription.Instance.ClearDescription ();

        DisplayFeedback.Instance.Display(str);

        TimeManager.Instance.timeOfDay = 6;
        TimeManager.Instance.NextDay();

        Invoke("SleepDelay",3f);

    }
    void SleepDelay()
    {
        Tile.Describe();
    }
    #endregion

    #region thirst
    public void RemoveThirst(int i)
    {
        thirst_CurrentStep -= i;

        thirst_CurrentStep = Mathf.Clamp(thirst_CurrentStep, 0, 4);
    }
    public void AddThirst(int i)
    {
        thirst_CurrentStep += i;

        thirst_CurrentStep = Mathf.Clamp(thirst_CurrentStep, 0, 4);
    }
    void DrinkAndRemove()
    {
        Drink();

        Item.Remove(InputInfo.GetCurrent.MainItem);

    }
    void Drink ()
	{
        RemoveThirst(4);

        thirst_CurrentHour = 0;

        DisplayFeedback.Instance.Display(InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Defined, Word.Preposition.None, Word.Number.None) + " vous déshydrate, vous n'avez plus soif");

        Item.Remove(InputInfo.GetCurrent.MainItem);

    }
    #endregion

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
        string str = Action.GetCurrent.contents[0];

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

        if ( stats.GetStat(statType) < Action.GetCurrent.ints[0])
        {
			ActionManager.Instance.BreakAction ();
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