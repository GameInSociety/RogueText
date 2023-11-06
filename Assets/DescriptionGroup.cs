using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DescriptionGroup {
    public int dataIndex = 0;
    public List<Item> items;

    public Item first => items[0];

    public DescriptionGroup(List<Item> items) {
        this.items = items;
    }

    public string getDescription(bool groupSimilar) {

        if (first.HasInfo("dif") && !groupSimilar) {
            string str = "";
            for (int i = 0; i < items.Count; i++) {
                str += $"{items[i].getText("a special dog")}";
                str += TextUtils.GetLink(i, items.Count);
            }
            return str;
        }

        if ( items.Count > 1)
            return $"{items.Count} {first.getText("dogs")}";
        else {
            if (first.HasInfo("dif")) {
                return $"{first.getText("a special dog")}";
            } else {
                return $"{first.getText("a dog")}";
            }
        }
    }

    public static List<DescriptionGroup> GetGroups (List<Item> items) {
        var groups = new List<DescriptionGroup>();
        foreach (var item in items) {
            var group = groups.Find(x => x.items[0].dataIndex == item.dataIndex);
            if (group == null) {
                group = new DescriptionGroup(new List<Item>());
                groups.Add(group);
            }
            group.items.Add(item);
        }
        return groups;
    }
}
