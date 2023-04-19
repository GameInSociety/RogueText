using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyManager : MonoBehaviour
{
     #region actions
    public void ChangeProperty()
    {
        ChangeProperty(PlayerAction.GetCurrent.GetContent(0));
    }
    public void ChangeProperty(string _property)
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        string[] newParts = _property.Split('/');

        if (!targetItem.HasProperty(newParts[0]))
        {
            // peut �tre la rajouter ? si elle n'existe pas, pour �tre sur �a peut �tre interessant
            Debug.LogError(targetItem.debug_name + " doesn't have the property " + newParts[0]);
            return;
        }

        Property property = targetItem.GetProperty(newParts[0]);

        property.UpdateParts(newParts);

        PhraseKey.WriteHard(property.GetDescription());
    }

    public void AddProperty()
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        string line = PlayerAction.GetCurrent.GetContent(0);
        string[] newParts = line.Split('/');

        if (targetItem.HasProperty(newParts[0]))
        {
            Debug.LogError(targetItem.debug_name + " ADDING PROPERTY : already has the property " + newParts[0]);
            return;
        }

        Property newProperty = targetItem.AddProperty(line);

        PhraseKey.WriteHard(newProperty.GetDescription());
        //
    }
    #endregion
 
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
