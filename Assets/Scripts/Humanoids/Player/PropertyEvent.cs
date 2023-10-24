using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[System.Serializable]
public class PropertyEvent {
    public Property.EventData eventData;
    public Property property;

    public bool called = false;


    public static PropertyEvent New(Property.EventData eventData, Property property) {
        var pEvent = new PropertyEvent();
        pEvent.eventData = eventData;
        pEvent.property = property;

        return pEvent;
    }

}
