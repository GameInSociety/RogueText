using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class DescriptionGroup {
    
    public static List<DescriptionGroup> archive = new List<DescriptionGroup>();
    public static List<DescriptionGroup> descriptionGroups = new List<DescriptionGroup>();
    public static List<int> describedItems = new List<int>();

    /// <summary>
    /// parameters
    /// </summary>
    public string name;
    public Options options;
    public List<ItemSlot> slots;
    public List<Property> properties = new List<Property>();

    public DescriptionGroup (string name, string opts) {
        this.name = name;
        slots = new List<ItemSlot>();
        options = new Options(opts);
    }

    // for now the only 
    public static bool DescriptionPending() {
        return descriptionGroups.Count > 0;
    }

    // static describe
    public static void StartDescription() {
        MapTexture.Instance.DisplayMap();

        for (int i = 0; i < descriptionGroups.Count; i++) {
            descriptionGroups[i].HandleProps();
        }
        descriptionGroups.RemoveAll(x => x.slots.Count == 0);

        foreach (var group in descriptionGroups) {
            var des = group.GetDescription();
            des = des.Trim('\n');
            TextManager.Write($"{des}\n");
        }

        // debug
        archive.AddRange(descriptionGroups);

        descriptionGroups.Clear();
        describedItems.Clear();
    }

    public static string GetTestDescription() {
        MapTexture.Instance.DisplayMap();

        string text = "";
        foreach (var itDes in descriptionGroups) {
            var des = itDes.GetDescription();
            des = des.Trim('\n');
            text += des;
        }
        descriptionGroups.Clear();
        describedItems.Clear();
        return text;

    }

    public static DescriptionGroup AddItems(string descriptionName, List<Item> items, string opts = "") {
        // find item description
        var dGroup = descriptionGroups.Find(x=> x.name == descriptionName);
        if (dGroup == null) {
            dGroup = new DescriptionGroup(descriptionName, opts);
            descriptionGroups.Add(dGroup);
        }

        for (int i = 0; i < items.Count; ++i) {
            // set item
            var item = items[i];

            // add to available items because described
            AvailableItems.Add("Described Items", item);
            var slot = dGroup.AddItem(item);

            if (false /* in event : add all visible properties */ ) {
                // add properties that need to be described ( maybe event not always )
            } else {
                // add contstant props
                var constant_props = item.props.FindAll(x => x.HasPart("description") && x.GetContent("description type") == "always");
                if (constant_props != null) {
                    slot.props.AddRange(constant_props);
                }
            }
           
        }

        return dGroup;

    }
    public static void AddProperties(string descriptionName, Item item, List<Property> props, string opts = "") {

        // find /create item description
        var itDes = descriptionGroups.Find(x => x.name == descriptionName);
        if (itDes == null) {
            itDes = new DescriptionGroup(descriptionName, opts);
            descriptionGroups.Add(itDes);
        }
       
        
        // find / create slot
        /*var slot = group.itemSlots.Find(x=> x.items.First().dataIndex == item.dataIndex);
        if (slot == null) {
            slot = new ItemSlot("all");
            group.itemSlots.Add(slot);
            slot.items.Add(item);
        }

        foreach (var prop in props) {
            if (slot.props.Find(x => x.GetCurrentDescription() == prop.GetCurrentDescription()) == null) {
                slot.props.Add(prop);
            }
        }*/
    }

    void HandleProps() {

        // remove doubles inside groups 
        foreach (var slot in slots) {
            for (int i = slot.props.Count-1; i >= 0; i--) {
                var prop = slot.props[i];
                var count = slot.props.FindAll(x => x.GetCurrentDescription() == prop.GetDisplayDescription()).Count;
                if ( count > 1) {
                    slot.props.RemoveAt(i);
                }
            }
        }

        // find props common to each slots
        var commonProps = new List<Property>();
        var allProps = new List<Property>();
        foreach (var slot in slots)
            allProps.AddRange(slot.props);

        for (int i = slots.Count-1; i >= 0; i--) {
            var slot = slots[i];
            for (int j = slot.props.Count-1; j >= 0; j--) {
                var prop = slot.props[j];
                var count = allProps.FindAll(x => x.GetCurrentDescription() == prop.GetCurrentDescription()).Count;
                if (count > 1) {
                    if (commonProps.Find(x => x.GetCurrentDescription() == prop.GetCurrentDescription()) == null) {
                        commonProps.Add(prop);
                        Debug.Log($"Create called {prop.GetCurrentDescription()} a group here");
                    }
                    Debug.Log($"move {slot.items.First().DebugName} to other group");
                    slot.props.RemoveAt(j);
                    slots.RemoveAt(i);
                    TransferToGroup(prop, slot);
                }
            }
        }
    }

    void TransferToGroup(Property prop, ItemSlot slot) {
        var group = descriptionGroups.Find(x => x.name == prop.GetCurrentDescription());
        if (group == null) {
            group = new DescriptionGroup(prop.GetCurrentDescription(), "list");
            group.properties.Add(prop);
            descriptionGroups.Add(group);
            group.slots.Add(slot);
        } else {
            group.slots.Add(slot);
        }



    }

    public ItemSlot AddItem(Item item) {
        var slot = slots.Find(x => x.dataIndex == item.dataIndex);
        if (slot == null) {
            slot = new ItemSlot($"{item.DebugName}", item.dataIndex);
            slots.Add(slot);
        }
        slot.items.Add(item);
        return slot;
    }

    public string GetDescription() {


        // group properties
       /* SplitSlotByPropertyName();

        // merge groups with same properties
        MergeSlotsWithSameProperties();*/

        var description = "";
        var tmp_slots = new List<ItemSlot>();


        if ( properties.Count > 0 ) {
            slots.First().props.InsertRange(0, properties);

        }

        foreach (var slot in slots) {
            tmp_slots.Add(slot);
        }
        while (tmp_slots.Count > 0) {
            string phrase = Phrase.GetPhrase(tmp_slots, out tmp_slots, options);
            description += $"{phrase}";
        }
        var whole = $"{options.start}{description}";
        return whole;
    }

    private void SplitSlotByPropertyName() {
        /*foreach (var slot in slots) {
            var confName = "";
            var newSlots = new List<ItemSlot>();
            foreach (var slot in slots) {
                foreach (var prop in slot.props) {
                    var similarProp = slot.props.Find(x => x.name == prop.name && x.GetCurrentDescription() != prop.GetCurrentDescription());
                    if (similarProp != null) {
                        // delete all properties that don't have the same name
                        confName = similarProp.name;
                        break;
                    }
                }
                if (confName != "") {
                    slot.props.RemoveAll(x => x.name != confName);
                    var d = slot.props.Select(x => x.GetCurrentDescription()).Distinct();
                    foreach (var s in d) {
                        var newSlot = new ItemSlot(s);
                        newSlot.items.AddRange(slot.items.FindAll(x => x.GetVisibleProps().Find(p => p.GetCurrentDescription() == s) != null));
                        newSlot.props.Add(slot.props.Find(x => x.GetCurrentDescription() == s));
                        newSlots.Add(newSlot);
                    }
                }
            }
            if (newSlots.Count > 0) {
                slot.itemSlots.RemoveAt(0);
                slot.itemSlots.AddRange(newSlots);
            }
        }*/
    }

    void MergeSlotsWithSameProperties() {
        /*var allSlots = new List<ItemSlot>();
        foreach (var group in slots)
            allSlots.AddRange(group.itemSlots);

        for (int i = 0; i < slots.Count; i++) {
            var baseGroup = slots[i];
            for (int j = 0; j < baseGroup.itemSlots.Count; j++) {
                var baseSlot = baseGroup.itemSlots[j];
                if (baseSlot.props.Count == 0) continue;
                var sameSlots = SlotsWithSameProperties(baseSlot, allSlots);
                if (sameSlots.Count > 0) {
                    foreach (var slt in sameSlots) {
                        var targetGroup = slots.Find(x => x.itemSlots.Find(s => s == slt) != null);
                        slt.props.Clear();
                        baseGroup.itemSlots.Add(slt);
                        targetGroup.itemSlots.RemoveAll(x=> sameSlots.Find(s=> s == slt) != null);
                    }
                    // setting base slot to last so the properties appear last
                    baseGroup.itemSlots.Add(baseSlot);
                    baseGroup.itemSlots.RemoveAt(0);
                    options.groupedSlots = true;
                }
            }
        }*/
    }
    List<ItemSlot> SlotsWithSameProperties(ItemSlot baseSlot, List<ItemSlot> slots) {
        var result = new List<ItemSlot>();
        foreach (var slot in slots) {
            if (baseSlot == slot) continue;
            if ( baseSlot.props.Count == slot.props.Count) {
                bool same = true;
                for (int i = 0; i < baseSlot.props.Count; i++) {
                    if (baseSlot.props[i].GetCurrentDescription() != slot.props[i].GetCurrentDescription())
                        same = false;
                }
                if (same)
                    result.Add(slot);
            }

        }
        return result;
    }

    public class Options {
        public string start;
        public bool definite;
        public bool list;
        public bool groupedSlots;
        public bool filterEvents;
        public Options(string txt) {

            if (string.IsNullOrEmpty(txt))
                return;

            var split = txt.Split('/');
            foreach (var _str in split) {
                string str = _str.Trim(' ');
                if (str.StartsWith("start"))
                    start = $"{str.Split(':')[1]} ";
                else {
                    switch (str) {
                        case "definite":
                            definite = true;
                            break;
                        case "list":
                            list = true;
                            break;
                        case "filter events":
                            filterEvents = true;
                            break;
                        default:
                            Debug.LogError($"Item Description : Unrecognized Option ({str})");
                            break;
                    }
                }
            }
        }

        
    }

    public static string log = "";
}
