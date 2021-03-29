
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class Action
{
    public enum Type
    {
        None,

        Move,
        MoveRel,
        MoveToTargetItem,
        Look,
        DisplayInventory,
        CloseInventory,
        Enter,
        GoOut,
        Eat,
        Drink,
        DrinkAndRemove,
        Sleep,
        Take,
        Throw,
        AddToInventory,
        RemoveFromInventory,
        AddToTile,
        RemoveFromTile,
        Require,
        Display,
        GiveClue,
        MoveAway,
        OpenContainer,
        CloseContainer,
        Equip,
        Unequip,
        DescribeExterior,
        DisplayTimeOfDay,
        ExitByWindow,
        DescribeItem,
        PointNorth,
        RemoveLastItem,
        CheckStat,
        ReplaceItem,
        Craft,
        ReadRecipe,
        DisplayHelp,
    }

    private static Action current;

    public static void SetCurrent(Action action)
    {
        current = action;
    }

    public static Action GetCurrent
    {
        get
        {
            return current;
        }
    }

    public void Call()
    {
        SetCurrent(this);
    }


    public Type type;

    public List<string> contents = new List<string>();
    public List<int> ints = new List<int>();
}