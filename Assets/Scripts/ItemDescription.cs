using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            showProps = false;
            propLayer = 0;
            if (propPart != null) {
                showProps = true;
                if (propPart.Contains('(')) {
                    bool b = int.TryParse(TextUtils.Extract('(', propPart), out propLayer);
                    if (b) Debug.Log($"item description has prop layer {propLayer}");
                }
            }

        }
        // will, if it can, mention the properties of the item
        public bool showProps;
        public int propLayer;
        // return between items
        public bool splitLines;
    }
    public Options options;

    [System.Serializable]
    public class Group {
        public Group(string name) {
            this.name = name;
            items = new List<Item>();
        }
        public string name;
        public List<Item> items;
        public Item first => items[0];

        public string GetText(Options options) {
            if (!first.HasVisibleProps())
                return items.Count == 1 ? $"{first.GetText("a dog")}" : $"multiple {items[0].GetText("dogs")}";

            string text = "";
            if (options.showProps) {
                text += $"{first.GetText("a special dog", options.propLayer)}";

                for (int itemIndex = 1; itemIndex < items.Count; itemIndex++) {
                    var item = items[itemIndex];

                    text += $"\nanother, ";

                    var visibleProps = item.GetVisibleProps(options.propLayer);

                    // remove props from previous items
                    for (int propertyIndex = 0; propertyIndex < visibleProps.Count; ++propertyIndex) {
                        var prop = visibleProps[propertyIndex];
                        if (items[itemIndex-1].HasProp(prop.name) && items[itemIndex-1].GetProp(prop.name).GetDescription() == prop.GetDescription())
                            continue;
                        text += $" {prop.GetDescription()}";
                    }

                    if (item.HasChildItems()) {
                        var childItems = item.GetChildItemsWithProp("visibility");
                        if (childItems.Count > 0)
                            text += $". there's {NewDescription(childItems)} in it.\n";
                    }

                    text += "\n";
                }
                return text;
            }
            return items.Count == 1 ? $"{first.GetText("a dog")}" : $"{items.Count} {items[0].GetText("dogs")}";
        }
    }

    public string GetDescription(List<Item> itms) {
        var groups = GetGroups(itms);
        var text = "";
        for (int i = 0; i < groups.Count; ++i) {
            var group = groups[i];
            if (options.splitLines)
                text += $"{group.GetText(options)}\n";
            else
                text += $"{group.GetText(options)}{TextUtils.GetCommas(i, groups.Count)}";
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
        // splitting groups.
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
