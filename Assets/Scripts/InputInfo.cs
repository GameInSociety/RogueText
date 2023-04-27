using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.ConstrainedExecution;
using System.Text;

[System.Serializable]
public class InputInfo : MonoBehaviour
{
    public static InputInfo Instance;
   

    // static
    public string text;
    public List<string> parts = new List<string>();

    // phrase content
    public Verb verb;

    public List<Item> items = new List<Item>();

    public Combination combination;

    public Player.Orientation orientation = Player.Orientation.None;
    public Cardinal cardinal = Cardinal.None;

    public bool hasValueInText = false;

    public int valueInText = 0;

    public bool actionOnAll = false;

    private void Awake()
    {
        Instance = this;
    }

    public Item GetItem(int index)
    {
        if (items.Count == 0)
        {
            return null;
        }

        if (index >= items.Count)
        {
            Debug.LogError("index " + index + " out of list of input items ( " + items.Count + " )");
            return GetItem(0);
        }

        return items[index];
    }


    public void ParseText(string str)
    {
        text = str;

        if (text.Length == 0)
        {
            Debug.LogError("input text : is empty");
            return;
        }

        text = text.TrimEnd(' ');

        // create action

        FindVerb();

        // separate text ( after verb in case phrase changing, verb is removed 
        parts = text.Split(new char[3] { ' ', '\'', '\'' }).ToList<string>();

        FindNumber();

        FindOrientation();

        FindItems();

        if (HasVerb())
        {
            if (HasItems())
            {
                FindCombination(verb, GetItem(0));
            }
            else
            {
                Item verbItem = Item.GetDataItem("verbe seul");
                FindCombination(verb, verbItem);
            }
        }
        else
        {
            combination = null;
        }


        PlayerActionManager.Instance.DisplayInputFeedback();
    }

    private void FindNumber()
    {
        hasValueInText = false;

        foreach (var part in parts)
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

        Verb tmpVerb = Verb.Find(text);

        if (tmpVerb == null)
        {
            // c'est pas grave de pas avoir de verbe
            //Debug.LogError("couldn't find verb : " + str);
        }
        else
        {
            verb = tmpVerb;
            // remove text from input text to avoid confusion
            // water sprout => il pensait que "water" était l'objet

            // REPLACE ONLY ONCE grâce à ce truc trouvé sur l'internet
            var regex = new Regex(Regex.Escape(verb.GetName + " "));

            text = regex.Replace(text, "",1);
        }
    }

    public void FindItems()
    {
        actionOnAll = false;

        if (text.Contains(" all ") )
        {
            Debug.Log("actions on all !!!!");
            actionOnAll = true;
        }

        List<Item> probableItems = new List<Item>();

        foreach (var input_part in parts)
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

        /// if no probable items, we KEEP the previous items, for feature enter IT, look at IT
        /// input preservation on peut appeler ça
        if (probableItems.Count == 0)
        {
            return;
        }

        items.Clear();


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

    public void FindOrientation()
    {
        orientation = Player.Orientation.None;

        for (int i = 0; i < 8; i++)
        {
            Player.Orientation tmpOrientation = (Player.Orientation)i;

            if (text.Contains(Coords.GetOrientationText(orientation)))
            {
                orientation = tmpOrientation;
            }
        }
    }

    public bool HasVerb()
    {
        return !string.IsNullOrEmpty(verb.GetName);
    }

    public bool HasItems()
    {
        return items.Count != 0;
    }

    public void FindCombination(Verb _verb, Item _item)
    {
        if ( verb == null)
        {
            return;
        }

        if ( _item == null)
        {
            return;
        }

        combination = verb.combinations.Find(x => x.itemIndex == _item.index);
    }
}
