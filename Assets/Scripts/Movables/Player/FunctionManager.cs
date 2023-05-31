
using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FunctionManager
{
    public class Function
    {
        // ChangeProp(fine, -1)

        // the parameters of the function ( fine ; -1 )
        private List<string> parameters = new List<string>();
        // the pending props of the function list
        public List<Property> props = new List<Property>();
        public List<Item> items = new List<Item>();
    }
    public static List<Function> list = new List<Function>();
    public static Function Current
    {
        get { return list[0]; }
    }

    private static List<string> parameters = new List<string>();
    public static List<Property> pendingProps = new List<Property>();
    private static List<Item> items = new List<Item>();
    public static bool breakFunctionList;

    public delegate void OnFunctionEnd();
    public static OnFunctionEnd onFunctionEnd;


    public static void CallFunctions(string functionList = null)
    {
        /// DEBUG ///
        DebugManager.Instance.function_Items.Clear();
        foreach (var item in items)
        {
            DebugManager.Instance.function_Items.Add(item);
        }
        ////////////

        while(items.Count > 0)
        {
            pendingProps.Clear();

            if ( functionList == null)
            {
                Verb verb = InputInfo.Instance.verb;

                if (!verb.HasFunctionList(GetCurrentItem()))
                {
                    TextManager.Write("input_noCombination", GetCurrentItem());
                    goto NextFunction;
                }

                functionList = verb.GetFunctionList(GetCurrentItem());
            }

            // get cell content
            string[] lines = functionList.Split('\n');

            Debug.Log("ITEM : " + items[0].debug_name);

            // separate all actions
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) { continue; }

                Call(line);

                Debug.Log("FUNCTION : " + line);

                if (breakFunctionList)
                {
                    Debug.Log(" !!!!! ACTION SEARCH IS STOPPED DUE TO THING !!!! ");
                    breakFunctionList = false;
                    return;
                }
            }


            NextFunction:
            if ( items.Count == 0)
            {
                Debug.LogError("FUNCTION : no more item");
            }
            items.RemoveAt(0);
        }

        if (onFunctionEnd != null)
        {
            onFunctionEnd();
        }
    }

    public static void Call(string line)
    {
        if (line.StartsWith("=>"))
        {
            // link to other function list
            // example : => open window
        }

        string functionName = line;

        parameters.Clear();

        // check parameters
        if (line.Contains("("))
        {
            functionName = line.Remove(line.IndexOf('('));
            InitParams(line);
        }

        FunctionList.TryFunction(functionName);   
    }


    #region break
    public static void Break(string text)
    {
        TextManager.Write(text);

        Break();
    }

    public static void Break()
    {
        breakFunctionList = true;
    }
    #endregion

    #region items
    public static List<Item> GetItems() { return items; }
    public static void SetItem(Item item)
    {
        items.Clear();
        AddItem(item);
    }
    public static void AddItem(Item item)
    {
        items.Add(item);
    }
    public static void RemoveItem(Item item)
    {
        items.Remove(item);
    }
    public static void SetItems(List<Item> _items)
    {
        items = _items;
    }
    public static bool HasItems()
    {
        return items.Count > 0;
    }
    public static bool HasItem(int i)
    {
        return i < items.Count;
    }
    public static bool HasItem(string item_name)
    {
        return items.Find(x=> x.HasWord(item_name)) != null;
    }
    public static Item GetCurrentItem()
    {
        return items[0];
    }
    public static Item GetItem(int i = 0)
    {
        return items[i];
    }
    #endregion

    #region parameters
    public static void InitParams(string line)
    {
        int startIndex = line.IndexOf('(');
        string parameters_str = line.Remove(0, startIndex +1);
        int endIndex = parameters_str.IndexOf(')');
        parameters_str = parameters_str.Remove(endIndex);

        // separate parameters
        string[] stringSeparators = new string[] { ", " };
        string[] args = parameters_str.Split(stringSeparators, StringSplitOptions.None);

        foreach (var arg in args)
        {
            parameters.Add(arg);
        }
    }
    public static void RemoveParam(int i)
    {
        if (i >= parameters.Count)
        {
            Debug.LogError("removing contents : out of range (" + i + "/" + parameters.Count + ")");
            return;
        }

        parameters.RemoveAt(i);
    }

    public static string GetParam(int i)
    {
        if ( i >= parameters.Count)
        {
            Debug.LogError("getting contents : out of range (" + i + "/" + parameters.Count + ")");
            return "no contents";
        }

        return parameters[i];
    }

    public static bool HasParams()
    {
        return parameters.Count > 0;
    }
    public static bool HasParam(int i)
    {
        return i < parameters.Count;
    }

    public static int GetParamCount()
    {
        return parameters.Count;
    }
    public static List<string> GetParams()
    {
        return parameters;
    }
    #endregion

    #region value
    public static bool HasValue(int i)
    {
        int value = 0;
        return i < parameters.Count && int.TryParse(parameters[i],out value);
    }

    public static int GetValue(int i)
    {
        if (i >= parameters.Count)
        {
            Debug.LogError("getting values : out of range (" + i + "/" + parameters.Count + ")");
            return -1;
        }

        int value = -1;

        if (!int.TryParse(parameters[i], out value))
        {
            Debug.LogError("couldn't parse value : " + parameters[i]);
        }

        return value;
    }
    #endregion
}