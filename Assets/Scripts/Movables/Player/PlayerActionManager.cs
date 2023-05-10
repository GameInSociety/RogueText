using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionManager : MonoBehaviour
{
    // singleton
    public static PlayerActionManager Instance;

    // action event
    public delegate void OnPlayerAction(PlayerAction action);
    /// <summary>
    /// POURQUOI CEST STATIC WEH SA VA CREER DES BRICOLES SI JE CHANGE DE SCENE
    /// </summary>
    public static OnPlayerAction onPlayerAction;

    public bool debug = false;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // CENTRALISER LES ACTIONS !
        onPlayerAction += HandleOnPlayerAction;
    }

    private void HandleOnPlayerAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.DescribeExterior:
                Interior.DescribeExterior();
                break;
            case PlayerAction.Type.DisplayTimeOfDay:
                TimeManager.GetInstance().WriteTimeOfDay();
                break;
            case PlayerAction.Type.PointNorth:
                Coords.WriteDirectionToNorth();
                break;
            case PlayerAction.Type.Write:
                TextManager.WritePhrase(PlayerAction.GetCurrent.GetContent(0));
                break;
            default:
                break;
        }
    }



    public void DisplayInputFeedback()
    {
        InputInfo inputInfo = InputInfo.Instance;

            // check if ANYTHING has been recognized
        if (!inputInfo.HasItems() && !inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb, no items");
            }

            TextManager.WritePhrase("input_nothingRecognized");
            return;
        }

            // check if verb has been recognized
        if (!inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb");
            }


            TextManager.WritePhrase("input_noVerb");
            return;
        }

            // check if item has been recognized
        if (inputInfo.HasVerb() && !inputInfo.HasItems())
        {
            if (debug)
            {
                Debug.LogError("only verb, no item");
            }

            inputInfo.sustainVerb = true;

            TextManager.WritePhrase("input_noItem");
            return;
        }

        // verb / item combinaison doesn't exist
        if (inputInfo.combination == null)
        {
            if (debug)
            {
                Debug.LogError("Fail : no combination between verb : " + inputInfo.verb.names[0] + " and item : " + inputInfo.GetItem(0).word.text);
            }

            TextManager.WritePhrase("input_noCombination");
            return;
        }

        // clear all props variables
        PropertyManager.Instance.pendingProps.Clear();

        // get cell content
        string[] lines = inputInfo.combination.content.Split('\n');

        // separate all actions
        foreach (var line in lines)
        {
            // parse action
            PlayerAction action = GetAction(line);

            if (action != null)
            {
                action.Call();
                onPlayerAction(action);

                if ( debug)
                {
                    Debug.Log("call action " + action.type);
                }

    

                if (breakActions)
                {
                    Debug.Log(" !!!!! ACTION SEARCH IS STOPPED DUE TO THING !!!! ");
                    breakActions = false;
                    break;
                }

            }
        }
    }

    public PlayerAction GetAction(string line)
    {
        if ( line.StartsWith("=>")){
            // link to other combinationn
            // example : => open window
        }

        PlayerAction.Type[] actionTypes = System.Enum.GetValues(typeof(PlayerAction.Type)) as PlayerAction.Type[];
        
            // check parameters
        bool hasParameters = line.Contains("(");
        string function_str = line;
        if (hasParameters)
        {
            function_str = line.Remove(line.IndexOf('('));
        }

            // get action type
        PlayerAction.Type actionType = System.Array.Find(actionTypes, x => function_str.ToLower() == x.ToString().ToLower());

        if (actionType == PlayerAction.Type.None)
        {
            Debug.LogError("Couldn't find action type : " + function_str);
            return null;
        }

            // create action 
        PlayerAction newAction = new PlayerAction();
        newAction.type = actionType;

            // check parameters
        if (hasParameters)
        {
            string parameters_str = line.Remove(0, actionType.ToString().Length);

            // remove parentheses
            parameters_str = parameters_str.Remove(0, 1);
            parameters_str = parameters_str.Remove(parameters_str.Length - 1);

            string[] stringSeparators = new string[] { ", " };
            string[] args = parameters_str.Split(stringSeparators, StringSplitOptions.None);

            foreach (var arg in args)
            {
                newAction.AddContent(arg);
            }
        }

        return newAction;
    }


    #region action breaking
    bool breakActions = false;

    public void BreakAction(string text)
    {
        TextManager.WritePhrase(text);

        BreakAction();
    }

    public void BreakAction()
    {
        breakActions = true;
    }
    #endregion


}
