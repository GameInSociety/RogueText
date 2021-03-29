using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

	public static TimeManager Instance;

	public int daysPasted = 0;

	public int timeOfDay = 0;

    /// <summary>
    /// RAIN
    /// </summary>
	public int rainRate_Max = 40;
	public int rainRate_Min = 10;
	public int hoursToRain = 0;
	public int rainDuration_Max = 10;
	public int rainDuration_Min = 1;
    public bool raining = false;

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
    public int currentMoves = 0;

    public bool changedPartOfDay = false;

	public int [] hoursToPartOfDay;

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

	void Awake () {
		Instance = this;
	}

    private void Start()
    {
        ResetRain();
        UpdateRain();
    }

    public void AdvanceTime()
    {
        currentMoves++;

        if (currentMoves >= movesToNextHour)
        {
            currentMoves = 0;

            NextHour();
        }
    }

    public void ChangeMovesPerHour(int i)
    {
        movesToNextHour = i;

        currentMoves = 0;

        NextHour();
    }

    void NextHour()
    {
        ++timeOfDay;

        previousPartOfDay = currentPartOfDay;

        currentPartOfDay = GetPartOfDay();

        Player.Instance.UpdateStates();

        UpdateRain();

        changedPartOfDay = false;
        if (previousPartOfDay != currentPartOfDay)
            changedPartOfDay = true;

        if (timeOfDay == 24)
        {
            timeOfDay = 0;
            NextDay();
        }
    }

    private void UpdateRain()
    {
        --hoursToRain;

        if ( hoursToRain == 0) {

            if (raining)
            {
                DisplayFeedback.Instance.Display("Il commence à pleuvoir...");
                raining = false;
                ResetRain();
            }
            else
            {
                DisplayFeedback.Instance.Display("Il s'est arrêté de pleuvoir");
                raining = true;
                ResetRain();
            }
        }
    }

    void ResetRain()
    {
        if (raining)
        {
            hoursToRain = Random.Range(rainDuration_Min, rainDuration_Max);
        }
        else
        {
            hoursToRain = Random.Range(rainRate_Min, rainRate_Max);
        }
    }

    public delegate void OnNextDay();
    public OnNextDay onNextDay;

    public void NextDay()
    {
        ++daysPasted;
        if (onNextDay!=null)
        {
            onNextDay();
        }

    }
}
