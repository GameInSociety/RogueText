using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    private static TimeManager _instance;
    public static TimeManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindObjectOfType<TimeManager>();
        }

        return _instance;
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

    public delegate void OnRaining();
    public OnRaining onRaining;

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
                if (onRaining != null)
                {
                    onRaining();
                }
            }

            if (timeOfDay == 24)
            {
                timeOfDay = 0;
                NextDay();
            }

            if (onNextHour != null)
            {
                onNextHour();
            }
        }

    }

    public void WriteTimeOfDay()
    {
        TextManager.WritePhrase("time_Hours");
    }

    public string GetTimeOfDayDescription()
    {
        if (TimeManager.GetInstance().timeOfDay == 12)
        {
            return "noon";
        }
        else if (TimeManager.GetInstance().timeOfDay == 0)
        {
            return "midnight";
        }
        else if (TimeManager.GetInstance().timeOfDay < 12)
        {
            return TimeManager.GetInstance().timeOfDay + " in the morning";
        }
        else
        {
            return TimeManager.GetInstance().timeOfDay + " at night";
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
                TextManager.WritePhrase("weather_dawn");
                break;
            case PartOfDay.Morning:
                TextManager.WritePhrase("weather_morning");
                break;
            case PartOfDay.Noon:
                TextManager.WritePhrase("weather_noon");
                break;
            case PartOfDay.Afternoon:
                TextManager.WritePhrase("weather_afternoon");
                break;
            case PartOfDay.Dusk:
                TextManager.WritePhrase("weather_dusk");
                break;
            case PartOfDay.Night:
                TextManager.WritePhrase("weather_night");
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
            TextManager.WritePhrase("weather_RainStarts");
        }
        else
        {
            TextManager.WritePhrase("weather_RainStops");
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
