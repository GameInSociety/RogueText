
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class PlayerAction
{
    public enum Type
    {
        None,

        // move
        Move,
        MoveRel,
        MoveToTargetItem,
        OrientPlayer,
        
        // look
        Look,
        LookAround,
        PointNorth,

        // interior
        Enter,
        UseDoor,
        DescribeExterior,

        // states
        SetState,
        
        // time
        Wait,
        DisplayTimeOfDay,

        // items
        PickUp,
        Throw,
        CreateInTile,
        DestroyItem,
        RequireItem,
        RequireItemWithProp,

        // prop
        RequireProp,
        ChangeProp,
        AddProp,
        RemoveProp,
        CheckProp,
        CheckPropValue,
        EnableProp,
        DisableProp,

        // container
        OpenContainer,
        CloseContainer,

        // equipment
        Equip,
        Unequip,

        // other
        Craft,
        ReadRecipe,
        SetParam,
        Write,
    }

    private static PlayerAction current;
    public Type type;
    private List<string> contents = new List<string>();

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

    public void RemoveContent(int i)
    {
        if (i >= contents.Count)
        {
            Debug.LogError("removing contents : out of range (" + i + "/" + contents.Count + ")");
            return;
        }

        contents.RemoveAt(i);
    }

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
    public bool HasContent(int i)
    {
        return contents.Count < i;
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