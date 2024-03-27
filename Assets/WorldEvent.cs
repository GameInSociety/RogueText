using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class WorldEvent {

    public static List<WorldEvent> worldEvents = new List<WorldEvent>();

    public WorldEvent(string name) {
        this.name = name;
    }

    public string name;
    public List<WorldAction> actions = new List<WorldAction>();
    
    public void AddWorldAction(WorldAction action) {
        actions.Add(action);
    }

    public static WorldEvent CreateWorldEvent(string name) {
        var wE = new WorldEvent(name);
        worldEvents.Add(wE);
        return wE;
    }


    public static void SubscribeItem(Item item, string content) {

        var split = content.Split('\n', 2);

        var eventName = split[0];
        var sequence = split[1];

        var worldEvent = GetWorldEvent(eventName);
        if ( worldEvent == null) {
            worldEvent = CreateWorldEvent(eventName);
        }

        var worldAction = new WorldAction(item, item.tileInfo , sequence); ;
        worldEvent.AddWorldAction(worldAction); 
    }

    public static void TriggerEvent(string name) {
        var worldEvent = GetWorldEvent(name);
        if ( worldEvent == null) {
            Debug.Log($"WORLD EVENT : no event named {name}, creating one");
            worldEvent = CreateWorldEvent(name);
        }
        foreach (var item in worldEvent.actions)
            item.Call();
    }

    public static WorldEvent GetWorldEvent(string name) {
        return worldEvents.Find(x => x.name == name);
    }

    
    public static void RemoveWorldEventsWithItem(Item item) {

        foreach (var itDes in ItemDescription.itemDescriptions) {
            foreach (var group in itDes.groups) {
                group.itemSlots.RemoveAll(x=> x.items.Contains(item));
            }
        }

        foreach (var worldEvent in worldEvents)
            worldEvent.actions.RemoveAll(x => x .TargetItem() == item);
    }
}
