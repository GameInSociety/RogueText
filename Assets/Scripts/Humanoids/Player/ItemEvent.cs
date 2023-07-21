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
    public string name;
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
            //Debug.LogError("no item event for : " + item.debug_name);
            return;
        }

        itemEvent.remove = true;
    }


    public static ItemEvent New(Item item, Tile tile)
    {
        ItemEvent itemEvent = new ItemEvent();

        itemEvent.name = item.debug_name;

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
            itemEvent.CallOnAllProps(eventName);

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

        targetEvent.Call(eventName, property);

    }
    public void CallOnAllProps(string eventName)
    {

        List<Property> properties = item.properties.FindAll(x=> x.enabled && x.HasEvent(eventName));

        foreach (var prop in properties)
        {
            Call(eventName, prop);

        }
    }
    public void Call(string eventName ,Property prop )
    {

        Property.EventData eventData = prop.GetEvent(eventName);

        if (eventData== null)
        {
            Debug.Log("no " + eventName + " on prop " + prop.name);
            return; 
        }

        FunctionSequence.NewSequence(
            eventData.cellContent,
            new List<Item>() { item },
            tile
        );
    }
    #endregion

}
