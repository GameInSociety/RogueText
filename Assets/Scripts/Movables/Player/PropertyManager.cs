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
        if (InputInfo.Instance.items.Count < 2)
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            InputInfo.Instance.sustainVerb = true;
            InputInfo.Instance.sustainItem = true;
            CellEvent.Break("item_noSecondItem");
            return;
        }

        string prop_name = CellEvent.GetContent(0);

        if (!InputInfo.Instance.GetItem(1).HasProperty(prop_name))
        {
            TextManager.Write("&the dog (override)& has no " + prop_name, InputInfo.Instance.GetItem(1));
            CellEvent.Break();
            return;
        }

        if (InputInfo.Instance.GetItem(1).GetProperty(prop_name).HasInt()
            && InputInfo.Instance.GetItem(1).GetProperty(prop_name).GetInt() == 0)
        {
            TextManager.Write("No more " + prop_name + " in &the dog (override)&", InputInfo.Instance.GetItem(1));
            CellEvent.Break();
            return;
        }

        CellEvent.props.Add(InputInfo.Instance.GetItem(1).GetProperty(prop_name));
    }
    
    public static void Event_ChangeProperty()
    {

        Item targetItem;

        if (CellEvent.GetContent(0).StartsWith('*'))
        {
            string itemName = CellEvent.GetContent(0).Remove(0,1);
            targetItem = ItemManager.Instance.FindInWorld(itemName);

            if ( targetItem == null)
            {
                Debug.Log("no " + itemName);
                CellEvent.Break("No " + itemName);
                return;
            }

            CellEvent.RemoveContent(0);
        }
        else
        {
            targetItem = InputInfo.Instance.GetItem(0);
        }

        string targetProp = CellEvent.GetContent(0);
        string line = CellEvent.GetContent(1);

        Action_ChangeProperty(targetItem, targetProp, line);
    }
    public static void Action_ChangeProperty(Item targetItem, string targetProp, string line)
    {
        // in the function type is not reffered, so go for part 0
        Property property = targetItem.GetProperty(targetProp);

        property.Update(line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    ///  ADD PROPERTY
    /// </summary>
    public static void Event_AddProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = CellEvent.GetContent(0);

        Action_AddProperty(targetItem, line);
    }
    public static void Action_AddProperty(Item targetItem, string property_line)
    {
        Property newProperty = targetItem.CreateProperty(property_line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    /// REMOVE PROPERTY
    /// </summary>
    public static void Event_RemoveProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string propertyName = CellEvent.GetContent(0);

        Action_RemoveProperty(targetItem, propertyName);
    }
    public static void Action_RemoveProperty(Item targetItem, string line)
    {
        targetItem.DeleteProperty(line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    /// CHECK PROPERTY ON TARGET ITEM
    /// </summary>
    public static void Event_CheckProp()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string property_line = CellEvent.GetContent(0);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                CellEvent.Break();
                return;
            }

            return;
            
        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            TextManager.Write("It's not " + parts[0]);
            CellEvent.Break();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if ( property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                CellEvent.Break();
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
        Item targetItem = InputInfo.Instance.GetItem(0);

        string propertyName = CellEvent.GetContent(0);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            TextManager.Write("No " + property.name);
            CellEvent.Break();
            return;
        }

        CellEvent.props.Add(property);
    }

    /// <summary>
    /// ENABLE / DISABLE PROPERTIES
    /// </summary>
    public static void Event_EnableProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = CellEvent.GetContent(0);

        Action_EnableProperty(targetItem, line);

    }
    public static void Action_EnableProperty(Item targetItem, string prop_name)
    {
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
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = CellEvent.GetContent(0);
        Action_DisableProperty(targetItem, line);

    }
    public static void Action_DisableProperty(Item targetItem, string prop_name)
    {
        Property property = targetItem.properties.Find(x => x.name == prop_name);
        property.Disable();
    }
    #endregion

    
    public static void UpdateDescription(Item targetItem)
    {
        targetItem.WriteDescription();

        /*if ( updateDescription)
        {
            return;
        }

        describedItem = targetItem;

        updateDescription = true;
        CancelInvoke("UpdateDescriptionDelay");
        Invoke("UpdateDescriptionDelay", 0f);*/
    }

    /*void UpdateDescriptionDelay()
    {
        updateDescription = false;
        describedItem.WriteProperties();
    }*/

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
