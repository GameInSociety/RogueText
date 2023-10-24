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
        int hours = ItemParser.GetCurrent.numericValueInInput;
        TimeManager.Instance.Wait(hours);
    }
}
