using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Function
{
    // ChangeProp(fine, -1)

    public static Function current;

    // the parameters of the function ( fine ; -1 )
    private List<string> parameters = new List<string>();
    // the pending props of the function list

    private List<Item> items = new List<Item>();

    public List<Item> GetItems
    {
        get { return items; }
    }
    public Item GetItem(int i = 0)
    {
        if (i >= items.Count)
        {
            Debug.LogError("Function GetItem : outside range ");
            return null;
        }

        return items[i];
    }
    public bool HasItem(int i)
    {
        return i < items.Count;
    }

    public static string GetName(string line)
    {
        if (line.Contains("("))
        {
            return line.Remove(line.IndexOf('('));
        }


        return line;
    }

    public virtual void TryCall()
    {
        items = CurrentItems.Get;

        if (GetParam(0).StartsWith('*'))
        {
            // targeting specific item
            string itemName = GetParam(0).Remove(0, 1);

            if ( CurrentItems.HasItem(itemName) == false)
            {
                Debug.Log("no item of type : " + itemName);
                CurrentItems.FindAll(itemName);
            }
            
            if (CurrentItems.waitForItem)
            {
                Debug.Log("waiting for item spec");
                //TextManager.Write("There's no " + GetParam(0));
                FunctionSequence.current.Break();
                return;
            }
     //TextManager.Write("You VERB_NAME &the dog& with ", GetItem());
            items[0] = CurrentItems.FindOfType(itemName);
            Debug.Log("setting first item : " + items[0].debug_name);
            //TextManager.Add("&the dog&", GetItem());
            RemoveParam(0);
        }

    }

    public virtual void Call(object obj)
    {
        current = this;

        string methodName = GetParam(0);
        Type type = obj.GetType();
        MethodInfo mi = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (mi == null)
        {
            Debug.LogError("no function " + methodName + " in " + type.Name);
            return;
        }
        RemoveParam(0);
        mi.Invoke(obj, null);
    }

    #region parameters
    public void InitParams(string line)
    {
        int startIndex = line.IndexOf('(');
        string parameters_str = line.Remove(0, startIndex + 1);
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
    public void RemoveParam(int i)
    {
        if (i >= parameters.Count)
        {
            Debug.LogError("removing contents : out of range (" + i + "/" + parameters.Count + ")");
            return;
        }

        parameters.RemoveAt(i);
    }

    public void SetParam(int i, string param)
    {
        if (i >= parameters.Count)
        {
            Debug.LogError("setting function params : out of range (" + i + "/" + parameters.Count + ")");
            return;
        }

        parameters[i] = param;
    }

    public string GetParam(int i)
    {
        if (i >= parameters.Count)
        {
            Debug.LogError("getting contents : out of range (" + i + "/" + parameters.Count + ")");
            return "no contents";
        }

        return parameters[i];
    }

    public bool HasParams()
    {
        return parameters.Count > 0;
    }
    public bool HasParam(int i)
    {
        return i < parameters.Count;
    }

    public int GetParamCount()
    {
        return parameters.Count;
    }
    public List<string> GetParams()
    {
        return parameters;
    }
    #endregion

    #region value
    public bool HasValue(int i)
    {
        int value = 0;
        return i < parameters.Count && int.TryParse(parameters[i], out value);
    }

    public int ParseParam(int i)
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