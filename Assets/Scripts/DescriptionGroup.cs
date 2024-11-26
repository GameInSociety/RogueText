using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A description group is an assemble of items & properties, grouped by phrases.
/// </summary>
[System.Serializable]
public class DescriptionGroup {
    
    // id 
    public string id;
    // item slots
    public List<ItemSlot> slots;
    // overall properties 
    public List<Property> properties = new List<Property>();

    public DescriptionGroup (string id) {
        this.id = id;
        slots = new List<ItemSlot>();
    }

    // (Main) Get text from all the item slots.
    public string GetDescription()
    {
        Debug.Log($"Slots Count : {slots.Count}");
        // group properties
        /* SplitSlotByPropertyName();

         // merge groups with same properties
         MergeSlotsWithSameProperties();*/

        // Create description.
        var description = "";
        var tmpSlots = new List<ItemSlot>(slots);

        while (tmpSlots.Count > 0)
        {
            string phrase = Phrase.GetPhrase(tmpSlots, out tmpSlots);
            description += $"{phrase}";
        }

        var whole = $"{description}";
        return whole;
    }


    public ItemSlot GetSlot(Item item)
    {
        // find / create slot
        var slot = slots.Find(x => x.items.First().dataIndex == item.dataIndex);
        if (slot == null)
        {
            slot = new ItemSlot("all", item.dataIndex);
            slots.Add(slot);
        }
        return slot;
    }

    public void AddItem(Item item) {
        
        // Get slot
        var slot = GetSlot(item);

        // Add to available items because described.
        AvailableItems.Add("Described Items", item);

        // Add Item 
        slot.items.Add(item);

        // Adding constant properties ( "Open window", "Burning house" )
        var constant_props = item.props.FindAll(x => x.HasPart("description") && x.GetContent("description type") == "always");
        slot.props.AddRange(constant_props);

    }

    public void AddProperty(Item item, Property prop)
    {
        Debug.Log($"Adding Property {prop.name} of item {item.DebugName} to group {id}");
        // Get slot.
        var slot = GetSlot(item);
        slot.props.Add(prop);
    }


    void HandleProps() {

        // Remove doubles inside groups.
        foreach (var slot in slots) {
            for (int i = slot.props.Count-1; i >= 0; i--) {
                var prop = slot.props[i];
                var count = slot.props.FindAll(x => x.GetCurrentDescription() == prop.GetDisplayDescription()).Count;
                if ( count > 1) {
                    slot.props.RemoveAt(i);
                }
            }
        }

        // Find props common to each slots.
        var commonProps = new List<Property>();
        var allProps = new List<Property>();
        var tmpSlots = new List<ItemSlot>(slots);

        foreach (var slot in tmpSlots)
            allProps.AddRange(slot.props);

        for (int i = tmpSlots.Count-1; i >= 0; i--) {
            var slot = tmpSlots[i];
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
                    tmpSlots.RemoveAt(i);
                    TransferToGroup(prop, slot);
                }
            }
        }
    }

    void TransferToGroup(Property prop, ItemSlot slot) {
        /*var group = descriptionGroups.Find(x => x.name == prop.GetCurrentDescription());
        if (group == null) {
            group = new DescriptionGroup(prop.GetCurrentDescription(), "list");
            group.properties.Add(prop);
            descriptionGroups.Add(group);
            group.slots.Add(slot);
        } else {
            group.slots.Add(slot);
        }*/
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
        // ?
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
