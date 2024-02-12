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
using static UnityEngine.ParticleSystem;

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

    public static string DetailedDescription(List<Item> its, string filters = "") {

        var baseProps = its.First().GetVisibleProps(filters);

        if  (its.Count == 1) {
            return $"{its.First().GetText("a lone dog")}, {PropertyDescription.GetDescription(its.First().GetVisibleProps(filters))}";
        }

        var description = $"multiple {its.First().GetText("lone dogs")}";

        for (var i = 0;i < its.Count; ++i) {

            var item = its[i];
            var newProps = new List<Property>();
            foreach (var prop in item.GetVisibleProps(filters)) {
                if ( baseProps.Find(x=> x.GetDescription() != prop.GetDescription()) != null) {
                    //Debug.Log($"describing new prop : {prop.name} / {prop.GetDescription()}");
                    newProps.Add(prop);
                }
            }
            if (newProps.Count > 0) {
                description += $"\nsome, {PropertyDescription.GetDescription(newProps)}";
                baseProps.AddRange(newProps);
            }
            
            if (item.HasChildItems()) {
                description += $"with {ItemDescription.NewDescription(item.GetChildItems())}";
            }
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
        public enum Type {
            Alone,
            DescribeAll,
        }

       
        private string SimpleDescription(List<Item> its) {
            return its.Count == 1 ? $"{first.GetText("a dog")}" : $"some {first.GetText("dogs")}";
        }

        public string GetText(Options options) {
            var text = $"";
            return DetailedDescription(items);
            List<Item> its = new List<Item>(items);

            /*bool displayedFirstWord = false;
            int index = 0;
            int l = its.Count;
            for (int i = its.Count-1; i >= 0; i--) {
                var item = its[i];
                if (Discern(item)) {
                    //text += displayedFirstWord ? $"{item.GetText("special")}" : $"{item.GetText("a special dog")}";
                    text += $"{item.GetText("a special dog")}";
                    text += TextUtils.GetCommas(index, l);
                    ++index;
                    displayedFirstWord = true;
                    if (item.HasProp("clear") && item.HasChildItems())
                        text += $", with {NewDescription(item.GetChildItems())} on it.";
                    its.RemoveAt(i);
                }
            }*/

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

    public static List<Item> delayedItems = new List<Item>();
    public static void DelayDescription(Item item) {
        delayedItems.Add(item);
    }
    public static string NewDescription(List<Item> items, string filters = "always") {

        foreach (var item in items) {
            AddToDescription(item);
        }

        var newDescription = new ItemDescription();
        //newDescription.options = new Options(prms);
        var groups = GetGroups(items);
        var text = "";
        for (int i = 0; i < groups.Count; ++i) {
            var group = groups[i];
            if (i > 0) text += '\n';
            //text += $"{group.GetText(options)}{TextUtils.GetCommas(i, groups.Count, false)}";
            text += $"{DetailedDescription(group.items, filters)}{TextUtils.GetCommas(i, groups.Count, false)}";
        }
        return text;
    }

    static List<Item> _items = new List<Item>();
    public static void AddToDescription(Item item) {
        if ( _items.Count == 0) {
            Debug.Log($"new description");
        }
        _items.Add(item);
    }

    public static void ResetDescription() {
        Debug.Log($"RESET");
        _items.Clear();
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
            var group = groups.Find(x => x.items.First().GetWord().GetText == items[i].GetWord().GetText);
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
