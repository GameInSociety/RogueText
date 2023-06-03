using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

[System.Serializable]
public class PropertyEvent
{
    public string name;
    public WorldEvent worldEvent;
    public Property property;

    public static List<PropertyEvent> list = new List<PropertyEvent>();
    public bool called = false;

    public static PropertyEvent New(string name, WorldEvent f, Property property)
    {
        PropertyEvent pEvent = new PropertyEvent();
        pEvent.name = name;
        pEvent.worldEvent = f;
        pEvent.property = property;

        pEvent.Subscribe();
        list.Add(pEvent);

        return pEvent;
    }

    #region events
    public void Subscribe()
    {
        switch (name)
        {
            case "subRain":
                TimeManager.Instance.onRaining += Call;
                break;
            case "subEmpty":
                property.onEmptyValue += Call;
                break;
            case "subHours":
                TimeManager.Instance.onNextHour += Call;
                break;
            default:
                break;
        }
    }
    public void Unsubscribe()
    {
        Debug.Log("unsuscribing : " + name + " of " + property.name);

        switch (name)
        {
            case "subRain":
                TimeManager.Instance.onRaining -= Call;
                break;
            case "subEmpty":
                property.onEmptyValue -= Call;
                break;
            case "subHours":
                TimeManager.Instance.onNextHour -= Call;
                break;
            default:
                break;
        }
    }

    void Call()
    {
        if (!property.enabled)
        {
            return;
        }

        Debug.Log("calling : " + name + " of " + property.name);

        worldEvent.Call();

        called = true;
    }
    #endregion

    public static void DescribeProperties()
    {
        if (!list.Any())
        {
            return;
        }

        List<PropertyEvent> calledEvents = list.FindAll(x => x.called);
        Debug.Log("called events count : " + calledEvents.Count);


            int index = 0;
        foreach (var item in calledEvents)
        {
            item.called = false;

            if ( index == 0)
            {
                TextManager.Write("&the dog& is ", item.worldEvent.GetItem(0));
            }

            TextManager.Add(item.property.GetDescription());
            string link = TextUtils.GetLink(index, calledEvents.Count);
            TextManager.Add(link);
            index++;

        }

    }
}
