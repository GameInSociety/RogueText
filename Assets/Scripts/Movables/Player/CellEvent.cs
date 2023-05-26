
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CellEvent
{
    private static List<string> contents = new List<string>();
    public static List<Property> props = new List<Property>();
    public static bool breakEvents;

    public static void CallEvents(string cell)
    {
        props.Clear();

        // get cell content
        string[] lines = cell.Split('\n');

        // separate all actions
        foreach (var line in lines)
        {
            Call(line);

            if (breakEvents)
            {
                Debug.Log(" !!!!! ACTION SEARCH IS STOPPED DUE TO THING !!!! ");
                breakEvents = false;
                break;
            }
        }
    }

    public static void Call(string line)
    {
        if (line.StartsWith("=>"))
        {
            // link to other combinationn
            // example : => open window
        }

        Debug.Log("line: " + line);

        string functionName = line;

        contents.Clear();

        // check parameters
        if (line.Contains("("))
        {
            int parenthesesIndex = functionName.IndexOf('(');

            // get function
            functionName = functionName.Remove(parenthesesIndex);
            Debug.Log("function name : " + functionName);

            // get parentheses
            string parameters_str = line.Remove(0, functionName.ToString().Length + 1);
            parameters_str = parameters_str.Remove(parameters_str.Length - 1);

            // separate parameters
            string[] stringSeparators = new string[] { ", " };
            string[] args = parameters_str.Split(stringSeparators, StringSplitOptions.None);

            foreach (var arg in args)
            {
                AddContent(arg);
            }
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
        breakEvents = true;
    }
    #endregion

    #region content
    public static void RemoveContent(int i)
    {
        if (i >= contents.Count)
        {
            Debug.LogError("removing contents : out of range (" + i + "/" + contents.Count + ")");
            return;
        }

        contents.RemoveAt(i);
    }

    public static string GetContent(int i)
    {
        if ( i >= contents.Count)
        {
            Debug.LogError("getting contents : out of range (" + i + "/" + contents.Count + ")");
            return "no contents";
        }

        return contents[i];
    }

    public static void AddContent(string str)
    {
        contents.Add(str);
    }

    public static bool HasContent()
    {
        return contents.Count > 0;
    }
    public static bool HasContent(int i)
    {
        return i < contents.Count;
    }

    public static int GetContentCount()
    {
        return contents.Count;
    }
    #endregion

    #region value
    public static bool HasValue(int i)
    {
        int value = 0;
        return i < contents.Count && int.TryParse(contents[i],out value);
    }

    public static int GetValue(int i)
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
    #endregion
}