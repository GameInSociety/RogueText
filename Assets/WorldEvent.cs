using DG.Tweening;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class WorldEvent {

    public bool debug_selected = false;
    public static List<WorldEvent> worldEvents = new List<WorldEvent>();
    public class ItemGroup {
        public bool debug_selected = false;
        public class PropGroup {
            public bool debug_selected = false;
            public Property prop;
            public string sequence;
        }
        public Item item;
        public List<PropGroup> propGroups = new List<PropGroup>(); 
    }
    public string name;
    public bool timeEvent = false;
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    // Creation
    public WorldEvent(string name) {
        this.name = name;
    }
    public static WorldEvent CreateWorldEvent(string name) {
        var wE = new WorldEvent(name);
        worldEvents.Add(wE);
        return wE;
    }

    public static void Subscribe(string eventName,Item item, Property prop, string sequence) {
        var worldEvent = GetWorldEvent(eventName);
        var itemGroup = worldEvent.itemGroups.Find(x=> x.item == item);
        if ( itemGroup == null) {
            itemGroup = new ItemGroup();
            itemGroup.item = item;
            worldEvent.itemGroups.Add(itemGroup);
        }
        var propGroup = new ItemGroup.PropGroup();
        propGroup.prop = prop;
        propGroup.sequence = sequence;
        itemGroup.propGroups.Add(propGroup);
    }

    public static void Unsubscribe(string eventName, Item item, Property prop, string sequence) {
        var worldEvent = GetWorldEvent(eventName);
        var itemGroup = worldEvent.itemGroups.Find(x => x.item == item);
        if (itemGroup == null)
            return;
        var propGroup = itemGroup.propGroups.Find(x=>x.prop.name == prop.name);
        if ( propGroup == null)
            return;
        itemGroup.propGroups.Remove(propGroup);
    }

    public static void TriggerEvent(string name) {
        var worldEvent = GetWorldEvent(name);
        var targetActions = new List<WorldAction>();
        foreach (var itemGroup in worldEvent.itemGroups) {
            foreach (var propGroup in itemGroup.propGroups) {
                if (!propGroup.prop.enabled)
                    continue;
                var worldAction = new WorldAction(itemGroup.item, propGroup.sequence, $"Event:{name}"); ;
                targetActions.Add(worldAction);
            }
        }

        foreach (var item in targetActions) {
            item.StartSequence(WorldAction.Source.Event);
        }

    }

    public static WorldEvent GetWorldEvent(string name) {
        var worldEvent = worldEvents.Find(x => x.name == name);
        if (worldEvent == null)
            worldEvent = CreateWorldEvent(name);
        return worldEvent;
    }

    
    public static void RemoveWorldEventsWithItem(Item item) {
        foreach (var itDes in DescriptionManager.Instance.descriptionGroups) {
            itDes.slots.RemoveAll(x => x.items.Contains(item));
        }
    }
}
