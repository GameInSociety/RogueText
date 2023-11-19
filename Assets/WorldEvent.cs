using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class WorldEvent {

    public static List<WorldEvent> worldEvents;
    public static void Init() {
        worldEvents = new List<WorldEvent>() {
            new WorldEvent("onRain"),
            new WorldEvent("onHours"),
            new WorldEvent("onDays")
        };
    }

    public static void SubscribeItem(Item item) {
        foreach (var worldEvent in worldEvents) {
            var prop = item.props.Find(x=> x.hasPart(worldEvent.name));
            if ( prop != null) {
                Debug.Log($"found event {worldEvent.name} in prop {prop.name} of item {item.debug_name}");
                WorldAction worldAction = new WorldAction(item, Player.Instance.coords, Player.Instance.tilesetId, prop.getPart(worldEvent.name).text);
            }
        }
    }

    public static void TriggerEvent(string name) {
        foreach (var item in GetWorldEvent("name").actions) {
            item.Call();
        }
    }

    public static WorldEvent GetWorldEvent(string name) {
        WorldEvent worldEvent = worldEvents.Find(x => x.name == name);
        if (worldEvent == null)
            Debug.LogError($"no world actions named {name}");
        return worldEvent;
    }

    public WorldEvent(string name) {
        this.name = name;
    }

    public string name;
    public List<WorldAction> actions;
    public void AddWorldAction(WorldAction action) {
        actions.Add(action);
    }

    public static void RemoveWorldEventsWithItem(Item item) {
        foreach (var worldEvent in worldEvents)
            worldEvent.actions.RemoveAll(x => x.item == item);
    }
}
