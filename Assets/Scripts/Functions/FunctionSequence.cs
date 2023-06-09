using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class FunctionSequence
{
    // static
    public static FunctionSequence current;
    public static List<FunctionSequence> list = new List<FunctionSequence>();
    public static bool function_OnGoing = false;

    public string[] lines;

    // the pendind props of the function
    public List<Property> pendingProps = new List<Property>();
    // the items the functions will be applied to
    public int itemIndex = 0;
    public struct ItemGroup
    {
        public List<Item> items;
    }
    public List<ItemGroup> itemGroups = new List<ItemGroup>();
    // the tile where the function takes place
    public Tile tile;

    public bool _break = false;
    public bool _nextNode = false;

    public static GameObject parent;


    public static FunctionSequence New(
        string cell,
        List<ItemGroup> itemGroups,
        Tile tile)
    {
        List<Item> items = new List<Item>();
        foreach (var group in itemGroups)
        {
            items.Add(group.items[0]);
        }

        return New(cell, items, tile);
    }
    public static FunctionSequence New(
        string cell,
        List<Item> items,
        Tile tile)
    {
        FunctionSequence f = new FunctionSequence();
        f.Parse(cell);
        f.SetTile(tile);
        foreach (var item in items)
        {
            ItemGroup group = new ItemGroup();
            group.items = items;
            f.itemGroups.Add(group);
        }
        return f;
    }

    public void Call()
    {
        if ( function_OnGoing)
        {
            //Debug.Log("[STACKING WORLD EVENT] " + lines[0]);
            list.Add(this);
            return;
        }

        //Debug.Log("[CALLING WORLD EVENT] " + lines[0]);

        current = this;

        function_OnGoing = true;

        pendingProps.Clear();

        for (itemIndex = 0; itemIndex < itemGroups.Count; itemIndex++)
        {
            // separate all actions
            foreach (var line in lines)
            {
                string _line = line;
                if (string.IsNullOrEmpty(_line)) { continue; }

                if (_line.StartsWith('*'))
                {
                    // check if the line starts with '*'
                    if (_nextNode)
                    {
                        // remove * and call function
                        _line = line.Remove(0, 1);
                    }
                    else
                    {
                        Debug.Log("* in line " + line + ", finishing sequence");
                        goto End;
                    }
                    
                }
                else
                {
                    if (_nextNode)
                    {
                        Debug.Log("skipping " + line);
                        // skip line
                        // could be the same function as break,
                        // but for now on garde les deux
                        continue;
                    }
                }

                if ( _line.StartsWith("=> "))
                {
                    JumpToSequence(_line);
                    goto End;
                }

                // get function class 
                string functionName = Function.GetName(_line);
                functionName = TextUtils.FirstLetterCap(functionName);
                var objectType = Type.GetType("Function_" + functionName);
                var function = Activator.CreateInstance(objectType) as Function;

                function.InitParams(_line);

                function.TryCall(itemGroups[itemIndex].items);

                if (_break)
                {
                    Debug.Log("breaking at " + _line);
                    goto End;
                }
            }

        }

        End:

        function_OnGoing = false;

        current = null;

        if ( list.Count > 0)
        {
            FunctionSequence worldEvent = list[0];
            list.RemoveAt(0);
            worldEvent.Call();
            return;
        }

        Property.DescribeUpdated();


    }

    void JumpToSequence(string line)
    {
        line = line.Remove(0, 3);

        Verb verb = Verb.Get(line);
        if (verb != null)
        {
            Debug.Log("found verb : " + line);
        }
        else
        {
            Debug.Log("did not find verb in " + line);
        }

        Item item = AvailableItems.FindInText(line);
        if (item != null)
        {
            Debug.Log("found item : " + item.debug_name);
        }
        else
        {
            Debug.Log("didn't find item in " + line);
        }

        string cell = verb.GetCell(item);

        FunctionSequence functionList = FunctionSequence.New(
            cell,
            itemGroups,
            Tile.GetCurrent
            );
        functionList.Call();


        Debug.Log("go to other sequence");
    }

    public void Parse(string cell)
    {
        lines = cell.Split('\n');
    }


    #region sequence
    public void GoToNextNode()
    {
        Debug.Log("skip to next node");
        _nextNode = true;
    }

    // break
    public void Break(string message)
    {
        TextManager.Write(message);
        Break();
    }
    public void Break()
    {
        _break = true;
    }
    #endregion


    #region items

    public void SetTile(Tile t)
    {
        tile = t;
    }
    
    #endregion

    

}