using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropertyManager : MonoBehaviour
{
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
                Action_CheckProperty();
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
            default:
                break;
        }
    }


    #region actions
    public void Action_ChangeProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);
        string targetProp = PlayerAction.GetCurrent.GetContent(0);
        string line = PlayerAction.GetCurrent.GetContent(1);
        Action_ChangeProperty(targetItem, targetProp, line);
    }
    public void Action_ChangeProperty(Item targetItem, string targetProp, string line)
    {

        // in the function type is not reffered, so go for part 0
        Property property = targetItem.GetProperty(targetProp);

        property.UpdateProperty(line);

        UpdateDescription();
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

        UpdateDescription();
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

        UpdateDescription();
    }

    /// <summary>
    /// CHECK PROPERTY ON TARGET ITEM
    /// </summary>
    public void Action_CheckProperty()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string property_line = PlayerAction.GetCurrent.GetContent(0);
        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasProperty(parts[0]))
        {
            PhraseKey.WriteHard("It's " + parts[0]);
            PlayerActionManager.Instance.BreakAction();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if ( property.GetInt() <= int.Parse(parts[1]))
            {
                PhraseKey.WriteHard("No " + property.name);
                PlayerActionManager.Instance.BreakAction();
                return;
            }
        }
        else
        {
            if (property.name != parts[1])
            {
                PhraseKey.WriteHard("It's not " + parts[1]);
                PlayerActionManager.Instance.BreakAction();
                return;
            }
        }
    }
    public void Action_CheckPropertyValue()
    {
        Item targetItem = InputInfo.Instance.GetItem(0);

        string propertyName = PlayerAction.GetCurrent.GetContent(0);

        Property property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0)
        {
            PhraseKey.WriteHard("No " + property.name);
            PlayerActionManager.Instance.BreakAction();
            return;
        }
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
    void UpdateDescription()
    {
        if ( updateDescription)
        {
            return;
        }

        updateDescription = true;
        CancelInvoke("UpdateDescriptionDelay");
        Invoke("UpdateDescriptionDelay", 0f);
    }

    void UpdateDescriptionDelay()
    {
        updateDescription = false;
        PhraseKey.WriteHard(InputInfo.Instance.GetItem(0).GetPropertiesDescription()); ;
    }

    public void UpdateProperties()
    {
        Tile.GetCurrent.UpdateProperties();
        Inventory.Instance.UpdateProperties();
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
