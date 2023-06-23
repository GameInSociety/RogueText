using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.Search;
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

    public delegate void OnFinishSequences();
    public static OnFinishSequences onFinishSequences;

    public string[] lines;

    public ItemGroup itemGroup;
    // the pendind props of the function
    public List<Property> pendingProps = new List<Property>();
    // the items the functions will be applied to
    public int itemIndex = 0;
    // the tile where the function takes place
    public Tile tile;

    public bool _break = false;
    public bool _nextNode = false;

    public static GameObject parent;

    public static FunctionSequence Call(
        string cell,
        ItemGroup group,
        Tile tile)
    {
        if ( tile == null)
        {
            Debug.LogError("tile is null for : " + group.text);
        }

        FunctionSequence f = new FunctionSequence();
        f.Parse(cell);
        f.SetTile(tile);
        f.itemGroup = group;

        f.Call();

        return f;
    }

    public void Call()
    {
        if ( function_OnGoing)
        {
            list.Add(this);
            return;
        }

        current = this;

        function_OnGoing = true;

        pendingProps.Clear();

        for (itemIndex = 0; itemIndex < itemGroup.GetItems.Count; itemIndex++)
        {

            // separate all actions
            foreach (var line in lines)
            {
                string _line = line;
                if (string.IsNullOrEmpty(_line)) { continue; }

                if (_line.StartsWith('*'))
                {
                    Debug.LogError(_line + " starts with *");

                    // check if the line starts with '*'
                    if (_nextNode)
                    {
                        // remove * and call function
                        _line = line.Remove(0, 1);
                        _nextNode = false;
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

                if ( _line.StartsWith("> "))
                {
                    JumpToSequence(_line);
                    goto End;
                }

                // get function class 
                string functionName = Function.GetName(_line);
                functionName = TextUtils.FirstLetterCap(functionName);
                var objectType = Type.GetType("Function_" + functionName);
                if ( objectType == null)
                {
                    Debug.LogError("function " + functionName + " doesn't exist");
                    return;
                }
                var function = Activator.CreateInstance(objectType) as Function;

                function.InitParams(_line);
                function.TryCall(itemGroup);

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

        if (itemGroup.waitForItem)
        {
        }
        else
        {
            InputInfo.Instance.Reset();
        }

        Property.DescribeUpdated();


        if ( onFinishSequences != null)
        {
            onFinishSequences();
        }
    }

    void JumpToSequence(string line)
    {
        line = line.Remove(0, 2);

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

        Verb.Sequence sequence = verb.GetSequence(item);

        FunctionSequence functionList = Call(
            sequence.content,
            itemGroup,
            Tile.GetCurrent
            );


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