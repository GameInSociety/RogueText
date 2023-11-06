using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Function_Time : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }

    void display() {
        TimeManager.Instance.WriteTimeOfDay();
    }

    void wait() {
        int hours = ItemParser.GetCurrent.itemGroups[0].items.Count;
        TimeManager.Instance.Wait(hours);
    }
}
