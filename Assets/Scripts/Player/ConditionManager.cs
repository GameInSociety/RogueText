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

    private void Start()
    {
        PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    private void HandleOnAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.SetState:
                SetCondition();
                break;
            case PlayerAction.Type.Wait:
                Wait();
                break;
            default:
                break;
        }
    }

    void SetCondition()
    {
        Condition.Type conditionType =  (Condition.Type)System.Enum.Parse(typeof(Condition.Type), PlayerAction.GetCurrent.GetContent(0), true);

        string str = PlayerAction.GetCurrent.GetContent(1);

        if (str.Contains("+"))
        {
            // add
            str = str.Remove(0,1);
            int value = 0;
            value = int.Parse(str);

            GetCondition(conditionType).Change((int)GetCondition(conditionType).progress+ value);
        }
        else if (str.Contains("-"))
        {
            // substract
            str = str.Remove(0, 1);

            int value = 0;
            value = int.Parse(str);

            GetCondition(conditionType).Change((int)GetCondition(conditionType).progress- value);

        }
        else
        {
            int value = 0;
            value = int.Parse(str);

            GetCondition(conditionType).Change(value);
        }

        WriteDescription();

        //Item.Remove(InputInfo.GetCurrent.GetItem(0));
    }

    #region wait
    public void Wait()
    {
        int hours;

        if (InputInfo.Instance.hasValueInText)
        {
            hours = InputInfo.Instance.valueInText;
        }
        else
        {
            hours = PlayerAction.GetCurrent.GetValue(0);
        }
        
        TimeManager.GetInstance().NextHour(hours);

        // pour l'instant un peu oblig� � cause des objets etc...
        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

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

        TextManager.WriteHard(text);
    }

    public Condition GetCondition(Condition.Type conditionType)
    {
        return conditions[(int)conditionType];
    }
}
