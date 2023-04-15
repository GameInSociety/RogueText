
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
        UseDoor,
        LookAround,
        GoOut,

        // states
        SetState,
        
        // time
        Wait,
        PickUp,
        Throw,
        AddToTile,
        RemoveItem,
        RequireItem,
        RequireProp,
        OpenContainer,
        CloseContainer,
        Equip,
        Unequip,
        DescribeExterior,
        DisplayTimeOfDay,
        ExitByWindow,
        DescribeItem,
        PointNorth,
        Craft,
        ReadRecipe,
        DisplayHelp,
        SetParam,
        ChangeProp,
        AddProp
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

    private List<string> contents = new List<string>();

    public string GetContent(int i)
    {
        if ( i >= contents.Count)
        {
            Debug.LogError("getting contents : out of range (" + i + "/" + contents.Count + ")");
            return "no contents";
        }

        return contents[i];
    }

    public void AddContent(string str)
    {
        contents.Add(str);
    }

    public bool HasContent()
    {
        return contents.Count > 0;
    }

    public int GetContentCount()
    {
        return contents.Count;
    }

    public bool HasValue(int i)
    {
        int value = 0;
        return i < contents.Count && int.TryParse(contents[i],out value);
    }

    public int GetValue(int i)
    {
        if (i >= contents.Count)
        {
            Debug.LogError("getting values : out of range (" + i + "/" + contents.Count + ")");
            return -1;
        }

        int value = -1;

        if (!int.TryParse(contents[i], out value))
        {
            Debug.LogError("couldn't parse value : " + contents[i]);
        }

        return value;
    }
}