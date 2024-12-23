using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using UnityEditor.Search;
using UnityEngine;

public class DescriptionManager : MonoBehaviour
{
    /// <summary>
    /// SINGLETON
    /// </summary>
    public static DescriptionManager Instance;
    private void Awake() {
        Instance = this;
    }

    public ItemDescription input;

    // (Debug) The previous described groups
    public List<Description> archive = new List<Description>();
    // The next described groups
    public List<Description> descriptions = new List<Description>();
    // ??
    public List<int> describedItems = new List<int>();

    #region description
    // Display text of all groups
    public void StartDescription()
    {
        // (Debug) Update map display.
        MapTexture.Instance.DisplayMap();

        // Only split the 
        foreach (var description in descriptions)
            description.TrySplit();

        CheckForChildrenItemsDescription();

        // Display all item & property descriptions
        foreach (var description in this.descriptions) {
            var des = description.GetDescription();
            des = des.Trim('\n');
            TextManager.Write($"{des}\n");
        }

        // (Debug) Add current groups to storage to look into in debug.
        archive.AddRange(this.descriptions);

        // clear all 
        this.descriptions.Clear();
        describedItems.Clear();
    }
    #endregion

    void CheckForChildrenItemsDescription() {
        // Check for Descriptions that allow the description of children items
        var parentItems = new List<Item>();
        foreach (var description in descriptions) {
            if (description.ShowChildrenItems()) {
                var containerItem = description.itemDescriptions.First()._infos.First()._item;
                parentItems.Add(containerItem);
            }
        }

        // Add Children Items for all parent items.
        foreach (var item in parentItems)
            AddItem($"{item._debugName} ({item.id}) (Child Items)", item.GetVisibleItems());

        // init & split the new descriptions without splitting the previous ones.
        foreach (var description in descriptions.FindAll(x => !x.split))
            description.TrySplit();

    }

    #region items & properties
    /// <summary>
    /// Add Item to be described
    /// </summary>
    /// <param name="descriptionID"></param>
    /// <param name="items"></param>
    public void AddItem(string descriptionID, List<Item> items)
    {
        // Find item description
        var description = GetDescription(descriptionID);

        foreach (var item in items)
        {
            description.AddItem(item);
        }
    }
    public void AddItem(string descriptionID, Item item)
    {
        AddItem(descriptionID, new List<Item>() { item });
    }

    /// <summary>
    /// Add Property to be described
    /// </summary>
    /// <param name="descriptionID"></param>
    /// <param name="item"></param>
    /// <param name="props"></param>
    public void AddProperty(string descriptionID, Item item, List<Property> props)
    {
        if (props == null || props != null & props.Count == 0) 
        {
            Debug.LogError($"({descriptionID}) Item {item.DebugName} has no property");
            return;
        }

        var description = GetDescription(descriptionID);
        foreach (var prop in props)
        {
            description.AddProperty(item, prop);
        }

    }
    public void AddProperty(string groupID, Item item, Property prop)
    {
        AddProperty(groupID, item, new List<Property>() { prop });
    }
    #endregion

    public Description GetDescription(string id)
    {
        // find /create item description
        var description = descriptions.Find(x => x.id == id);
        if (description == null)
        {
            description = new Description(id);
            descriptions.Add(description);
        }
        return description;
    }


    // has the game something to say ?
    public bool DescriptionPending()
    {
        return descriptions.Count > 0;
    }
}
