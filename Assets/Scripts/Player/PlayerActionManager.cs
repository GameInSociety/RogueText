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
    public static OnPlayerAction onPlayerAction;

    public bool debug = false;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        onPlayerAction += HandleOnPlayerAction;
    }

    private void HandleOnPlayerAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.Display:
			    Phrase.Write(action.GetContent(0));
                break;
            case PlayerAction.Type.DescribeExterior:
                Interior.DescribeExterior();
                break;
            case PlayerAction.Type.DisplayTimeOfDay:
                TimeManager.GetInstance().WriteTimeOfDay();
                break;
            case PlayerAction.Type.DescribeItem:
                Item.Describe(InputInfo.GetCurrent.MainItem);
                break;
            case PlayerAction.Type.PointNorth:
                Coords.WriteDirectionToNorth();
                break;
            case PlayerAction.Type.ChangeProp:
                Property.ChangeProperty();
                break;
            case PlayerAction.Type.AddProp:
                Property.AddProperty();
                break;
            default:
                break;
        }
    }



    public void DisplayInputFeedback()
    {
        InputInfo inputInfo = InputInfo.GetCurrent;

            // check if ANYTHING has been recognized
        if (!inputInfo.HasItems() && !inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb, no items");
            }

            Phrase.Write("Quoi ?");
            return;
        }

            // check if verb has been recognized
        if (!inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb");
            }
            string str = "Que faire avec &le chien (main item)&";
            Phrase.Write(str);
            return;
        }

            // check if item has been recognized
        if (inputInfo.HasVerb() && !inputInfo.HasItems())
        {
            if (debug)
            {
                Debug.LogError("only verb, no item");
            }

            // get verb only action
            Item verbItem = Item.GetDataItem("verbe seul");
            inputInfo.AddItem(verbItem);
            inputInfo.FindCombination();

            // no verb, displaying thing
            if (inputInfo.combination == null)
            {
                string str = inputInfo.verb.question + " voulez vous " + inputInfo.verb.names[0];
                str = TextManager.WithCaps(str);
                Phrase.Write(str);
                return;
            }
        }

        // verb / item combinaison doesn't exist
        if (inputInfo.combination == null)
        {
            if (debug)
            {
                Debug.LogError("Fail : no combination between verb : " + inputInfo.verb.names[0] + " and item : " + inputInfo.MainItem.word.text);
            }

            string str;

            if (inputInfo.verb.preposition.Length != 0)
            {
                str = "Vous ne pouvez pas " + inputInfo.verb.names[0] + " " + inputInfo.verb.preposition + " &le chien (main item)&";
            }
            else
            {
                str = "Vous ne pouvez pas " + inputInfo.verb.names[0] + " &le chien (main item)&";
            }

            Phrase.Write(str);
            return;
        }

        if (debug)
        {
            Debug.LogError("no action");
        }
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
                int i = 0;

                newAction.AddContent(arg);

                /*if (int.TryParse(arg, out i))
                {
                    newAction.values.Add(i);
                }
                else
                {
                    newAction.contents.Add(arg);
                }*/

            }
        }

        return newAction;
    }


    #region action breaking
    bool breakActions = false;

    public void BreakAction()
    {
        breakActions = true;
    }
    #endregion


}
