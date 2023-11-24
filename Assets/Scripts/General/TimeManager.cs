using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeManager {

    public static int hours = 0;
    public static int days;
    public static int mounts;
    public static int years;

    static bool raining;
    public static int movesToNextHour = 3;
    public static int currentMove = 0;
    static int hoursToRain = 0;
    static bool writeWeather = false;

    public static bool changedPartOfDay = true;

    public enum PartOfDay {
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Dusk,
        Night,

        None,
    }

    public static PartOfDay partOfDay;
    public static PartOfDay previousPartOfDay;

    public static PartOfDay GetPartOfDay() {

        if (hours < WorldData.getInt("hours_dawn"))
            return PartOfDay.Night;

        if (hours < WorldData.getInt("hours_morning"))
            return PartOfDay.Dawn;

        if (hours < WorldData.getInt("hours_noon"))
            return PartOfDay.Morning;

        if (hours < WorldData.getInt("hours_afternoon"))
                return PartOfDay.Noon;

        if (hours < WorldData.getInt("hours_dusk"))
            return PartOfDay.Afternoon;

        if (hours < WorldData.getInt("hours_night"))
            return PartOfDay.Dawn;

        return PartOfDay.Night;
    }

    public static void Init() {
        ResetRain();
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
            ++hours;
            if (raining)
                WorldEvent.TriggerEvent("onRain");

            if (hours == 24) {
                hours = 0;
                NextDay();
            }

            WorldEvent.TriggerEvent("onHours");
        }

    }

    public static string GetTimeOfDayDescription() {
        if (hours == 12) {
            return "noon";
        } else if (hours == 0) {
            return "midnight";
        } else if (hours < 12) {
            return hours + " in the morning";
        } else {
            return hours + " at night";
        }
    }

    public static void NextDay() {
        ++days;
    }

    #region part of day
    public static void writeTimeOfDay() {
        if (!changedPartOfDay) {
            return;
        }

        changedPartOfDay = false;

        switch (partOfDay) {
            case PartOfDay.Dawn:
                TextManager.Write("weather_dawn");
                break;
            case PartOfDay.Morning:
                TextManager.Write("weather_morning");
                break;
            case PartOfDay.Noon:
                TextManager.Write("weather_noon");
                break;
            case PartOfDay.Afternoon:
                TextManager.Write("weather_afternoon");
                break;
            case PartOfDay.Dusk:
                TextManager.Write("weather_dusk");
                break;
            case PartOfDay.Night:
                TextManager.Write("weather_night");
                break;
            default:
                break;
        }


    }
    #endregion

    #region weather
    public static void writeWeatherDescription() {
        if (!writeWeather)
            return;
        writeWeather= false;

        if (raining) {
            TextManager.Write("weather_RainStarts");
        } else {
            TextManager.Write("weather_RainStops");
        }


    }
    static void UpdateStates() {
        // part of day
        previousPartOfDay = partOfDay;
        partOfDay = GetPartOfDay();

        if (partOfDay != previousPartOfDay)
            changedPartOfDay = true;

        --hoursToRain;

        if (hoursToRain == 0) {

            raining = !raining;
            ResetRain();
            writeWeather = true;

        }
    }

    static void ResetRain() {
        if (raining)
            hoursToRain = Random.Range(WorldData.getInt("rain_duration_min"), WorldData.getInt("rain_duration_max"));
        else
            hoursToRain = Random.Range(WorldData.getInt("rain_start_min"), WorldData.getInt("rain_start_max"));
    }
    #endregion
}
