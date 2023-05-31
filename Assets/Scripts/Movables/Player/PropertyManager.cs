using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public class PropertyManager : MonoBehaviour
{
    #region actions
    public static void Event_RequireItemWithProp()
    {
        if (!FunctionManager.HasItem(1))
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            InputInfo.Instance.WaitForItem();
            TextManager.Write("item_noSecondItem", FunctionManager.GetCurrentItem());
            FunctionManager.Break();
            return;
        }

        string prop_name = FunctionManager.GetParam(0);

        Item item = FunctionManager.GetItems().Find(x => x.HasProperty(prop_name));

        if (item == null)
        {
            TextManager.Write("&the dog (override)& has no " + prop_name, FunctionManager.GetItem(1));
            FunctionManager.Break();
            return;
        }

        if (item.GetProperty(prop_name).HasInt())
        {
            if (FunctionManager.GetItem(1).GetProperty(prop_name).GetInt() == 0)
            {
                TextManager.Write("No more " + prop_name + " in &the dog (override)&", item);
                FunctionManager.Break();
                return;
            }
        }

        FunctionManager.pendingProps.Add(item.GetProperty(prop_name));
        FunctionManager.RemoveItem(item);
    }
    
    public static void ChangeProperty()
    {
        Item targetItem;

        // if starts with "*", change property of another item in tile, not the function item
        if (FunctionManager.GetParam(0).StartsWith('*'))
        {
            string itemName = FunctionManager.GetParam(0).Remove(0,1);
            targetItem = ItemManager.Instance.FindInWorld(itemName);

            if ( targetItem == null)
            {
                FunctionManager.Break("No " + itemName);
                return;
            }

            FunctionManager.RemoveParam(0);
        }
        else
        {
            targetItem = FunctionManager.GetCurrentItem();
        }

        string targetProp = FunctionManager.GetParam(0);
        string line = FunctionManager.GetParam(1);

        // in the function type is not reffered, so go for part 0
        Property property = targetItem.GetProperty(targetProp);

        property.Update(line);
    }

    /// <summary>
    ///  ADD PROPERTY
    /// </summary>
    public static void Event_AddProperty()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string line = FunctionManager.GetParam(0);

        Action_AddProperty(targetItem, line);
    }
    public static void Action_AddProperty(Item targetItem, string property_line)
    {
        Property newProperty = targetItem.CreateProperty(property_line);
    }

    /// <summary>
    /// REMOVE PROPERTY
    /// </summary>
    public static void Event_RemoveProperty()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string propertyName = FunctionManager.GetParam(0);

        targetItem.DeleteProperty(propertyName);
    }

    /// <summary>
    /// CHECK PROPERTY ON TARGET ITEM
    /// </summary>
    public static void Event_CheckProp()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string property_line = FunctionManager.GetParam(0);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                FunctionManager.Break();
                return;
            }

            return;
            
        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            TextManager.Write("It's not " + parts[0]);
            FunctionManager.Break();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if ( property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                FunctionManager.Break();
                return;
            }
        }
        else
        {
            // peut �tre obsoloete

            /*if (property.name != parts[0])
            {
                TextManager.WriteHard("It's not " + parts[0]);
                PlayerActionManager.Instance.BreakAction();
                return;
            }*/
        }
    }
    public static void Event_CheckPropertyValue()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string propertyName = FunctionManager.GetParam(0);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            TextManager.Write("No " + property.name);
            FunctionManager.Break();
            return;
        }

        FunctionManager.pendingProps.Add(property);
    }

    /// <summary>
    /// ENABLE / DISABLE PROPERTIES
    /// </summary>
    public static void Event_EnableProperty()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string prop_name = FunctionManager.GetParam(0);

        Property property = targetItem.properties.Find(x => x.name == prop_name);

        if ( property == null)
        {
            Debug.LogError("ACTION_ENABLEPROPERY");
            Debug.LogError("did not find property : " + prop_name);
        }

        property.Enable();
    }
    public static void Event_DisableProperty()
    {
        Item targetItem = FunctionManager.GetCurrentItem();

        string line = FunctionManager.GetParam(0);
        Action_DisableProperty(targetItem, line);

    }
    public static void Action_DisableProperty(Item targetItem, string prop_name)
    {
        Property property = targetItem.properties.Find(x => x.name == prop_name);
        property.Disable();
    }
    #endregion


    bool updateDescription = false;
    public Item describedItem;
    private static PropertyManager _instance;
    public static PropertyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PropertyManager>().GetComponent<PropertyManager>();
            }

            return _instance;
        }
    }
}
