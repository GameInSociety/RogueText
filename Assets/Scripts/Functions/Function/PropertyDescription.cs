using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class PropertyDescription {
    public static List<Item> list = new List<Item>();
    public static List<Item> debug_Items = new List<Item>();
    public static List<Property> debug_Property = new List<Property>();


    public static void Add(Item it, Property prop) {
        if (!list.Contains(it)) {
            list.Add(it);
        }
        prop.changed = true;
    }


    public static void Describe() {

        for (var itIndex = 0; itIndex < list.Count; itIndex++) {
            var it = list[itIndex];
            var props = it.props.FindAll(x => x.changed);

            if (itIndex > 0 && it.IsAChildItemOf(list[itIndex - 1])) {
                Debug.Log(it.debug_name + " is contained in " + list[itIndex - 1].debug_name);
                TextManager.Write("(d) its &dog& is ", it);
            } else {
                TextManager.Write("(d) &the dog& is ", it);
            }


            for (var i = 0; i < props.Count; i++) {
                var prop = props[i];
                TextManager.add($"{prop.GetDescription()} {TextUtils.GetLink(i, props.Count)}");
                prop.changed = false;
            }

        }

        list.Clear();
    }
}
