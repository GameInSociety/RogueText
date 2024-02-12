using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

        prop.GetDescription();
        if ( propDescription.properties.Contains(prop)) {
            Debug.Log($"({item.debug_name}) already contains prop  {prop.name}");
            return;
        }
        propDescription.properties.Add(prop);

    }

    public static void Describe() {
        AvailableItems.UpdateItems();
            // removing updated items that are not around ( prop update from events )
        propDescriptions.RemoveAll(x => !AvailableItems.currItems.Contains(x.item));

        DebugManager.Instance.debug_PropertyDescriptions = new List<PropertyDescription>(propDescriptions);

        for (int i = 0; i < propDescriptions.Count; i++) {
            var pD = propDescriptions[i];

            // getting item 
            var description = $"";

            var targetProps = pD.properties;

            // skipping if no changed props were found
            if (targetProps.Count == 0) {
                Debug.Log($"no properties to describe in item {pD.item.debug_name}");
                continue;
            }

            // removing unchanged props
            targetProps = targetProps.FindAll(x => DescribeInEvents(x));
            // describing all props
            for (int j = 0; j < targetProps.Count; j++) {
                var prop = targetProps[j];
                prop.RemovePart("changed");
                description += $"{prop.GetDescription()}{TextUtils.GetCommas(j, targetProps.Count)}";
            }

            // write
            if (!string.IsNullOrEmpty(description))
                TextManager.Write($"{pD.item.GetText("the dog")} is {description}");
        }


        propDescriptions.Clear();
    }

    public static string GetDescription(List<Property> props) {
        var str = "";
        for (int i = 0;i < props.Count;i++) {
            var prop = props[i];
            str += $"{prop.GetDescription()}{TextUtils.GetCommas(i, props.Count)}";
        }
        return str;
    }

    public static bool DescribeInEvents(Property prop) {
        if (!prop.HasPart("description type") || !prop.HasPart("description"))
            return false;

        var type = prop.GetPart("description type").content;

        if (type.StartsWith("lerp")) {
            int i = 0;
            if (!int.TryParse(type.Substring(4), out i)) {
                Debug.LogError($"could't parse prop description type {type}");
                return false;
            }
            return prop.GetNumValue() >= i;
        }

        switch (type) {
            case "always":
                return true;
            case "on change":
                return prop.HasPart("changed");
            default:
                Debug.Log($"description type {type} for {prop.name} is under contextual conditions");
                return false;
        }
    }
}
