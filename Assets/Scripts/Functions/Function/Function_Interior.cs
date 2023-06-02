using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Interior : Function
{
    public override void Call()
    {
        base.Call();

        if ( GetParam(0) == "enter")
        {
            Interior.Get(Player.Instance.coords).Enter();
            return;
        }

        if ( GetParam(0) == "describeOut")
        {
            Interior.DescribeExterior();
            return;
        }
    }
}
