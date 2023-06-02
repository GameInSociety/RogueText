using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    private static ConditionManager _instance;
    public static ConditionManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindObjectOfType<ConditionManager>();
        }

        return _instance;
    }

    public Condition[] conditions;

    private void Awake()
    {
        _instance = this;
    }


    public void AdvanceCondition()
    {
        GetCondition(Condition.Type.Thirst).Advance();
        GetCondition(Condition.Type.Hunger).Advance();
        GetCondition(Condition.Type.Sleep).Advance();
    }

    public void WriteDescription()
    {
        string text = "";

        int index = 0;

        foreach (var Condition in conditions)
        {
            if (Condition.progress != Condition.Progress.Normal)
            {
                text += Condition.GetDescription();
                
                index++;

                if ( index >= conditions.Length-1)
                {
                    break;
                }

                if ( index == conditions.Length -2 )
                {
                    text += " and ";
                }
                else
                {
                    text += ", ";
                }

            }
        }

        TextManager.Write(text);
    }

    public Condition GetCondition(Condition.Type conditionType)
    {
        return conditions[(int)conditionType];
    }
}
