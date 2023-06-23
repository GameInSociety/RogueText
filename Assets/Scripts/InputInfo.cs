using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine.Analytics;

[System.Serializable]
public class InputInfo : MonoBehaviour
{
    public static InputInfo Instance;

    // static
    public string inputText;

    public bool hasValueInText = false;

    public int valueInText = 0;

    public bool itemConfusion = false;

    public bool waitForVerb = false;
    // bizarre, mais en soi wait for specific item est que pour les specs, pas pour le premier
    public bool waitForFirstItem = false;

    public delegate void OnAction();
    public OnAction onAction;

    ItemGroup itemGroup;

    private void Awake()
    {
        Instance = this;
    }

    public void Reset()
    {
        itemGroup = null;
        waitForFirstItem = false;
        waitForVerb = false;
        Verb.Clear();
    }

    public void ParseText(string str)
    {
        inputText = str;

        if (string.IsNullOrEmpty(inputText))
        {
            return;
        }

        inputText = inputText.TrimEnd(' ');

        if (!Verb.HasCurrent)
        {
            Verb verb = Verb.Get(inputText);

            if (waitForVerb && verb == null)
            {
                waitForVerb = false;
                TextManager.Write("Can't do that");
                Reset();
                return;
            }
            waitForVerb = false;

            Verb.SetCurrent(verb);
        }

        if ( itemGroup == null)
        {
            itemGroup = ItemGroup.New();
        }

        if (itemGroup.Empty|| itemGroup.waitForItem)
        {
            itemGroup.Init(inputText);
        }

        if (itemGroup.Empty)
        {
            if (Verb.HasCurrent && !waitForFirstItem)
            {
                waitForFirstItem = true;
                TextManager.Write("input_noItem");
                //itemGroup.WaitForSpecificItem("input_noItem");
            }
            else
            {
                Reset();
                TextManager.Write("input_nothingRecognized");
            }
            return;
        }

        if (itemGroup.waitForItem)
        {
            return;
        }

        if (!Verb.HasCurrent && !itemGroup.Empty)
        {
            waitForVerb = true;
            TextManager.Write("input_noVerb", itemGroup.GetItems.First());
            return;
        }

        Verb.Sequence sequence = itemGroup.GetSequence();

        if (sequence == null)
        {
            TextManager.Write("input_noCombination", itemGroup.GetItems.First());
            Reset();
            return;
        }

        FunctionSequence functionList = FunctionSequence.Call(
            sequence.content,
            itemGroup,
            Tile.GetCurrent
            );

        if (onAction != null)
            onAction();

        //Reset();
    }


    private void FindNumber()
    {
        hasValueInText = false;

        foreach (var part in inputText.Split(' '))
        {
            if (part.All(char.IsDigit))
            {

                if ( int.TryParse(part, out valueInText))
                {
                    hasValueInText = true;
                }
                else
                {
                    //Debug.LogError("couldn't parse value " + part);
                }

                break;
            }
        }
    }

}
