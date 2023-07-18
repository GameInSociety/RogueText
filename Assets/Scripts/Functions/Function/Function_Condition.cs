using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Condition : Function
{
    public override void Call()
    {
        base.Call();
        Call(this);
    }
    void MEMRDE()
    {
        Condition.Type conditionType = (Condition.Type)System.Enum.Parse(typeof(Condition.Type), GetParam(0), true);

        string str = GetParam(1);

        Condition condition = ConditionManager.GetInstance().GetCondition(conditionType);

        if (str.Contains("+"))
        {
            // add
            str = str.Remove(0, 1);
            int value = 0;
            value = int.Parse(str);

            condition.Change((int)condition.progress + value);
        }
        else if (str.Contains("-"))
        {
            // substract
            str = str.Remove(0, 1);

            int value = 0;
            value = int.Parse(str);

            condition.Change((int)condition.progress - value);

        }
        else
        {
            int value = 0;
            value = int.Parse(str);

            condition.Change(value);
        }

        ConditionManager.GetInstance().WriteDescription();
    }
}
