using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

[System.Serializable]
public class Function {
    // the parameters of the function ( fine ; -1 )
    private List<string> parameters = new List<string>();
    // the pending props of the function list

    public Item item;

    public Item targetItem() {
        return item;
    }

    /*public bool HasItem(int i)
    {
        return i < GetItems.Count;
    }*/

    public static string GetName(string line) {
        if (line.Contains("(")) {
            return line.Remove(line.IndexOf('('));
        }

        return line;
    }

    public virtual void TryCall(Item it) {
        item = it;

        /*if (GetParam(0).StartsWith('*'))
        {
            // targeting specific item
            string itemName = GetParam(0).Remove(0, 1);

            Item newItem;
            if (ItemParser.Get.replacingItem)
            {
                newItem = ItemParser.Get.sustainItem;
                //ItemParser.Get.replacingItem = false;
            }
            else
            {
                Item specItem = ItemParser.Get.GetItems.Find(x=> x.debug_name == itemName);
                if ( specItem != null)
                {
                    item = specItem;
                }
                else
                {
                    ItemParser.Get.ParseItems(itemName);
                    if (ItemParser.Get.IsEmpty)
                    {
                        TextManager.Write("Theres no " + itemName + " around");
                        FunctionSequence.current.Break();
                        return;
                    }
                    if (ItemParser.Get.waitingForItem)
                    {
                        ItemParser.Get.replacingItem = true;
                        FunctionSequence.current.Pause();
                        return;
                    }
                    item = ItemParser.Get.FirstItem;

                }
            }


        }*/

        Call();
    }

    public virtual void Call() {

    }

    public virtual void Call(object obj) {
        var methodName = GetParam(0);
        var type = obj.GetType();
        var mi = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (mi == null) {
            Debug.LogError("no function " + methodName + " in " + type.Name);
            return;
        }
        RemoveParam(0);
        _ = mi.Invoke(obj, null);
    }

    #region parameters
    public void InitParams(string line) {
        var startIndex = line.IndexOf('(');
        var parameters_str = line.Remove(0, startIndex + 1);
        var endIndex = parameters_str.IndexOf(')');
        parameters_str = parameters_str.Remove(endIndex);

        // separate parameters
        var stringSeparators = new string[] { ", " };
        var args = parameters_str.Split(stringSeparators, StringSplitOptions.None);

        foreach (var arg in args) {
            parameters.Add(arg);
        }
    }
    public void RemoveParam(int i) {
        if (i >= parameters.Count) {
            Debug.LogError("removing contents : out of range (" + i + "/" + parameters.Count + ")");
            return;
        }

        parameters.RemoveAt(i);
    }

    public void SetParam(int i, string param) {
        if (i >= parameters.Count) {
            Debug.LogError("setting function params : out of range (" + i + "/" + parameters.Count + ")");
            return;
        }

        parameters[i] = param;
    }

    public string GetParam(int i) {
        if (i >= parameters.Count) {
            //Debug.LogError("getting contents : out of range (" + i + "/" + parameters.Count + ")");
            return "no contents";
        }

        return parameters[i];
    }

    public bool HasParams() {
        return parameters.Count > 0;
    }
    public bool HasParam(int i) {
        return i < parameters.Count;
    }

    public int GetParamCount() {
        return parameters.Count;
    }
    public List<string> GetParams() {
        return parameters;
    }
    #endregion

    #region value
    public bool HasValue(int i) {
        return i < parameters.Count && int.TryParse(parameters[i], out _);
    }

    public int ParseParam(int i) {
        try {
            return int.Parse(parameters[i]);
        } catch {
            Debug.LogError($"couldn't parse parameter {i}");
            return -1;
        }
    }
    #endregion
}