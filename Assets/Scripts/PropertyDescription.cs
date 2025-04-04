using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public class PropertyDescription
{
    public static void Add(Item item, Property prop, Sequence.Source source, bool failed) {
        if (!prop.HasPart("description"))
            return;

        if ( source == Sequence.Source.Event) {
            if (!prop.CanBeDescribed())
                return;
            //ItemDescription.AddProperties($"Event Properties ({item._debugName})", item, new List<Property>() { prop }, "list / definite / filter events");
        } else {
            string opts = failed ? "list / definite / start:can't, " : "list / definite";
            var descriptionGroupId = $"Player Action Properties ({item._debugName})";
            DescriptionManager.Instance.AddProperty(descriptionGroupId, item, prop);
        }

    }

    public static string GetDescription(Property prop) {
        return GetDescription(new List<Property> { prop });
    }
    public static string GetDescription(List<Property> props) {
        var str = "";
        for (int i = 0;i < props.Count;i++) {
            var prop = props[i];
            str += $"{prop.GetDescription()}{TextUtils.GetCommas(i, props.Count)}";
        }
        return str;
    }

    public static string log;
    public static void LOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
}
