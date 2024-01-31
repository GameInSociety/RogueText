using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using TextSpeech;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemDescription {
    public struct Options {
        public Options(string str) {
            var parts = str.Split(", ").ToList();
            splitLines = parts.Contains("split lines");
            var propPart = parts.Find(x => x.StartsWith("show props"));
        }
        // will, if it can, mention the properties of the item
        // return between items
        public bool splitLines;
    }
    public Options options;

    public static string DetailedDescription(List<Item> its) {

        var props = its.First().GetAllVisibleProps();

        var description = $"they {PropertyDescription.GetDescription(props)}";

        foreach (var item in its) {
            var newProps = item.GetAllVisibleProps().FindAll(x => props.Find(y => y.GetDescription() != x.GetDescription()) == null);
            if (newProps.Count == 0)
                continue; 
            description += $"\nsome {PropertyDescription.GetDescription(newProps)}";
            props.AddRange(newProps);
        }

        return description;
    }


    [System.Serializable]
    public class Group {
        public Group(string name) {
            this.name = name;
            items = new List<Item>();
        }
        public string name;
        public List<Item> items;
        public Item first => items[0];

       
        private string SimpleDescription(List<Item> its) {
            return its.Count == 1 ? $"{first.GetText("a dog")}" : $"multiple {first.GetText("dogs")}";
        }

        public string GetText(Options options) {
            var text = $"";

            List<Item> its = new List<Item>(items);

            bool displayedFirstWord = false;
            int index = 0;
            int l = its.Count;
            for (int i = its.Count-1; i >= 0; i--) {
                var item = its[i];
                if (Discern(item)) {
                    Debug.Log($"discern : {item.debug_name}");
                    text += displayedFirstWord ? $"{item.GetText("special")}" : $"{item.GetText("a special dog")}";
                    text += TextUtils.GetCommas(index, l);
                    ++index;
                    displayedFirstWord = true;
                    if (item.HasProp("clear") && item.HasChildItems())
                        text += $", with {NewDescription(item.GetChildItems())} on it.";
                    its.RemoveAt(i);
                }
            }

            if ( its.Count > 0)
                text += $"{SimpleDescription(its)}";
            return  $"{text}";
        }

        // function that returns wether or not an itme has it's own line ( visible prop, visible child items etc... )
        bool Discern(Item item) {
            return
                item.HasVisibleProps()
                ||
                item.HasProp("clear") && item.HasChildItems();
        }

    }

    public string GetDescription(List<Item> itms) {
        var groups = GetGroups(itms);
        var text = "";
        for (int i = 0; i < groups.Count; ++i) {
            var group = groups[i];
            if (i > 0) text += '\n';
            text += $"{group.GetText(options)}{TextUtils.GetCommas(i, groups.Count, false)}";
        }
        return text;
    }

    public static string NewDescription(List<Item> items, string prms = "") {
        var newDescription = new ItemDescription();
        newDescription.options = new Options(prms);
        return newDescription.GetDescription(items);
    }

    #region utils
    [System.Serializable]
    public class DescriptionGroup_Debug {
        public DescriptionGroup_Debug(List<Group> groups) {
            this.groups = groups;
        }
        public List<Group> groups = new List<Group>();
    }

    static List<Group> GetGroups(List<Item> items) {
        // splitting groups_deub.
        var groups = new List<Group>();
        for (int i = 0; i < items.Count; ++i) {
            var group = groups.Find(x => x.items.First().dataIndex == items[i].dataIndex);
            if (group == null) {
                group = new Group($"{items[i].debug_name}");
                groups.Add(group);
            }
            group.items.Add(items[i]);
        }
        DebugManager.Instance.debugDescriptions.Add(new DescriptionGroup_Debug(groups));
        return groups;
    }
    #endregion
}
