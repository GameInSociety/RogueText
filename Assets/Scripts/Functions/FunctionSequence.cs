using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor.Search;
using UnityEditor.SearchService;
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

    public List<Item> items;
    // the pendind props of the function
    public List<Property> pendingProps = new List<Property>();
    // the items the functions will be applied to
    public int itemIndex = 0;
    // the tile where the function takes place
    public Tile tile;

    public bool _break = false;
    public bool _nextNode = false;

    public static GameObject parent;

    public static FunctionSequence pausedSequence;

    public static FunctionSequence Call(
        string cell,
        List<Item> its,
        Tile tile)
    {
        if ( tile == null)
        {
            Debug.LogError("tile is null for : " + its[0].debug_name);
        }

        FunctionSequence f = new FunctionSequence();
        f.Parse(cell);
        f.SetTile(tile);
        f.items = its;

        f.Call();

        return f;
    }

    public void Call()
    {


        _break = false;

        if (function_OnGoing)
        {
            list.Add(this);
            return;
        }


        current = this;

        function_OnGoing = true;

        pendingProps.Clear();

        for (itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {

            // separate all actions
            foreach (var line in lines)
            {
                string _line = line;
                if (string.IsNullOrEmpty(_line)) { continue; }

                if (_line.StartsWith("else "))
                {

                    // check if the line starts with '*'
                    if (_nextNode)
                    {
                        // remove * and call function
                        _line = line.Remove(0, 5);
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

                if (_line.StartsWith("> "))
                {
                    JumpToSequence(_line);
                    goto End;
                }

                CallFunction(line);

                if (_break)
                {
                    goto End;
                }
            }

        }

        End:

        function_OnGoing = false;

        current = null;

        if (list.Count > 0)
        {
            FunctionSequence worldEvent = list[0];
            list.RemoveAt(0);
            worldEvent.Call();
            return;
        }

        if (ItemParser.waitingForItem)
        {
        }
        else
        {
            InputInfo.Instance.Reset();
        }

        //Property.DescribeUpdated();

        if (onFinishSequences != null)
        {
            onFinishSequences();
        }

        TextManager.Return();

        PropertyDescription.Describe();
    }


    void CallFunction(string line)
    {
        Item item = items[itemIndex];

        if (line.StartsWith('*'))
        {
            // search for other items / types
            string content = line.Remove(0, 1);
            content = content.Remove(content.IndexOf('*'));

            item = ItemParser.SearchItem(content);

            if ( item == null)
            {
                Debug.Log("no item of type " + content + " either, throwing confusion / absence");
                TextManager.Write("There's no " + content + " around");
                Break();
                return;
            }

            if ( ItemParser.waitingForItem)
            {
                Debug.LogError("Waiting for item spec");
                Pause();
                return;
            }

            line = line.Remove(0, content.Length + 3);
        }

        // get function class 
        string functionName = Function.GetName(line);
        functionName = TextUtils.FirstLetterCap(functionName);
        var objectType = Type.GetType("Function_" + functionName);
        if (objectType == null)
        {
            Debug.LogError("function " + functionName + " doesn't exist");
            return;
        }

        var function = Activator.CreateInstance(objectType) as Function;

        function.InitParams(line);
        function.TryCall(item);
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
            items,
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

    public void Pause()
    {
        pausedSequence = this;
        Break();
    }

    // break
    public void Break(string message)
    {
        TextManager.Write(message);
        Break();
    }
    public void Break()
    {
        current = null;
        function_OnGoing = false;
        //list.Clear();
        _break = true;
    }
    #endregion


    #region items
    public Item FirstItem
    {
        get
        {
            return items[0];
        }
    }
    public void SetTile(Tile t)
    {
        tile = t;
    }
    
    #endregion

    

}