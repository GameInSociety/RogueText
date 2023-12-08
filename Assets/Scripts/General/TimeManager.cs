using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class TimeManager {

    public static int movesToNextHour = 3;
    public static int currentMove = 0;

    public static WorldAction worldAction_Hours;

    public static void Init() {
        var timeItem = WorldData.globalItems.Find(x => x.debug_name == "hour");
        worldAction_Hours = new WorldAction(timeItem, "update(hours, +1)");
    }

    public static void AdvanceTime() {
        currentMove++;
        if (currentMove >= movesToNextHour) {
            currentMove = 0;
            NextHour(1);
        }
    }

    public static void Wait(int hours) {
        NextHour(hours);
    }

    public static void ChangeMovesPerHour(int i) {
        movesToNextHour = i;
        currentMove = 0;
        NextHour(1);
    }

    public static void NextHour(int count) {
        for (var i = 0; i < count; i++) {
            worldAction_Hours.Call();
        }

    }

}
