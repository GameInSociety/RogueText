using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A Description has a list of description slot group, and handles splitting and cleaning the description
/// </summary>
[System.Serializable]
public class Description {
    // Context of the description (ex:"Treasure Description", "Enemy movement")
    // That way, all of the items are not described all together in a big blob, but rather with each a different context.
    public string id;
    // The output of all the slot groups.
    public List<ItemDescription> itemDescriptions;
    // Group that stores all of the comming items & props, that is used later by splitting and cleaning.
    public ItemDescription itemDescription_Input;

    // Parametre ajouté pour la description de Children items. Pas ouf, mais faut avancer. 
    public bool split = false;

    public Description (string id) {
        this.id = id;
        itemDescriptions = new List<ItemDescription>();
        itemDescription_Input = new ItemDescription("All");
    }

    public string GetDescription() {
        string description = "";
        var tmp_ItemDescriptions = new List<ItemDescription>(itemDescriptions);
        while (tmp_ItemDescriptions.Count > 0) {
            // The amount of slots used in the Phrase is random, so the slots we receive are different.
            string phrase = Phrase.GetPhrase(tmp_ItemDescriptions, out tmp_ItemDescriptions);
            description += $"{phrase}\n";
        }
        return description;
    }

    public void AddItem(Item item) {

        // Add Item to input group
        itemDescription_Input.AddInfo(item);

        // Adding constant properties ( "Open window", "Burning house" )
        var constant_props = item.props.FindAll(x => x.HasPart("description") && x.GetContent("description type") == "always");
        foreach (var prop in constant_props)
            AddProperty(item, prop);

        // Add to available items because described.
        AvailableItems.Add("Described Items", item);
    }

    public void AddProperty(Item item, Property prop)
    {
        // Add property to slot with same item.
        var targetInfo = itemDescription_Input._infos.Find(x=>x._item.id == item.id);
        targetInfo.props.Add(prop);
    }

    public bool ShowChildrenItems() {
        return itemDescriptions.Count == 1 && itemDescriptions.First()._infos.Count == 1
            && itemDescriptions.First()._infos.First()._item.HasVisibleItems();
    }

    public void HandleNewItems() {
        if (itemDescriptions.Count == 1 && itemDescriptions.First()._infos.Count == 1) {
            Debug.Log($"SEULEMENT UN OBJET DANS LA DESCRIPTION");
            var sampleItem = itemDescriptions.First()._infos.First()._item;
        } else {
            Debug.Log($"PLUSIEURS DANS LA DESCRIPTION");
        }
    }

    public void TrySplit() {

        // Split the items by name/nature
        itemDescriptions = itemDescription_Input.SplitByName();

        if (itemDescriptions.Count > 1)
            SplitByItemNature();
        else
            SplitByProperty();

        split = true;
    }

    public void SplitByItemNature() {
        // If the items of description are different, only clean properties and group items.
        // Remove all properties that are not share with all the slots
        foreach (var group in itemDescriptions)
            group.CleanProperties();
    }

    public void SplitByProperty() {
        var itemDescription = itemDescriptions.First();
        // If the items are the same, differenciate them by property.
        // First, single out slots with unique properties (w/additional or different types)
        // Then, try to split the group by properties.
        // Setting if every property is shared w/ the rest of the group
        itemDescription.SetPropertyLinks();

        // Get the property with which the items are going to be split
        // (ex: by material =>  2 wooden, 3 brick)
        var splitProp = itemDescription.GetPropertyForSplit();

        // If a property was found, clean & split/
        if (!string.IsNullOrEmpty(splitProp)) {
            // Delete all non splited props
            itemDescription.CleanUselessProperties(splitProp);
            // Split group with target property
            var splitGroups = itemDescription.SplitByProperty(splitProp);
            itemDescriptions = splitGroups;
        }
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
