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
        UpdateWeather();
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

            // part of day
            previousPartOfDay = currentPartOfDay;
            currentPartOfDay = GetPartOfDay();

            // states
            StateManager.GetInstance().AdvanceStates();

            // 
            UpdateWeather();

            changedPartOfDay = false;
            if (previousPartOfDay != currentPartOfDay)
                changedPartOfDay = true;

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

    public void NextDay()
    {
        ++daysPasted;
        if (onNextDay != null)
        {
            onNextDay();
        }

    }

    #region part of day
    public string GetPartOfDayDescription()
    {
        string str = "\n";

        switch (currentPartOfDay)
        {
            case PartOfDay.Dawn:
                str += "Le soleil ne fait que se lever...";
                break;
            case PartOfDay.Morning:
                str += "Le matin s'est installé";
                break;
            case PartOfDay.Noon:
                str += "Le matin se termine doucement et le jour commence";
                break;
            case PartOfDay.Afternoon:
                str += "Il doit être l'après midi";
                break;
            case PartOfDay.Dusk:
                str += "Le soleil se couche au loin";
                break;
            case PartOfDay.Night:
                str += "La nuit est tombée, tout est sombre";
                break;
            default:
                str += "pas de description de PART OF DAY?";
                break;
        }

        str += "\n";

        return str;
    }
    #endregion

    #region weather
    public string GetWeatherDescription()
    {
        displayRainDescription = false;

        if (raining)
        {
            return "Il pleut très fort sur " + Tile.GetCurrent.tileItem.word.GetContent("le chien sage");
        }
        else
        {
            return "La pluie cesse doucement de tomber sur " + Tile.GetCurrent.tileItem.word.GetContent("le chien sage");
        }
    }
    private void UpdateWeather()
    {
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
