using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class PropertyEvent
{
    public string name;
    public WorldEvent functionList;
    public Property property;

    public static List<PropertyEvent> events = new List<PropertyEvent>();

    public static PropertyEvent New(string name, WorldEvent f, Property property)
    {
        PropertyEvent pEvent = new PropertyEvent();
        pEvent.name = name;
        pEvent.functionList = f;
        pEvent.property = property;

        pEvent.Subscribe();
        events.Add(pEvent);

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

        functionList.Call();

        property.changed = true;
    }
    #endregion
}
