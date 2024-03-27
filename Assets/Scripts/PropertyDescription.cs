using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public class PropertyDescription
{
    public string name;
    public Item item;
    public WorldAction.Source source;
    public List<Property> properties;

    public static List<PropertyDescription> propDescriptions = new List<PropertyDescription>();

    public PropertyDescription(Item item) {
        name = item.debug_name;
        properties = new List<Property>();
        this.item = item;
    }

    public static void Add(Item item, Property prop, WorldAction.Source source, bool failed) {
        if (!prop.HasPart("description"))
            return;

        if ( source == WorldAction.Source.Event) {
            LOG($"[{prop.name}] source : {source.ToString()}", Color.magenta);
            ItemDescription.AddProperties("action", item, new List<Property>() { prop }, "list / definite / filter events");
        } else {
            LOG($"[{prop.name}] source : {source.ToString()}", Color.yellow);
            string opts = failed ? "list / definite / start:can't, " : "list / definite";
            ItemDescription.AddProperties("action", item, new List<Property>() { prop }, opts);
        }
        //ItemDescription.Add(prop, item);

        /*var propDescription = propDescriptions.Find(x=> x.item == item);

        if ( propDescription == null) {
            propDescription = new PropertyDescription(item);
            propDescription.source = source;
            propDescriptions.Add(propDescription);
        }

        prop.GetDescription();
        if ( propDescription.properties.Contains(prop)) {
            return;
        }
        propDescription.properties.Add(prop);*/

    }

    public static void Describe() {
        AvailableItems.UpdateItems();

        // removing updated items that are not around ( prop update from events )
        var outOfRangeProps = propDescriptions.FindAll(x => !AvailableItems.currItems.Contains(x.item));
        if (outOfRangeProps.Count == 0)
            LOG($"all property items are in range", Color.white);
        else
            LOG($"removing {outOfRangeProps.Count} out of range proeprty items", Color.white);
        foreach (var item in outOfRangeProps)
            LOG($"{item.name}", Color.grey);
        propDescriptions.RemoveAll(x => !AvailableItems.currItems.Contains(x.item));
        //

        for (int i = 0; i < propDescriptions.Count; i++) {
            var pD = propDescriptions[i];
            LOG($"[CHECKING PROPS OF ITEM] : {pD.item.debug_name}", Color.magenta);

            var description = $"";
            var targetProps = pD.properties;

            // removing unchanged props
            if ( pD.source == WorldAction.Source.Event) {
                LOG($"[FROM EVENT]", Color.white);
                for (int j = targetProps.Count - 1; j >= 0; j--) {
                    var prop = targetProps[j];
                    LOG($"[PROP]:{prop.name} / {prop.GetCurrentDescription()}", Color.cyan);
                    if (!DescribeInEvents(prop))
                        targetProps.RemoveAt(j);
                }
                if (targetProps.Count == 0) {
                    LOG("no props to describe", Color.red);
                    continue;
                }
            } else {
            }

            // describing all props
            for (int j = 0; j < targetProps.Count; j++) {
                var prop = targetProps[j];
                LOG($"describing : {prop.name}/{prop.GetCurrentDescription()}", Color.green);
                if (!prop.enabled) {
                    description += $"not {prop.GetCurrentDescription()}{TextUtils.GetCommas(j, targetProps.Count)} anymore";
                } else {
                    description += $"{prop.GetCurrentDescription()}{TextUtils.GetCommas(j, targetProps.Count)}";
                }
            }

            // write
            if (!string.IsNullOrEmpty(description))
                TextManager.Write($"{pD.item.GetText("the dog")} is {description}");
        }


        propDescriptions.Clear();
    }

    public static string GetDescription(Property prop) {
        return GetDescription(new List<Property> { prop });
    }
    public static string GetDescription(List<Property> props) {
        var str = "";
        for (int i = 0;i < props.Count;i++) {
            var prop = props[i];
            str += $"{prop.GetCurrentDescription()}{TextUtils.GetCommas(i, props.Count)}";
        }
        return str;
    }

    public static bool DescribeInEvents(Property prop) {

        if (!prop.enabled) {
            LOG($"Skipping : Disabled.", Color.gray);
            return false;
        }

            var type = prop.GetPart("description type").content;
        if (type.StartsWith("lerp")) {
            float lerp = prop.GetNumValue() / prop.GetNumValue("max");
            int i = 0;
            if (!int.TryParse(type.Substring(4), out i)) {
                Debug.LogError($"could't parse prop description type {type}");
                return false;
            }
            float t = (float)i / 10f;
            if (lerp >= t) {
                LOG($"[{type} current={lerp} / target={t} / value={prop.GetNumValue()}] Describing", Color.gray);
                return true;
            } else {
                LOG($"[{type} current={lerp} / target={t} / value={prop.GetNumValue()}] Skipping", Color.gray);
                return false;
            }
        }

        switch (type) {
            case "always":
                LOG($"[ALWAYS] Describing", Color.gray);
                return true;
            case "on change":
                if ( prop.HasPart("changed")) {
                    LOG($"[ON CHANGE] Describing", Color.gray);
                    prop.RemovePart("changed");
                    return true;
                }
                LOG($"[ON CHANGE] Skipping ( hasn't changed )", Color.gray);
                return false;
            default:
                LOG($"[NO TYPE] Describing", Color.gray);
                return true;
        }
    }

    public static string log;
    public static void LOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
}
