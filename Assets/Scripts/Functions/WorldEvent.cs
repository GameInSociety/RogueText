using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class WorldEvent
{

    // Pareil que pour les function,
    // faire différentes classe enfantes
    // pour chacun des evenements

    public string name = "";
    // debug
    public GameObject obj;

    // static
    public static WorldEvent current;
    public static List<WorldEvent> list = new List<WorldEvent>();
    public static bool function_OnGoing = false;


    public string[] lines;

    // the pendind props of the function
    public List<Property> pendingProps = new List<Property>();
    // the items the functions will be applied to
    int itemIndex = 0;
    public List<Item> targetItems = new List<Item>();
    // the tile where the function takes place
    public Tile tile;

    public bool _break = false;

    public static GameObject parent;


    public static WorldEvent New(
        string _name,
        string cell,
        List<Item> items,
        Tile tile)
    {
        WorldEvent f = new WorldEvent();
        f.name = _name;
        f.Parse(cell);
        f.SetItems(items);
        f.SetTile(tile);

        f.CreateDebugObj();
        return f;
    }

    public void CreateDebugObj()
    {
        //
        obj = new GameObject();
        obj.name = name;
        if (parent == null)
        {
            parent = new GameObject();
            parent.name = "Events";
        }
        obj.transform.SetParent(parent.transform);
        //
    }

    public void Call()
    {


        if ( function_OnGoing)
        {
            Debug.Log("stack function : " + name + " " + lines[0]);

            list.Add(this);
            return;
        }


        Debug.Log("call function : " + name + " " + lines[0]);

        current = this;

        function_OnGoing = true;

        pendingProps.Clear();

        for (itemIndex = 0; itemIndex < targetItems.Count; itemIndex++)
        {
            // separate all actions
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) { continue; }


                string functionName = Function.GetName(line);
                functionName = TextUtils.FirstLetterCap(functionName);
                var objectType = Type.GetType("Function_" + functionName);
                var function = Activator.CreateInstance(objectType) as Function;

                function.InitParams(line);

                function.Call();

                if (_break)
                {
                    goto End;
                }
            }

        }
        
        End:

        function_OnGoing = false;
        
        if ( list.Count > 0)
        {
            WorldEvent worldEvent = list[0];
            list.RemoveAt(0);
            worldEvent.Call();
            return;
        }

        PropertyEvent.DescribeProperties();
    }

    public void Parse(string cell)
    {
        lines = cell.Split('\n');
    }

    public void Break(string message)
    {
        TextManager.Write(message);
        _break = true;
    }
    public void Break()
    {
        _break = true;
    }

    #region items
    public List<Item> GetItems() { return targetItems; }

    public void SetTile(Tile t)
    {
        tile = t;
    }

    public void SetItem(Item item)
    {
        targetItems.Clear();
        AddItem(item);
    }
    public void AddItem(Item item)
    {
        targetItems.Add(item);
    }
    public void RemoveItem(Item item)
    {
        targetItems.Remove(item);
    }
    public void SetItems(List<Item> _items)
    {
        targetItems = _items;
    }
    public bool HasItems()
    {
        return targetItems.Count > 0;
    }
    public bool HasItem(int i)
    {
        return i < targetItems.Count;
    }
    public bool HasItem(string item_name)
    {
        return targetItems.Find(x => x.HasWord(item_name)) != null;
    }
    public Item GetCurrentItem()
    {
        return targetItems[itemIndex];
    }
    public Item GetItem(int i = 0)
    {
        return targetItems[i];
    }
    #endregion

    

}