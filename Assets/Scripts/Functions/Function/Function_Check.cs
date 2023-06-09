using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Check : Function
{
    public override void Call(List<Item> items)
    {
        base.Call(items);

        if ( GetParam(0) == "second item")
        {
            if (HasItem(1))
            {
                Debug.Log("has second item");
                
                // do stuff below until "*"
            }
            else
            {
                FunctionSequence.current.GoToNextNode();
                // go to next "*"
            }
        }

        // useless pour l'instant, dans Function_Item.Require mais centraliser les checks ?
        if (GetParam(0) == "item")
        {
            string item_name = GetParam(1);
            Item targetItem = AvailableItems.Find(item_name);

            if (targetItem == null)
            {

            }
        }
    }

}
