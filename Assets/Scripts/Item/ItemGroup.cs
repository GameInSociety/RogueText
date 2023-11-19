using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;
using Unity.Android.Types;
using System.Net.Configuration;
using static UnityEngine.ParticleSystem;
using System.Runtime.InteropServices;
using UnityEditor;

[System.Serializable]
public class ItemGroup {
    public string debug_name;
    public ItemGroup(int index, Word.Number num) {
        this.index = index;
        this.num = num;
    }
    public int index;
    public Word.Number num;
    public List<Item> items = new List<Item>();
    public List<ItemGroup> childGroups = new List<ItemGroup>();
    public Item first => items[0];
    public string text;

    [System.Serializable]
    public class DescriptionParameters {
        public bool groupItems = false;
        public string debug_description;
    }

    public bool tryInit() {
        if (Regex.IsMatch(text, @$"\ball\b")) {
            // do nothing, not deleting any other item in the group
        }

        // check some 
        if (Regex.IsMatch(text, @$"\bsome\b")) {
            Debug.Log("found some in " + first.debug_name);
            float f = (float)items.Count / 2;
            int half = (int)Mathf.Clamp(Mathf.Round(f), 1, items.Count);
            if (half < items.Count)
                items.RemoveRange(half, items.Count - half);
        }

        // check for numerics
        foreach (var str in text.Split(' ')) {
            int count = 0;
            if (str.All(char.IsDigit) && int.TryParse(str, out count)) {
                Debug.Log($"found number {count} in item group of {first.debug_name}");
                for (int i = count; i < items.Count; i++)
                    items.RemoveAt(i);
                break;
            }
        }

        if (num == Word.Number.Singular) {

            if (first.HasProp("dif") && !Item.AllSpecMatch(items)) {
                // check for distinct item
                Item specificItem = getSpecific();
                if (specificItem != null)
                    items.RemoveAll(x => x != specificItem);
                else
                    return false;

            } else {
                if (items.Count > 1)
                    items.RemoveRange(1, items.Count - 1);
            }
        }

        return true;
    }

    public bool ItemAreTheSame() {
        return !first.HasProp("dif") && Item.AllSpecMatch(items);
    }

    Item getSpecific() {
        // try to find an item spec in the input
        assignOrdinates();
        Spec spec = null;
        string text = ItemParser.GetCurrent.lastInput;
        return items.Find(x => x.textHasSpecs(text, out spec));
    }
    private void assignOrdinates() {
        for (int i = 0; i < items.Count; i++) {
            string ordinal = GetOrdinal(i);
            Spec ordinalSpec = items[i].getKeyInfo("ordinal");
            if (ordinalSpec != null) {
                ordinalSpec.searchValue = ordinal;
                ordinalSpec.displayValue = ordinal;
            } else
                items[i].setSpec(ordinal, ordinal, "ordinal", false);
        }
    }

