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
    public bool waitForItem = false;

    private void Awake()
    {
        Instance = this;
    }

    public void Reset()
    {
        CurrentItems.Clear();
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


        if (CurrentItems.Empty|| !CurrentItems.foundItem)
        {
            Debug.Log("find items");
            CurrentItems.FindAll(inputText);

        }

        if (CurrentItems.Empty)
        {
            if (Verb.HasCurrent)
            {
                TextManager.Write("input_noItem");
                CurrentItems.AskForSpecificItem();
            }
            else
            {
                TextManager.Write("input_nothingRecognized");
            }
            return;
        }

        if (!CurrentItems.foundItem)
        {
            Debug.Log("wait specific");
            return;
        }

        if (!Verb.HasCurrent && !CurrentItems.Empty)
        {
            waitForVerb = true;
            TextManager.Write("input_noVerb", CurrentItems.Get.First());
            return;
        }

        if (!CurrentItems.Empty && !Verb.GetCurrent.HasCell(CurrentItems.Get.First()))
        {
            TextManager.Write("input_noCombination", CurrentItems.Get.First());
            Reset();
            return;
        }

        string cell = Verb.GetCurrent.GetCell(CurrentItems.Get.First());

        FunctionSequence functionList = FunctionSequence.New(
            cell,
            CurrentItems.Get,
            Tile.GetCurrent
            );
        functionList.Call();

        Debug.Log("clearing verb");
        Reset();
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
