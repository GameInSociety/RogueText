using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public class PropertyManager : MonoBehaviour
{
    public List<Property> pendingProps = new List<Property>();
    public List<Item> pendingItems = new List<Item>();

    public delegate void OnEmptyValue();
    public OnEmptyValue onEmptyValue;

    private void Start()
    {
        // CENTRALISER LES ACTIONS !
        PlayerActionManager.onPlayerAction += HandleOnPlayerAction;
    }

    private void HandleOnPlayerAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.ChangeProp:
                Action_ChangeProperty();
                break;
            case PlayerAction.Type.AddProp:
                Action_AddProperty();
                break;
            case PlayerAction.Type.RemoveProp:
                Action_RemoveProperty();
                break;
            case PlayerAction.Type.CheckProp:
                Action_CheckProp();
                break;
            case PlayerAction.Type.CheckPropValue:
                Action_CheckPropertyValue();
                break;
            case PlayerAction.Type.EnableProp:
                Action_EnableProperty();
                break;
            case PlayerAction.Type.DisableProp:
                Action_DisableProperty();
                break;
            case PlayerAction.Type.RequireItemWithProp:
                RequireItemWithProp();
                break;
            default:
                break;
        }
    }


    #region actions
    void RequireItemWithProp()
    {
        if (InputInfo.Instance.items.Count < 2)
        {
            // this will display a phrase "what do you want to charge the flashlight with"
            // hence, sustain verb and all so input follows
            // but many actions require to sustain things
            // so until je trouve quelque chose de mieux, je le mets partout
            InputInfo.Instance.sustainVerb = true;
            InputInfo.Instance.sustainItem = true;
            PlayerActionManager.Instance.BreakAction("item_noSecondItem");
            return;
        }

        string prop_name = PlayerAction.GetCurrent.GetContent(0);

        if (!InputInfo.Instance.GetItem(1).HasProperty(prop_name))
        {
            TextManager.WritePhrase("&the dog (override)& has no " + prop_name, InputInfo.Instance.GetItem(1));
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        if (InputInfo.Instance.GetItem(1).GetProperty(prop_name).HasInt()
            && InputInfo.Instance.GetItem(1).GetProperty(prop_name).GetInt() == 0)
        {
            TextManager.WritePhrase("No more " + prop_name + " in &the dog (override)&", InputInfo.Instance.GetItem(1));
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        pendingProps.Add(InputInfo.Instance.GetItem(1).GetProperty(prop_name));
    }
    public void Action_ChangeProperty()
    {

        Item targetItem;

        if (PlayerAction.GetCurrent.GetContent(0).StartsWith('*'))
        {
            string itemName = PlayerAction.GetCurrent.GetContent(0).Remove(0,1);
            targetItem = ItemManager.Instance.FindInWorld(itemName);

            if ( targetItem == null)
            {
                Debug.Log("no " + itemName);
                PlayerActionManager.Instance.BreakAction("No " + itemName);
                return;
            }

            PlayerAction.GetCurrent.RemoveContent(0);
        }
        else
        {
            targetItem = InputInfo.Instance.GetItem(0);
        }

        string targetProp = PlayerAction.GetCurrent.GetContent(0);
        string line = PlayerAction.GetCurrent.GetContent(1);
        Action_ChangeProperty(targetItem, targetProp, line);
    }
    public void Action_ChangeProperty(Item targetItem, string targetProp, string line)
    {
        // in the function type is not reffered, so go for part 0
        Property property = targetItem.GetProperty(targetProp);

        property.Update(line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    ///  ADD PROPERTY
    /// </summary>
    public void Action_AddProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = PlayerAction.GetCurrent.GetContent(0);

        Action_AddProperty(targetItem, line);
    }
    public void Action_AddProperty(Item targetItem, string property_line)
    {
        Property newProperty = targetItem.CreateProperty(property_line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    /// REMOVE PROPERTY
    /// </summary>
    public void Action_RemoveProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string propertyName = PlayerAction.GetCurrent.GetContent(0);

        Action_RemoveProperty(targetItem, propertyName);
    }
    public void Action_RemoveProperty(Item targetItem, string line)
    {
        targetItem.DeleteProperty(line);

        UpdateDescription(targetItem);
    }

    /// <summary>
    /// CHECK PROPERTY ON TARGET ITEM
    /// </summary>
    public void Action_CheckProp()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string property_line = PlayerAction.GetCurrent.GetContent(0);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.WritePhrase("It's " + property_line);
                PlayerActionManager.Instance.BreakAction();
                return;
            }

            return;
            
        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            TextManager.WritePhrase("It's not " + parts[0]);
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if ( property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.WritePhrase("No " + property.name);
                PlayerActionManager.Instance.BreakAction();
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
    public void Action_CheckPropertyValue()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string propertyName = PlayerAction.GetCurrent.GetContent(0);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            TextManager.WritePhrase("No " + property.name);
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        pendingProps.Add(property);
    }

    /// <summary>
    /// ENABLE / DISABLE PROPERTIES
    /// </summary>
    public void Action_EnableProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = PlayerAction.GetCurrent.GetContent(0);

        Action_EnableProperty(targetItem, line);

    }
    public void Action_EnableProperty(Item targetItem, string prop_name)
    {
        Property property = targetItem.properties.Find(x => x.name == prop_name);

        if ( property == null)
        {
            Debug.LogError("ACTION_ENABLEPROPERY");
            Debug.LogError("did not find property : " + prop_name);
        }

        property.Enable();
    }
    public void Action_DisableProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string line = PlayerAction.GetCurrent.GetContent(0);
        Action_DisableProperty(targetItem, line);

    }
    public void Action_DisableProperty(Item targetItem, string prop_name)
    {
        Property property = targetItem.properties.Find(x => x.name == prop_name);
        property.Disable();
    }
    #endregion

    bool updateDescription = false;
    Item describedItem;
    void UpdateDescription(Item targetItem)
    {
        if ( updateDescription)
        {
            return;
        }

        describedItem = targetItem;

        updateDescription = true;
        CancelInvoke("UpdateDescriptionDelay");
        Invoke("UpdateDescriptionDelay", 0f);
    }

    void UpdateDescriptionDelay()
    {
        updateDescription = false;
        describedItem.WritePropertiesDescription();
    }

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
