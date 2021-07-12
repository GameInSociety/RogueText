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

            DisplayFeedback.Instance.Display("Quoi ?");
            return;
        }

            // check if verb has been recognized
        if (!inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb");
            }

            DisplayFeedback.Instance.Display("Que faire avec " + inputInfo.MainItem.word.GetContent("le chien"));
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
            Item verbItem = Item.FindByName("verbe seul");
            inputInfo.items.Add(verbItem);
            inputInfo.FindCombination();

            // no verb, displaying thing
            if (inputInfo.combination == null)
            {
                string str = inputInfo.verb.question + " voulez vous " + inputInfo.verb.names[0];
                str = TextManager.WithCaps(str);
                DisplayFeedback.Instance.Display(str);
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

            if (inputInfo.verb.preposition.Length != 0)
            {
                DisplayFeedback.Instance.Display("Vous ne pouvez pas " + inputInfo.verb.names[0] + " " + inputInfo.verb.preposition + " " + inputInfo.MainItem.word.GetContent("le chien"));
            }
            else
            {
                DisplayFeedback.Instance.Display("Vous ne pouvez pas " + inputInfo.verb.names[0] + " " + inputInfo.MainItem.word.GetContent("le chien"));
            }
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

            string[] args = parameters_str.Split(',');

            foreach (var arg in args)
            {
                int i = 0;

                if (int.TryParse(arg, out i))
                {
                    newAction.values.Add(i);
                }
                else
                {
                    newAction.contents.Add(arg);
                }

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
