using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum StateType
    {
        Health,
        Thirst,
        Hunger,
        Sleep,
    }

    private static StateManager _instance;
    public static StateManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindObjectOfType<StateManager>();
        }

        return _instance;
    }

    public State[] states;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    private void HandleOnAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.Eat:
                Eat();
                break;
            case PlayerAction.Type.DrinkAndRemove:
                DrinkAndRemove();
                break;
            case PlayerAction.Type.Drink:
                Drink();
                break;
            case PlayerAction.Type.Sleep:
                Sleep();
                break;
            case PlayerAction.Type.Wait:
                Wait();
                break;
            case PlayerAction.Type.ChangeState:
                ChangeState();
                break;
            default:
                break;
        }
    }

    void ChangeState()
    {
        StateType stateType =  (StateType)System.Enum.Parse(typeof(StateType), PlayerAction.GetCurrent.contents[0], true);

        GetState(stateType).Change(PlayerAction.GetCurrent.values[0]);

        DisplayDescription.Instance.UpdateDescription();
    }

    #region eat
    void Eat ()
	{
        string name = "" + InputInfo.GetCurrent.MainItem.word.GetContent("le chien");
        string str = "Vous mangez " + name + " et avez moins faim";

        // add health
        if (PlayerAction.GetCurrent.values.Count > 1)
        {
            GetState(StateType.Health).Remove(PlayerAction.GetCurrent.values[1]);
            str += "\nVous vous sentez aussi au meilleure santé";
        }

        DisplayFeedback.Instance.Display(str);

        Item.Remove(InputInfo.GetCurrent.MainItem);

        GetState(StateType.Hunger).Remove(PlayerAction.GetCurrent.values[0]);

        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

    #region sleep
    void Sleep()
    {
        GetState(StateType.Sleep).Remove(PlayerAction.GetCurrent.values[0]);

        DisplayFeedback.Instance.Display("Vous vous réveillez et vous sentez reposé...");

        TimeManager.GetInstance().timeOfDay = 6;
        TimeManager.GetInstance().NextDay();

        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

    #region wait
    public void Wait()
    {
        int hours;

        if (InputInfo.GetCurrent.containsNumericValue)
        {
            hours = InputInfo.GetCurrent.numericValue;
        }
        else
        {
            hours = PlayerAction.GetCurrent.values[0];
        }
        
        TimeManager.GetInstance().NextHour(hours);

        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

    #region thirst
    void DrinkAndRemove()
    {
        Drink();

        Item.Remove(InputInfo.GetCurrent.MainItem);

        DisplayDescription.Instance.UpdateDescription();

    }
    void Drink()
    {
        Item item = InputInfo.GetCurrent.MainItem;

        string str = "";

        if (item.HasProperty("full") )
        {
            if (item.GetProperty("full").GetValue() == "true")
            {
                str += "ITEM est maintenant vide\n";
                item.GetProperty("full").SetValue("false");
            }
            else
            {
                DisplayFeedback.Instance.Display("La gourde est vide...");
                return;
            }

        }

        GetState(StateType.Thirst).Remove(PlayerAction.GetCurrent.values[0]);

        str += "Vous vous désaltérez et vous avez moins soif...";

        DisplayFeedback.Instance.Display(str);
        DisplayDescription.Instance.UpdateDescription();


    }
    #endregion

    public void AdvanceStates()
    {
        GetState(StateType.Thirst).Advance();
        GetState(StateType.Hunger).Advance();
        GetState(StateType.Sleep).Advance();
    }

    public string GetDescription()
    {
        string str = "";

        foreach (var state in states)
        {
            str += state.GetDescription();
        }

        return str;
    }

    public State GetState(StateType stateType)
    {
        return states[(int)stateType];
    }
}
