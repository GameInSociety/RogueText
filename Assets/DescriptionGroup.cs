using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DescriptionGroup {
    public int dataIndex = 0;
    public List<Item> items;

    public Item first => items[0];

    public DescriptionGroup(List<Item> items) {
        this.items = items;
    }

    public void splitSpecs() {

        

    }

    public string getDescription(bool differenciate) {

        if (differenciate) {
            if (first.HasInfo("dif")) {

                // check if FIRST spec match

                // then, check additional specs from other items

                if (items.Count == 1) {
                    // normal description
                    return $"{first.getText("a special dog")}";
                } else {
                    if (Item.allSpecMatch(items)) {
                        return $"{items.Count} {first.getText("special dogs")}";
                    }

                    string str = "";
                    for (int i = 0; i < items.Count; i++) {
                        if (i > 0)
                            str += $"and {items[i].getSpec(1).GetDisplayValue}";
                        else
                            str += $"{items[i].getText("a special dog")}";
                    }
                    return str;
                }
            }
        }

        if (items.Count > 1)
            return $"{items.Count} {first.getText("dogs")}";
        else {
            if (first.HasInfo("dif"))
                return $"{first.getText("a special dog")}";
            else
                return $"{first.getText("a dog")}";
        }
    }

    public struct Paramereters {
        public bool groupSimilar;

        public Paramereters(bool groupSimilar) {
            this.groupSimilar = groupSimilar;
        }
    }

    public static string NewDescription(List<Item> items, bool differenciate = false) {

        // ici on va essayer de distinguer 
        // a white road on the left 
        // a white road on the right
        // a grey field in front of you
        // = >
        // a white road on your left and right
        // a grey field in front of you

        var groups = new List<DescriptionGroup>();
        foreach (var item in items) {
            var group = groups.Find(x => x.items[0].dataIndex == item.dataIndex );
            if (group == null) {
                group = new DescriptionGroup(new List<Item>());
                groups.Add(group);
            }
            group.items.Add(item);
        }

        foreach (var item in groups) {
            DebugManager.Instance.descriptionGroups.Add(item);
        }

        string str = "";
        for (int i = 0; i < groups.Count; ++i) {
            str += groups[i].getDescription(differenciate);
            str += TextUtils.GetLink(i, groups.Count);
        }
        Debug.Log("new item description : " + str);

        return str;
    }
}
