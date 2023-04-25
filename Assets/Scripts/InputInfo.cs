using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class InputInfo
{
    public static void SetCurrent(InputInfo inputInfo)
    {
        current = inputInfo;
    }

    private static InputInfo current;

    public static InputInfo GetCurrent
    {
        get
        {
            return current;
        }
    }

    // static
    public static string text;
    public static List<string> parts = new List<string>();

    // phrase content
    public Verb verb;
    private List<Item> items = new List<Item>();
    public Combination combination;
    public Player.Orientation orientation;
    public Cardinal direction;
    public bool hasValueInText = false;
    public int valueInText = 0;


    public bool actionOnAll = false;

    public Item MainItem
    {
        get
        {
            if (!HasItems())
            {
                return null;
            }

            return items[0];
        }
    }

    public bool HasSecondItem()
    {
        return items.Count > 1;
    }

    public Item GetSecondItem
    {
        get
        {
            if ( items.Count < 1)
            {
                Debug.LogError("no second item");
                return MainItem;
            }

            return items[1];
        }
    }

    public void GetItemCount()
    {

    }


    public static void ParsePhrase(string str)
    {
        text = str;

        if (text.Length == 0)
        {
            return;
        }

        text = text.TrimEnd(' ');

       

        // create action
        InputInfo newInputInfo = new InputInfo();

        InputInfo.SetCurrent(newInputInfo);

        newInputInfo.FindVerb();

        // separate text ( after verb in case phrase changing, verb is removed 
        parts = text.Split(new char[3] { ' ', '\'', '\'' }).ToList<string>();

        newInputInfo.FindNumber();

        newInputInfo.FindOrientation();

        newInputInfo.FindItems();

        newInputInfo.FindCombination();


        PlayerActionManager.Instance.DisplayInputFeedback();
    }

    private void FindNumber()
    {
        hasValueInText = false;

        foreach (var part in InputInfo.parts)
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

    public void FindVerb()
    {
        //string str = InputInfo.parts[0];

        verb = Verb.Find(text);

        if (verb == null)
        {
            // c'est pas grave de pas avoir de verbe
            //Debug.LogError("couldn't find verb : " + str);
        }
        else
        {
            // remove text from input text to avoid confusion
            // water sprout => il pensait que "water" était l'objet

            // REPLACE ONLY ONCE grâce à ce truc trouvé sur l'internet
            var regex = new Regex(Regex.Escape(verb.Name + " "));

            text = regex.Replace(text, "",1);
        }
    }

    public void FindItems()
    {
        actionOnAll = false;
        items.Clear();

        if (InputInfo.text.Contains(" tous ") || InputInfo.text.Contains(" toutes "))
        {
            Debug.Log("actions on all !!!!");
            actionOnAll = true;
        }

        List<Item> probableItems = new List<Item>();

        foreach (var input_part in InputInfo.parts)
        {
            // ici c'est la longueur minimale de chaine de character pour la detection
            // ( pour éviter que " l' " soit détécté comme n'importe quel mot commençant par " l " dans l'inventaire*
            // la tile ou les mots utilisables n'importe ou
            if (input_part.Length < 3)
                continue;

            Item item = Item.FindInWorld(input_part);

            if  (item != null)
            {
                //Debug.Log("found : " + item.word.text);

                item.inputToFind = input_part;

                //Debug.Log("input to find : " + item.word.text + " is " + input_part);
                probableItems.Add(item);
            }
        }

        if (probableItems.Count == 0)
        {
            return;
        }

        Item mostProbableItem = probableItems[0];

        foreach (var item in probableItems)
        {
            if (item.inputToFind.Length > mostProbableItem.inputToFind.Length)
            {
                mostProbableItem = item;
            }
        }

        items = probableItems;

        //items.Sort((a, b) => a.word.text.Length.CompareTo(b.word.text.Length));

        /*foreach (var item in items)
        {
            Debug.Log("item in input : " + item.word.text);
        }*/

        //items.Add(mostProbableItem);
    }

    public void AddItem( Item item)
    {
        items.Add(item);
    }

    public void FindOrientation()
    {
        orientation = Player.Orientation.None;

        foreach (var inputPart in InputInfo.parts)
        {
            // actuellement ça fonctionne pas trop aprce que "devant à droite" c'est plusieurs parties par exemple
            string[] strs = new string[8]
            {
                "devant",
                "devant à droite",
                "droite",
                "derrière à droite",
                "derrière",
                "derrière à gauche",
                "gauche",
                "devant à gauche" 
            };

            Player.Orientation tmpOrientation = (Player.Orientation)0;

            foreach (var str in strs)
            {
                if (inputPart.Contains(str))
                {
                    orientation = tmpOrientation;
                    //Debug.Log("found orientation : " + orientation.ToString());
                    return;
                }

                ++tmpOrientation;
            }

        }
    }

    public bool HasVerb()
    {
        return verb != null;
    }

    public bool HasItems()
    {
        return items.Count != 0;
    }

    public Combination FindCombination()
    {
        if ( verb == null)
        {
            return null;
        }

        if ( !HasItems())
        {
            return null;
        }

        combination = verb.combinations.Find(x => x.itemIndex == MainItem.index);

        if (combination == null)
        {
            return null;
        }

        return combination;
    }
}
