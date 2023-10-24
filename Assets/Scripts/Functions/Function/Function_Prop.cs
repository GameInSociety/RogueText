using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.Search;
using UnityEngine;

public class Function_Prop : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }
    void disable() {
        var targetItem = base.targetItem();

        var line = GetParam(0);
        var property = targetItem.properties.Find(x => x.name == line);
        targetItem.DisableProperty(line);
    }

    void enable() {
        var targetItem = base.targetItem();

        var prop_name = GetParam(0);

        var property = targetItem.properties.Find(x => x.name == prop_name);

        if (property == null) {
            Debug.LogError("ACTION_ENABLEPROPERY : did not find property : " + prop_name + " on " + targetItem.debug_name);
            return;
        }

        targetItem.EnableProperty(prop_name);


    }

    void checkValue() {
        var targetItem = base.targetItem();

        var propertyName = GetParam(0);

        var property = targetItem.GetProperty(propertyName);

        if (property.GetInt() <= 0) {
            TextManager.write("No " + property.name);
            FunctionSequence.current.Stop();
            return;
        }
    }

    void check() {

    }

    void remove() {
        var targetItem = base.targetItem();

        var propertyName = GetParam(0);

        targetItem.DeleteProperty(propertyName);
    }

    void create() {
        var targetItem = base.targetItem();

        var line = GetParam(0);
        _ = targetItem.addProperty(line);
    }

    void update() {
        var propName = GetParam(0);
        var line = GetParam(1);
        targetItem().UpdateProperty(propName, line);
    }
}
