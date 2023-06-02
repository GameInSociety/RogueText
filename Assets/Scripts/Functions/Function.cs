using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Function
{
    // ChangeProp(fine, -1)

    public static Function current;

    // the parameters of the function ( fine ; -1 )
    private List<string> parameters = new List<string>();
    // the pending props of the function list

    public static string GetName(string line)
    {
        if (line.Contains("("))
        {
            return line.Remove(line.IndexOf('('));
        }

        return line;
    }

    public virtual void Call()
    {
        current = this;
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