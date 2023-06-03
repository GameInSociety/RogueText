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
            Interior interior = WorldEvent.current.GetCurrentItem().interior;

            if ( interior == null)
            {
                interior = new Interior();
                interior.Genererate(WorldEvent.current.GetCurrentItem());
            }

            interior.Enter();
            return;
        }

        if ( GetParam(0) == "describeOut")
        {
            Interior.DescribeExterior();
            return;
        }
    }
}
