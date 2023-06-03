using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Prop : Function
{
    public override void Call()
    {
        base.Call();

        if (GetParam(0) == "update")
        {
            UpdateProp();
        }

        if ( GetParam(0) == "create")
        {
            CreateProp();
        }

        if ( GetParam(0) == "remove")
        {
            RemoveProp();
        }

        if ( GetParam(0) == "check")
        {
            CheckProp();
        }

        if ( GetParam(0) == "check value")
        {
            CheckValue();
        }

        if ( GetParam(0) == "enable")
        {
            Enable();
        }
        
        if (GetParam(0) == "disable")
        {
            Disable();
        }

        if ( GetParam(0) == "require")
        {
            Require();
        }

    }

    void Require()
    {
        if (!WorldEvent.current.HasItem(1))
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            InputInfo.Instance.WaitForItem();
            TextManager.Write("item_noSecondItem", WorldEvent.current.GetCurrentItem());
            WorldEvent.current.Break();
            return;
        }

        string prop_name = GetParam(1);

        Item item = WorldEvent.current.GetItems().Find(x => x.HasProperty(prop_name));

        if (item == null)
        {
            TextManager.Write("&the dog (override)& has no " + prop_name, WorldEvent.current.GetItem(1));
            WorldEvent.current.Break();
            return;
        }

        if (item.GetProperty(prop_name).HasInt())
        {
            if (WorldEvent.current.GetItem(1).GetProperty(prop_name).GetInt() == 0)
            {
                TextManager.Write("No more " + prop_name + " in &the dog (override)&", item);
                WorldEvent.current.Break();
                return;
            }
        }

        WorldEvent.current.pendingProps.Add(item.GetProperty(prop_name));
        WorldEvent.current.RemoveItem(item);
    }

    void Disable()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string line = GetParam(1);
        Property property = targetItem.properties.Find(x => x.name == line);
        targetItem.DisableProperty(line);
    }

    void Enable()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string prop_name = GetParam(1);

        Property property = targetItem.properties.Find(x => x.name == prop_name);

        if (property == null)
        {
            Debug.LogError("ACTION_ENABLEPROPERY");
            Debug.LogError("did not find property : " + prop_name);
        }

        targetItem.EnableProperty(prop_name);

    }

    void CheckValue()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string propertyName = GetParam(1);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            TextManager.Write("No " + property.name);
            WorldEvent.current.Break();
            return;
        }

        WorldEvent.current.pendingProps.Add(property);
    }

    void CheckProp()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string property_line = GetParam(1);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                WorldEvent.current.Break();
                return;
            }

            return;

        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            TextManager.Write("It's not " + parts[0]);
            WorldEvent.current.Break();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if (property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                WorldEvent.current.Break();
                return;
            }
        }
    }

    void RemoveProp()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string propertyName = GetParam(1);

        targetItem.DeleteProperty(propertyName);

    }

    void CreateProp()
    {
        Item targetItem = WorldEvent.current.GetCurrentItem();

        string line = GetParam(1);

        Property newProperty = targetItem.CreateProperty(line);
    }

    void UpdateProp()
    {
        Item targetItem;

        // if starts with "*", change property of another item in tile, not the function item
        if (GetParam(1).StartsWith('*'))
        {
            string itemName = GetParam(1).Remove(0, 1);
            targetItem = ItemManager.Instance.FindInWorld(itemName);

            if (targetItem == null)
            {
                WorldEvent.current.Break("No " + itemName);
                return;
            }

            RemoveParam(1);
        }
        else
        {
            targetItem = WorldEvent.current.GetCurrentItem();
        }

        string targetProp = GetParam(1);
        string line = GetParam(2);

        // in the function type is not reffered, so go for part 0
        Property property = targetItem.GetProperty(targetProp);

        property.Update(line);

    }
}
