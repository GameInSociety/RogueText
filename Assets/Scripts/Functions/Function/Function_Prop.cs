using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.Search;
using UnityEngine;

public class Function_Prop : Function
{
    public override void Call()
    {
        base.Call();
        Call(this);
    }
    void disable()
    {
        Item targetItem = GetItem();

        string line = GetParam(0);
        Property property = targetItem.properties.Find(x => x.name == line);
        targetItem.DisableProperty(line);
    }

    void enable()
    {
        Item targetItem = GetItem();

        string prop_name = GetParam(0);

        Property property = targetItem.properties.Find(x => x.name == prop_name);

        if (property == null)
        {
            Debug.LogError("ACTION_ENABLEPROPERY : did not find property : " + prop_name + " on " + targetItem.debug_name);
            return;
        }

        targetItem.EnableProperty(prop_name);


    }

    void checkValue()
    {
        Item targetItem = GetItem();

        string propertyName = GetParam(0);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            TextManager.Write("No " + property.name);
            FunctionSequence.current.Break();
            return;
        }

        FunctionSequence.current.pendingProps.Add(property);
    }

    void check()
    {
       
    }

    void remove()
    {
        Item targetItem = GetItem();

        string propertyName = GetParam(0);

        targetItem.DeleteProperty(propertyName);
    }

    void create()
    {
        Item targetItem = GetItem();

        string line = GetParam(0);

        Property newProperty = targetItem.AddProperty(line);
    }

    void update()
    {
        string propName = GetParam(0);
        string line = GetParam(1);
        GetItem().UpdateProperty(propName, line);
    }
}