    public string GetOrdinal(int i) {
        var ordinals = new string[10]
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
            "eighth",
            "ninth",
            "tenth",
        };
        return ordinals[i];
    }


    public static List<ItemGroup> GetGroups(List<Item> targetItems, string filter = "") {
        var groups = new List<ItemGroup>();
        int index = 0;
        foreach (var item in targetItems) {
            Word.Number num = Word.Number.None;
            index = string.IsNullOrEmpty(filter) ? item.GetData().index : item.getIndexInText(filter, out num);
            if (index >= 0) {
                var itemgroup = groups.Find(x => x.index == index);
                if (itemgroup == null) {
                    itemgroup = new ItemGroup(index, num);
                    itemgroup.debug_name = item.debug_name;
                    itemgroup.text = filter;
                    groups.Add(itemgroup);
                }
                itemgroup.items.Add(item);
            }
        }

        groups.Sort((a, b) => a.index.CompareTo(b.index));
        return groups;
    }
    public List<ItemGroup> SplitBySpecs(int splitLayer) {
        var groups = new List<ItemGroup>();
        foreach (var item in items) {
            Word.Number num = Word.Number.None;
            var itemgroup = groups.Find(x => x.index == index && x.first.getSpec(splitLayer).displayValue == item.getSpec(splitLayer).displayValue);
            if (itemgroup == null) {
                itemgroup = new ItemGroup(index, num);
                itemgroup.debug_name = item.getSpec(splitLayer).displayValue;
                groups.Add(itemgroup);
            }
            itemgroup.items.Add(item);
        }
        return groups;
    }

    public string GetDescription(bool detailItems= false) {

        // aller à l'envers.
        // toujours partir de la liste d'objet
        // et s'il s'avère que les objects partage une prop, et n'ont pas d'objets visibles,
        // les regrouper.

        // if only one item, always detailed description
        // "special dogs" will display properties descriptions, so nothing if there's none.
        // NOTE PEUT ËTRE : "very special dog" met des descriptions plus poussées ?
        if (items.Count == 1)
            return $"{first.GetText("a special dog")}";

        if ( detailItems) {
            // if the tmpItems have all the same specs, no need to list them
            // ( a white house and a white house ) becomes (2 white houses)
            if (Item.AllSpecMatch(items)) // peut être là rajouter le spec layer ?
                return $"{items.Count} {first.GetText("special dogs")}";

            int specLayer = 0; // 1 si on va chercher plus loin etc.. mais pas sûr que ça va aller plus loin
            // spliting the group by other groups with specs
            childGroups = SplitBySpecs(0);
                string str = "";
            foreach (var childGroup in childGroups) {
                // a road on the right, on the left and on the 
                for (int i = 0; i < childGroup.items.Count; i++) {
                    if (i > 0) 
                        str += $"and {childGroup.items[i].getSpec(1).GetDisplayValue}";
                    else
                        str += $"{childGroup.items[i].GetText("a special dog")}";

                    if (childGroup.items[i].HasChildItems()) {
                        var childItems = childGroup.items[i].GetChildItems("visibility", "1");
                        str += $"\nyou see {GetDescription(childItems, false)}.\n";
                    }
                }
            }

            return str;
        }

        // grouped tmpItems 
        // meaning => "3 houses" au lieu de "an ashy house, a big house and a brick house"
        if (items.Count > 1)
            return $"{items.Count} {first.GetText("dogs")}";
        else {
            if (first.HasProp("dif"))
                return $"{first.GetText("a special dog")}";
            else
                return $"{first.GetText("a dog")}";
        }
    }
    public static string GetDescription(List<Item> items, bool detailItems) {

        /// TEST DESCRIPTION surrounding tiles ( et autres à voir )

        var tmpItems = new List<Item>(items);

        var b = 0;

        var description = "";
        while (tmpItems.Count > 0) {
            
            // display next item
            Item item = tmpItems[0];


            if (item.HasChildItems() && item.GetChildItems("visibility", "1").Count > 0) {
                var childItems = item.GetChildItems("visibility", "1");
                description += $"you see {childItems.Count} {childItems[0].GetText("dogs")} in ";
            } else {

            }

            var phrase = $"{item.GetText("a special dog")}";
            description += $"{phrase}";
            tmpItems.RemoveAt(0);



            // ici, très loin, tu vois aussi des tours, des grands batiments des ponts etc...
            // tu check la visibility dans les directions


            // group similar tmpItems
            var similarItems = tmpItems.FindAll(x =>
            x.dataIndex == item.dataIndex &&
            x.getSpec(0).displayValue == item.getSpec(0).displayValue &&
            (!x.HasChildItems() ||
            (x.HasChildItems() && x.GetChildItems("visibility", "1").Count ==0)
            ));
            foreach (var sim in similarItems) {
                description += $" and {sim.getSpec(1).GetDisplayValue}";
                tmpItems.Remove(sim);
            }

            description += "\n";

            ++b;
            if (b > 10)
                return "broke";
        }
        return description;


        return GetDescription(GetGroups(tmpItems), detailItems);
    }
    public static string GetDescription(List<ItemGroup> groups, bool detailItems) {
        var text = "";
        for (int i = 0; i < groups.Count; i++) {
            text += groups[i].GetDescription(detailItems);
            text += TextUtils.GetLink(i, groups.Count);
        }
        return text;
    }
}