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
            case PlayerAction.Type.SetState:
                SetState();
                break;
            case PlayerAction.Type.Wait:
                Wait();
                break;
            default:
                break;
        }
    }

    void SetState()
    {
        StateType stateType =  (StateType)System.Enum.Parse(typeof(StateType), PlayerAction.GetCurrent.GetContent(0), true);

        string str = PlayerAction.GetCurrent.GetContent(1);

        if (str.Contains("+"))
        {
            // add
            str = str.Remove(0,1);
            int value = 0;
            value = int.Parse(str);

            GetState(stateType).Change((int)GetState(stateType).progress+ value);
        }
        else if (str.Contains("-"))
        {
            // substract
            str = str.Remove(0, 1);

            int value = 0;
            value = int.Parse(str);

            GetState(stateType).Change((int)GetState(stateType).progress- value);

        }
        else
        {
            int value = 0;
            value = int.Parse(str);

            GetState(stateType).Change(value);
        }

        WriteText();

        Item.Remove(InputInfo.GetCurrent.MainItem);
    }

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
            hours = PlayerAction.GetCurrent.GetValue(0);
        }
        
        TimeManager.GetInstance().NextHour(hours);

        // pour l'instant un peu obligé à cause des objets etc...
        DisplayDescription.Instance.UpdateDescription();
    }
    #endregion

    public void AdvanceStates()
    {
        GetState(StateType.Thirst).Advance();
        GetState(StateType.Hunger).Advance();
        GetState(StateType.Sleep).Advance();
    }

    public void WriteDescription()
    {
        bool b = false;
        foreach (var state in states)
        {
            if (state.progress != State.Progress.Normal)
            {
                string phrase = state.phrases[(int)state.progress - 1];
                Phrase.Write(state.GetDescription());

                b = true;
            }
        }

        if (b)
        {
            Phrase.Space();
        }
    }

    public void WriteText()
    {
        WriteDescription();
    }

    public State GetState(StateType stateType)
    {
        return states[(int)stateType];
    }
}
