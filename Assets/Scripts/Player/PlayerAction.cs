
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class PlayerAction
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
        LookAround,
        GoOut,
        Eat,
        Drink,
        DrinkAndRemove,
        ChangeState,
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
        SetParam,
        Plant,
        Unearth,
        Fill
    }

    private static PlayerAction current;

    public static void SetCurrent(PlayerAction action)
    {
        current = action;
    }

    public static PlayerAction GetCurrent
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
    public List<int> values = new List<int>();
}