using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_Prop : Function
{
    public override void Call(List<Item> items)
    {
        base.Call(items);

        string methodName = GetParam(0);
        MethodInfo mi = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if ( mi == null)
        {
            Debug.LogError("no function " + methodName + " in " + GetType().Name);
        }

        mi.Invoke(this, null);
        
    }

    void require()
    {
        if (!HasItem(1))
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            CurrentItems.AskForSpecificItem("item_noSecondItem");
            FunctionSequence.current.Break();
            return;
        }

        string prop_name = GetParam(1);

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

        string line = GetParam(1);
        Property property = targetItem.properties.Find(x => x.name == line);
        targetItem.DisableProperty(line);
    }

    void enable()
    {
        Item targetItem = GetItem();

        string prop_name = GetParam(1);

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

        string propertyName = GetParam(1);

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
        Item targetItem = GetItem();

        Debug.Log("check item : " + targetItem.debug_name);

        string property_line = GetParam(1);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                FunctionSequence.current.Break();
                return;
            }

            return;

        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            Debug.Log(targetItem.debug_name + " id : " + targetItem.debug_randomID + " dont have the prop " + parts[0]);

            TextManager.Write("It's not " + parts[0]);
            FunctionSequence.current.Break();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if (property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                FunctionSequence.current.Break();
                return;
            }
        }
    }

    void remove()
    {
        Item targetItem = GetItem();

        string propertyName = GetParam(1);

        targetItem.DeleteProperty(propertyName);


    }

    void create()
    {
        Item targetItem = GetItem();

        string line = GetParam(1);

        Property newProperty = targetItem.CreateProperty(line);
    }

    void update()
    {
        Item targetItem;
      
        string targetProp = GetParam(1);
        string line = GetParam(2);

        // in the function type is not reffered, so go for part 0
        Property property = GetItem().GetProperty(targetProp);

        property.Update(line);


    }
}
