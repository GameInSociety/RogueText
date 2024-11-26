using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public class DescriptionManager : MonoBehaviour
{
    // (Debug) The previous described groups
    public List<DescriptionGroup> archive = new List<DescriptionGroup>();
    // The next described groups
    public List<DescriptionGroup> descriptionGroups = new List<DescriptionGroup>();
    // ??
    public List<int> describedItems = new List<int>();

    #region description
    // Display text of all groups
    public void StartDescription()
    {
        // (Debug) Update map display.
        MapTexture.Instance.DisplayMap();


        for (int i = 0; i < descriptionGroups.Count; i++)
        {
            //descriptionGroups[i].HandleProps();
        }
        descriptionGroups.RemoveAll(x => x.slots.Count == 0);

        // Display all item & property descriptions
        foreach (var group in descriptionGroups)
        {
            var des = group.GetDescription();
            des = des.Trim('\n');
            TextManager.Write($"{des}\n");
        }

        // (Debug) Add current groups to storage to look into in debug.
        archive.AddRange(descriptionGroups);

        // clear all 
        descriptionGroups.Clear();
        describedItems.Clear();
    }
    #endregion

    #region items & properties
    public void Add(string groupID, List<Item> items)
    {
        // Find item description
        var dGroup = descriptionGroups.Find(x => x.id == groupID);
        if (dGroup == null)
        {
            dGroup = new DescriptionGroup(groupID);
            descriptionGroups.Add(dGroup);
        }

        foreach (var item in items)
        {
            dGroup.AddItem(item);
        }
    }
    public void Add(string groupID, Item item)
    {
        Add(groupID, new List<Item>() { item });
    }
    public void Add(string groupID, Item item, List<Property> props)
    {
        if (props == null || props != null & props.Count == 0)
        {
            Debug.LogError($"({groupID}) Item {item.DebugName} has no property");
            return;
        }

        var group = GetGroup(groupID);
        foreach (var prop in props)
        {
            group.AddProperty(item, prop);
        }

    }
    public void Add(string groupID, Item item, Property prop)
    {
        Add(groupID, item, new List<Property>() { prop });
    }
    #endregion

    public DescriptionGroup GetGroup(string groupID)
    {
        // find /create item description
        var group = descriptionGroups.Find(x => x.id == groupID);
        if (group == null)
        {
            group = new DescriptionGroup(groupID);
            descriptionGroups.Add(group);
        }
        return group;
    }


    // has the game something to say ?
    public bool DescriptionPending()
    {
        return descriptionGroups.Count > 0;
    }

    /// <summary>
    /// SINGLETON
    /// </summary>
    public static DescriptionManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
