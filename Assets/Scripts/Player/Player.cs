using System.Collections;
using System.Collections.Concurrent;
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
    public Coords prevCoords = new Coords(-1, -1);
    public Coords coords = new Coords(-1, -1);
    public Coords direction = new Coords(-1, -1);

    public Coords startCoords;

    public Player()
    {

    }

    void Awake()
    {
        Instance = this;
    }

    public void Init() {

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

        PlayerActionManager.onPlayerAction += HandleOnAction;

        Move(Cardinal.None);
    }

    void HandleOnAction(PlayerAction action)
    {
        switch (action.type) {
            case PlayerAction.Type.Move:
                Move((Cardinal)PlayerAction.GetCurrent.GetValue(0));
                break;
            case PlayerAction.Type.MoveRel:
                Orientation moveOrientation = (Orientation)PlayerAction.GetCurrent.GetValue(0);
                Move(OrientationToCardinal(moveOrientation));
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
                EnterCurrentInterior();
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
        Item targetItem = InputInfo.Instance.GetItem(0);

        // check if locked
        if (targetItem.HasProperty("locked"))
        {
            PhraseKey.WritePhrase("door_locked");
            return;
        }

        // check if has direction ( for interiors )
        if (!targetItem.HasProperty("entrance"))
        {
            switch (targetItem.GetProperty("direction").value)
            {
                case "north":
                    Move(Cardinal.North);
                    break;

                case "south":
                    Move(Cardinal.South);
                    break;

                case "east":
                    Move(Cardinal.East);
                    break;

                case "west":
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

    void EnterCurrentInterior()
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
            if (Player.Instance.coords == Coords.Zero)
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

    #region vision
    public bool CanSee()
    {
        if (TimeManager.GetInstance().currentPartOfDay == TimeManager.PartOfDay.Night)
        {
            if (Inventory.Instance.HasItemWithProperty("source of light"))
            {
                PhraseKey.WritePhrase("lamp_on");
                return true;
            }
            else
            {
                PhraseKey.WritePhrase("lamp_off");
                return false;
            }
        }

        return true;
    }
    #endregion

    #region movement
    public void Move(Player.Orientation orientation)
    {
        Move(OrientationToCardinal(orientation));
    }
    public void Move(Cardinal dir)
    {
        Coords targetCoords = coords + (Coords)dir;
        Move(targetCoords);
    }

    public void Move(Coords targetCoords) {

        // cancel if path blocked
        if (!CanMoveForward(targetCoords))
        {
            return;
        }

        StartCoroutine(MoveCoroutine(targetCoords));

    }
    IEnumerator MoveCoroutine(Coords targetCoords)
    {
        // first of all, advance time ( and items states etc... )
        TimeManager.GetInstance().AdvanceTime();

        // ?
        if (coords.x > 0)
        {
            prevCoords = coords;
        }

        // change current coords
        coords = targetCoords;
        direction = coords - prevCoords;

        // set preivous & current tile
        Tile.SetPrevious(Tile.GetCurrent);
        Tile.SetCurrent(TileSet.current.GetTile(coords));

        // generate tile items
        if (Tile.GetCurrent.visited == false)
        {
            Tile.GetCurrent.GenerateItems();
        }

        // set new direction
        currentCarnidal = (Cardinal)direction;

        DisplayDescription.Instance.UpdateDescription();

        MapTexture.Instance.UpdateFeedbackMap();

        Tile.GetCurrent.visited = true;

        yield return new WaitForEndOfFrame();

    }

    public void MoveToTargetItem()
    {
        Debug.Log("move to target tile");

        Item targetItem = InputInfo.Instance.GetItem(0);

        /// ici il faut check si il y a pas d'adjectifs dans l'inputinfo
        foreach (var tile in SurroundingTiles())
        {
            Debug.Log("tile : " + tile.tileItem.debug_name);

            if (tile.tileItem.SameTypeAs(targetItem))
            {
                Move(tile.coords);
                return;
            }
        }

        PhraseKey.WritePhrase("You are already &on the dog&");
    }

    public void Orient(Orientation orientation)
    {
        PhraseKey.SetOverrideOrientation(orientation);
        PhraseKey.WritePhrase("position_orientPlayer");
        SetDirection(OrientationToCardinal(orientation));
    }
    public void SetDirection(Cardinal cardinal)
    {
        previousCardinal = currentCarnidal;
        currentCarnidal = cardinal;
    }
    #endregion

    bool CanMoveForward(Coords c)
    {
        Tile targetTile = TileSet.current.GetTile(c);

        if (targetTile == null) {
            PhraseKey.WritePhrase("blocked_void");
            return false;
        }

        switch (targetTile.type) {
            case Tile.Type.Hill:
                PhraseKey.WritePhrase("blocked_hill");
                return false;
            case Tile.Type.Mountain:
                PhraseKey.WritePhrase("blocked_mountain");
                return false;
            case Tile.Type.Sea:
                PhraseKey.WritePhrase("blocked_sea");
                return false;
            case Tile.Type.Lake:
                PhraseKey.WritePhrase("blocked_lake");
                return false;
            case Tile.Type.River:
                PhraseKey.WritePhrase("blocked_river");
                return false;
            default:
                break;
        }

        return true;
    }



    public List<Tile> SurroundingTiles()
    {
        List<Tile> result = new List<Tile>();

        List<Orientation> orientations = new List<Player.Orientation>
        {
            Orientation.Front,
            Orientation.Right,
            Orientation.Left
        };

        foreach (var orientation in orientations)
        {

            Tile targetTile = GetTile(orientation);

            if (targetTile == null)
            {
                continue;
            }

            if (targetTile.enclosed)
            {
                continue;
            }


            result.Add(targetTile);
        }

        return result;
    }

    public Tile GetTile(Orientation orientation)
    {
        Cardinal dir = OrientationToCardinal(orientation);

        Coords targetCoords = coords + (Coords)dir;

        return TileSet.current.GetTile(targetCoords);
    }


    public Cardinal OrientationToCardinal(Orientation orientation)
    {

        int a = (int)currentCarnidal + (int)orientation;
        if (a >= 8)
        {
            a -= 8;
        }

        return (Cardinal)a;
    }

    public Orientation CardinalToOrientation(Cardinal cardinal)
    {

        int a = (int)cardinal - (int)currentCarnidal;
        if (a < 0)
        {
            a += 8;
        }

        return (Orientation)a;
    }

    /// <summary>
    /// ORIENTATION : front, left, right, back etc...
    /// CARDINAL : north, west, south east
    /// DIRECTION : to north, to west, to east, to south
    /// </summary>

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