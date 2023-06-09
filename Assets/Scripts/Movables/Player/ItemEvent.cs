using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ItemEvent
{
    public Item item;
    public Tile tile;
    public List<PropertyEvent> events = new List<PropertyEvent>();

    public static List<ItemEvent> list = new List<ItemEvent>();

    public bool remove = false;

    public static void Remove(Item item)
    {
        ItemEvent itemEvent = list.Find(x=> x.item == item);

        if ( itemEvent == null)
        {
            return;
            Debug.LogError("no item event for : " + item.debug_name);
        }

        itemEvent.remove = true;
    }


    public static ItemEvent New(Item item, Tile tile)
    {
        ItemEvent itemEvent = new ItemEvent();
        
        itemEvent.item = item;
        itemEvent.tile = tile;

        list.Add(itemEvent);
        return itemEvent;
    }
    #region call
    public static void CallEvent(string eventName)
    {
        list.RemoveAll(x => x.remove);

        foreach (var itemEvent in list)
        {
            itemEvent.Call(eventName);

            //Property.DescribeUpdated(itemEvent.item);

        }


    }
    public static void CallEventOnProp(string eventName, Property property)
    {
        ItemEvent targetEvent = list.Find(x => x.item.HasProperty(property.name));

        if ( targetEvent == null)
        {
            Debug.LogError("no events with property : " + property.name);
            return;
        }

        targetEvent.CallOnProp(eventName, property);

    }
    public void Call(string eventName)
    {

        List<Property> properties = item.properties.FindAll(x=> x.enabled && x.HasEvent(eventName));

        foreach (var prop in properties)
        {
            CallOnProp(eventName, prop);

        }
    }
    public void CallOnProp(string eventName ,Property prop )
    {
        Property.EventData eventData = prop.GetEvent(eventName);

        FunctionSequence worldEvent = FunctionSequence.New(
        eventData.cellContent,
        new List<Item>() { item },
        tile
        );

        worldEvent.Call();
    }
    #endregion

}
