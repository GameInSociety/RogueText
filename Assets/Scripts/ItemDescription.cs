using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public static List<string> links = new List<string>() {
        "near",
        "behind",
        "in front of",
        "next to",
        "sides",
        "between",
        "in the middle of ",
        "surrounding",
    };

    public static List<string> article_multiple = new List<string>() {
        "some",
        "multiple",
        "a few",
        "several",
    };

    public static List<string> phrase_forms = new List<string>() {
        "there's XXX LINK YYY",
        "XXX sit LINK YYY",
        "XXX stand LINK YYY"
    };

    public static string DescribeItemGroup(List<Item> its, string filters) {
        //var description = $"{its.First().GetText((its.Count ==1? "a lone dog": "lone dogs"))}";
        // debug range
        var single = $"{its.First().GetText("a lone dog")}";
        var multiple = $"{article_multiple[Random.Range(0, article_multiple.Count)]} {its.First().GetText("lone dogs")}";
        var item_text = its.Count == 1 ? single : multiple;
        //var description = $"({its.First().GetProp("range").GetNumValue()}) {item_text}";
        var description = $"{item_text}";

        // CHECK PROPS
        description += GetItemProps(its, filters);

        // CHECK CHILD ITEMS
        description += GetItemChildItems(its, filters);

        return description;
    }

    /*public static string DetailedDescription(List<Item> its, string filters, bool showChildItems) {
        // multiple items
        var description = "";

        bool oneItemPerProp = false;

        // CHECK PROPS
        description += GetItemProps(its, filters);

        // CHECK CHILD ITEMS
        description += GetItemChildItems(its, filters, showChildItems);

        // INSERT ITEM NAME
        if ((its.Count > 1 && oneItemPerProp) || its.Count == 1)
            description = description.Insert(0, $"{its.First().GetText("a lone dog")}");
        else {
            var vars = new List<string> {
                    "a handhul of",
                    "multiple",
                    "numerous",
                    its.Count.ToString()
                };
            description = description.Insert(0, $"{vars[Random.Range(0, vars.Count)]} {its.First().GetText("lone dogs")}");
        }

        return description;
    }*/

    public static string GetItemProps(List<Item> its, string filters) {
        var description = "";
        var propsHistory = new List<Property>();
        for (var i = 0; i < its.Count; ++i) {

            var item = its[i];
            var visibleProps = item.GetVisibleProps(filters);
            var l = visibleProps.Count;
            // item doesn't have anything to discent it self for, continuing
            if (l == 0 && !item.HasExposedItems())
                continue;


            for (var j = 0; j < l; ++j) {
                var prop = item.GetVisibleProps(filters)[j];
                var propCount = its.FindAll(x => x.props.FindLast(y => y.GetDescription() == prop.GetDescription()) != null).Count;
                // has prop already been described ?
                if (propsHistory.Find(x => x.GetDescription() == prop.GetDescription()) != null)
                    continue;

                if (j == 0) description += ", ";

                if (propCount > 1) {
                    description += $"\nsome, {PropertyDescription.GetDescription(prop)}";
                } else {
                    description += $" {PropertyDescription.GetDescription(prop)}{TextUtils.GetCommas(j,l)}";
                }

                propsHistory.Add(prop);
            }
            description += TextUtils.GetCommas(i, its.Count);
        }
        return description;
    }

    static string GetItemChildItems(List<Item> items, string filters) {
        var description = "";
        for (var i = 0; i < items.Count; ++i) {
            var item = items[i];
            if (item.HasExposedItems())
                description += $"\n{item.GetText("the dog")},has {DescribeItems(item.GetExposedItems())}";
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

    }

    public static string delayedSetup = "";
    public static List<Item> delayedItems = new List<Item>();
    public static void DelayDescription(string setup, List<Item> items) {
        if (!string.IsNullOrEmpty(setup)) delayedSetup = setup;
        delayedItems.AddRange(items);
    }
    public static void DelayDescription(string setup, Item item) {
        DelayDescription(setup, new List<Item>() { item });
    }
    public static string DescribeItems(Item item) {
        return DescribeItems(new List<Item> { item });
    }
    public static string DescribeItems(List<Item> items) {
        return DescribeItems(items, "always");
    }
    public static string DescribeItems(List<Item> items, string filters) {
        // group items
        var groups = GroupItems(items);
        // init the overall description
        var text = "";
        // loop throught groups
        while (groups.Count > 0) {

            var firstGroup = groups[Random.Range(0, groups.Count)];
            var items_1 = DescribeItemGroup(firstGroup.items, filters);
            groups.Remove(firstGroup);
            if (groups.Count == 0)
                return $"{items_1}";

            
            var secondGroup = groups[Random.Range(0, groups.Count)];
            var items_2 = DescribeItemGroup(secondGroup.items, filters);
            groups.Remove(secondGroup);
            if (groups.Count == 1) {
                items_1 += $" and {DescribeItemGroup(groups.First().items, filters)}";
                Debug.Log($"lone word");
                groups.Clear();
            }


            text += $"{items_1} {links[Random.Range(0, links.Count)]} {items_2}. ";
            if (Random.value > 0.7f)
                text += "\n";
        }
        return text;
    }

    static List<Group> GroupItems(List<Item> items) {
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
        return groups;
    }
}
