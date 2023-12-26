using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PropertyDescription
{
    public string name;
    public Item item;
    public List<Property> properties;

    public static List<PropertyDescription> propDescriptions = new List<PropertyDescription>();

    public PropertyDescription(Item item) {
        name = item.debug_name;
        properties = new List<Property>();
        this.item = item;
    }

    public static void Add(Item item, Property prop) {
        
        if (!prop.HasPart("description")) {
            return;
        }

        var propDescription = propDescriptions.Find(x=> x.item == item);

        if ( propDescription == null) {
            propDescription = new PropertyDescription(item);
            propDescriptions.Add(propDescription);
        }

        if( propDescription.properties.Contains(prop)) {
            Debug.Log($"({item.debug_name}) already contains prop  {prop.name}");
            return;
        }
        propDescription.properties.Add(prop);

    }

    public static void Describe() {
        DebugManager.Instance.debug_PropertyDescriptions = new List<PropertyDescription>(propDescriptions);
        AvailableItems.UpdateItems();
        for (int i = 0; i < propDescriptions.Count; i++) {
            var pD = propDescriptions[i];
            // removing updated items that are not around ( prop update from events )
            if (!AvailableItems.currItems.Contains(pD.item)) {
                Debug.Log($"({pD.item.debug_name}) is'nt contianed in availble items");
                continue;
            }

            // getting item 
            var description = $"";

            var targetProps = pD.properties;
            // removing unchanged properties from description, but on ly from events ( need to get feed back when action )
            if ( WorldAction.current.source == WorldAction.Source.Event) {
                // get all props
                targetProps = pD.properties.FindAll(x => x.DescriptionChanged() && x.enabled);
            }
            // skipping if no changed props were found
            if (targetProps.Count == 0)
                continue;

            // describing all props
            for (int j = 0; j < targetProps.Count; j++) {
                var prop = targetProps[j];
                description += $"{prop.GetDescription()}{TextUtils.GetCommas(j, targetProps.Count)}";
            }
            
            // write
            TextManager.Write($"{pD.item.GetText("the dog")} is {description}");
        }


        propDescriptions.Clear();
    }
}
