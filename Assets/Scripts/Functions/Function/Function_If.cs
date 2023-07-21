using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_If : Function
{
    public override void Call()
    {
        base.Call();
        Call(this);
    }
    void prop()
    {
        Item targetItem = GetItem();

        string property_line = GetParam(0);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                FunctionSequence.current.Stop();
                return;
            }

            return;

        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            Debug.Log(targetItem.debug_name + " id : " + targetItem.debug_randomID + " dont have the prop " + parts[0]);
            Debug.Log("NOT WRITING IT BECAUSE IT WRITES IT IN WEIRD WAYS");
            //TextManager.Write("It's not " + parts[0]);
            FunctionSequence.current.Stop();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if (property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                FunctionSequence.current.Stop();
                return;
            }
        }
    }

    void has()
    {
        if (ItemParser.HasItem( GetParam(0)) )
        {
            Debug.Log("has " + GetParam(0));

            // do stuff below until "*"
        }
        else
        {
            /*if ( GetParam(1) != "hide")
            {
                TextManager.Write("I have no " + GetParam(0));
            }*/
            Debug.Log("doesn't have " + GetParam(0));
            FunctionSequence.current.GoToNextNode();
            // go to next "*"
        }
    }

}
