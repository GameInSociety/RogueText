using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Time : Function
{
    public override void TryCall(ItemGroup itemGroup)
    {
        base.TryCall(itemGroup);
        Call(this);
    }

    void display()
    {
        TimeManager.Instance.WriteTimeOfDay();
    }

    void wait()
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
