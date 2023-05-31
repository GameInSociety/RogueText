using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// this class is used to keep the link between the property and the item
// that way, when the event is called, it is called on the right item
// maybe, property groups may have many items,
// solving the GROWING cunandrum ( plant grows in the current tile instead of another )
[System.Serializable]
public class WorldEvent
{
    public string eventName;
    public Item item;
    public Property property;
    public Tile tile;
    public bool changed = false;

    public static List<WorldEvent> list = new List<WorldEvent>();

    public static void Add(
        string name,
        Item _item,
        Property _property,
        Tile _tile
        )
    {
        WorldEvent worldEvent = new WorldEvent();

        worldEvent.eventName = name;
        worldEvent.item = _item;
        worldEvent.property = _property;
        worldEvent.tile = _tile;

        worldEvent.Subscribe();

        list.Add( worldEvent );
    }

    public static void Remove(string name,
        Item _item,
        Property _property,
        Tile _tile)
    {
        WorldEvent worldEvent = list.Find(x =>
        x.eventName == name
        &&
        x.item == _item
        &&
        x.property == _property
        &&
        x.tile == _tile);

        if ( worldEvent == null)
        {
            return;
        }

        list.Remove(worldEvent);
    }

    public void Subscribe()
    {
        switch (eventName)
        {
            case "subRain":
                TimeManager.GetInstance().onRaining += HandleOnRaining;
                break;
            case "subEmpty":
                property.onEmptyValue += HandleOnEmpty;
                break;
            case "subHours":
                TimeManager.GetInstance().onNextHour += HandleOnNextHour;
                break;
            default:
                break;
        }
    }

    void HandleOnNextHour()
    {
        Call();
    }

    void HandleOnEmpty()
    {
        Call();
    }

    void HandleOnRaining()
    {
        Call();
    }

    static List<WorldEvent> stackEvents= new List<WorldEvent>();
    static bool onGoingEvent = false;
    public void Call()
    {
        if ( !property.enabled)
        {
            return;
        }
        
        if ( onGoingEvent )
        {
            stackEvents.Add(this);
            return;
        }

        onGoingEvent = true;
        FunctionManager.onFunctionEnd += HandleOnEndFunction;

        FunctionManager.SetItem(item);
        string functionList = property.FindEvent(eventName).functionList;
        FunctionManager.CallFunctions(functionList);

        changed = true;

        
    }

    void HandleOnEndFunction()
    {
        onGoingEvent = false;
        FunctionManager.onFunctionEnd -= HandleOnEndFunction;

        if ( stackEvents.Count > 0)
        {
            WorldEvent worldEvent = stackEvents[0];
            stackEvents.RemoveAt(0);
            worldEvent.Call();
        }
    }
}