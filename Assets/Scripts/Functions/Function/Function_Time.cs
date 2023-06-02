using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Time : Function
{
    public override void Call()
    {
        base.Call();

        if ( GetParam(0) == "display")
        {
            TimeManager.Instance.WriteTimeOfDay();
            return;
        }

        if ( GetParam(0) == "wait")
        {
            int hours;

            if (InputInfo.Instance.hasValueInText)
            {
                hours = InputInfo.Instance.valueInText;
            }
            else
            {
                hours = 1;
            }

            TimeManager.Instance.Wait(hours);
        }
    }
}
