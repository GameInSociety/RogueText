using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    private static TimeManager _instance;
    public static TimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<TimeManager>();
            }

            return _instance;
        }
    }
    public int daysPasted = 0;

	public int timeOfDay = 0;

    /// <summary>
    /// RAIN
    /// </summary>
	public int rainRate_Max = 40;
	public int rainRate_Min = 10;
	public int hoursLeftToRain = 0;
	public int rainDuration_Max = 10;
	public int rainDuration_Min = 1;
    public bool raining = false;
    public bool displayRainDescription = false;

    /// <summary>
    /// HOURS
    /// </summary>
	public int hourToDawn = 5;
	public int hourToMorning = 8;
	public int hourToNoon = 12;
	public int hourToAfternoon = 14;
	public int hourToDusk = 18;
	public int hourToNight = 21;

    public int movesToNextHour = 3;
    public int currentMove = 0;

    public bool changedPartOfDay = true;

    public delegate void OnNextDay();
    public OnNextDay onNextDay;

    public delegate void OnNextHour();
    public OnNextHour onNextHour;

    public enum PartOfDay {
		Dawn,
		Morning,
		Noon,
		Afternoon,
		Dusk,
		Night,

        None,
	}

	public PartOfDay currentPartOfDay;
	public PartOfDay previousPartOfDay;

	public PartOfDay GetPartOfDay () {

		if (timeOfDay < hourToDawn)
			return PartOfDay.Night;

		if (timeOfDay < hourToMorning)
			return PartOfDay.Dawn;

		if (timeOfDay < hourToNoon)
			return PartOfDay.Morning;

		if (timeOfDay < hourToAfternoon)
			return PartOfDay.Noon;

		if (timeOfDay < hourToDusk)
			return PartOfDay.Afternoon;

		if (timeOfDay < hourToNight)
			return PartOfDay.Dawn;

		return PartOfDay.Night;
	}

    private void Start()
    {
        ResetRain();
        UpdateStates();

    }

    public void AdvanceTime()
    {
        currentMove++;

        if (currentMove >= movesToNextHour)
        {
            currentMove = 0;

            NextHour(1);
        }
    }

    public void Wait(int hours)
    {
        NextHour(hours);

        // pour l'instant un peu oblig� � cause des objets etc...
        Tile.GetCurrent.Describe();
    }

    public void ChangeMovesPerHour(int i)
    {
        movesToNextHour = i;

        currentMove = 0;

        NextHour(1);
    }

    public void NextHour(int hours)
    {
        for (int i = 0; i < hours; i++)
        {
            ++timeOfDay;
            
            // states
            ConditionManager.GetInstance().AdvanceCondition();

            // 
            UpdateStates();

            if (raining)
            {
                ItemEvent.CallEvent("subRain");
            }

            if (timeOfDay == 24)
            {
                timeOfDay = 0;
                NextDay();
            }

            ItemEvent.CallEvent("subHours");

            if (onNextHour != null)
            {
                onNextHour();
            }
        }

    }

    public void WriteTimeOfDay()
    {
        TextManager.Write("time_Hours");
    }

    public string GetTimeOfDayDescription()
    {
        if (timeOfDay == 12)
        {
            return "noon";
        }
        else if (timeOfDay == 0)
        {
            return "midnight";
        }
        else if (timeOfDay < 12)
        {
            return timeOfDay + " in the morning";
        }
        else
        {
            return timeOfDay + " at night";
        }
    }

    public void NextDay()
    {
        ++daysPasted;
        if (onNextDay != null)
        {
            onNextDay();
        }

    }

    #region part of day
    public void WriteDescription()
    {
        if (!changedPartOfDay)
        {
            return;
        }

        changedPartOfDay = false;

        switch (currentPartOfDay)
        {
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
    public void WriteWeatherDescription()
    {
        if (!displayRainDescription)
        {
            return;
        }

        displayRainDescription = false;

        if (raining)
        {
            TextManager.Write("weather_RainStarts");
        }
        else
        {
            TextManager.Write("weather_RainStops");
        }

        
    }
    private void UpdateStates()
    {
        // part of day
        previousPartOfDay = currentPartOfDay;
        currentPartOfDay = GetPartOfDay();

        if (currentPartOfDay != previousPartOfDay)
        {
            changedPartOfDay = true;
        }

        --hoursLeftToRain;

        if ( hoursLeftToRain == 0) {

            raining = !raining;

            ResetRain();
            displayRainDescription = true;

        }
    }

    void ResetRain()
    {
        if (raining)
        {
            hoursLeftToRain = Random.Range(rainDuration_Min, rainDuration_Max);
        }
        else
        {
            hoursLeftToRain = Random.Range(rainRate_Min, rainRate_Max);
        }
    }
    #endregion
}
