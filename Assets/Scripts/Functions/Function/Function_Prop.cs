using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_Prop : Function
{
    public override void TryCall()
    {
        base.TryCall();
        Call(this);
    }

    void require()
    {
        if (!HasItem(1))
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            CurrentItems.WaitForSpecificItem("item_noSecondItem");
            FunctionSequence.current.Break();
            return;
        }

        string prop_name = GetParam(0);

        Item item = GetItems.Find(x => x.HasProperty(prop_name));

        if (item == null)
        {
            TextManager.Write("&the dog (override)& has no " + prop_name, GetItem(1));
            FunctionSequence.current.Break();
            return;
        }

        if (item.GetProperty(prop_name).HasInt())
        {
            if (GetItem(1).GetProperty(prop_name).GetInt() == 0)
            {
                TextManager.Write("No more " + prop_name + " in &the dog (override)&", item);
                FunctionSequence.current.Break();
                return;
            }
        }

        FunctionSequence.current.pendingProps.Add(item.GetProperty(prop_name));
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
            Debug.LogError("ACTION_ENABLEPROPERY");
            Debug.LogError("did not find property : " + prop_name);
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

        Property newProperty = targetItem.CreateProperty(line);
    }

    void update()
    {
        Item targetItem;
      
        string targetProp = GetParam(0);
        string line = GetParam(0);

        // in the function type is not reffered, so go for part 0
        Property property = GetItem().GetProperty(targetProp);

        property.Update(line);


    }
}
