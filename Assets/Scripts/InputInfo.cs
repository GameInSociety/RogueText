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
    public string inputText;
    public List<string> parts = new List<string>();

    // phrase content
    public Verb verb;

    public List<Item> items = new List<Item>();

    public Combination combination;

    public Movable.Orientation orientation = Movable.Orientation.None;
    public Cardinal cardinal = Cardinal.None;

    public bool hasValueInText = false;

    public int valueInText = 0;

    public bool actionOnAll = false;

    public bool itemConfusion = false;

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
        inputText = str;

        if (inputText.Length == 0)
        {
            Debug.LogError("input text : is empty");
            return;
        }

        inputText = inputText.TrimEnd(' ');

        // create action

        FindVerb();

        // separate text ( after verb in case phrase changing, verb is removed 
        parts = inputText.Split(new char[3] { ' ', '\'', '\'' }).ToList<string>();

        FindNumber();

        FindOrientation();

        FindItems();

        if ( itemConfusion)
        {
            return;
        }

        if (HasVerb())
        {
            if (HasItems())
            {
                FindCombination(verb, GetItem(0));
            }
            else
            {
                Item verbItem = ItemManager.Instance.GetDataItem("verbe seul");

                FindCombination(verb, verbItem);

                if ( combination != null)
                {
                    items.Add(verbItem);
                }
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

    public bool sustainVerb = false;

    public void FindVerb()
    {
        //string str = InputInfo.parts[0];

        if ( !sustainVerb)
        {
            verb = null;
        }

        sustainVerb = false;


        Verb tmpVerb = Verb.Find(inputText);

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

            inputText = regex.Replace(inputText, "",1);
        }
    }

    public bool sustainItem = false;

    public void FindItems()
    {
        actionOnAll = false;

        if (inputText.Contains(" all ") )
        {
            Debug.Log("actions on all !!!!");
            actionOnAll = true;
        }

        itemConfusion = false;

        foreach (var input_part in parts)
        {
            if (inputText == "it" || inputText == "them")
            {
                sustainItem = true;
            }
        }

        if (sustainItem)
        {
            sustainItem = false;
            return;
        }
        else
        {
            items.Clear();
        }

        foreach (var input_part in parts)
        {
            // ici c'est la longueur minimale de chaine de character pour la detection
            // ( pour éviter que " l' " soit détécté comme n'importe quel mot commençant par " l " dans l'inventaire*
            // la tile ou les mots utilisables n'importe ou
            if (input_part.Length < 3)
                continue;
            
            List<Item> tmpItems = ItemManager.Instance.FindItemsInWorld(input_part);

            if (tmpItems.Count > 1) {


                Item item = SocketManager.Instance.GetSocketItemInText(inputText);

                if (item != null)
                {
                    items.Add(item);
                    return;
                }

                if ( HasVerb())
                {
                    TextManager.WritePhrase("input_itemConfusion", tmpItems[0]);
                    InputInfo.Instance.sustainVerb = true;
                }
                else
                {
                    TextManager.WritePhrase("Which &dog (override)&", tmpItems[0]);
                }
                itemConfusion = true;
            }
            else if (tmpItems.Count == 1)
            {
                items.Add(tmpItems[0]);
            }
        }

        // if no items, search for mentionned sockets
        // maybe temporary ou pas, c'est pour pouvoir ne pas avoir de confusion quand on met "open right door"
        // il allait chercher le mot right, et à priori ça devrait se régler comme ça
        if (items.Count==0)
        {
            Item item = SocketManager.Instance.GetSocketItemInText(inputText);

            if (item != null)
            {
                items.Add(item);
                return;
            }
        }
    }

    public Item FindPreciseItem(List<Item> tmpItems)
    {
        foreach (var item in tmpItems)
        {
            if (!item.word.HasAdjective())
            {
                continue;
            }

            string itemAdjective = item.word.GetAdjective.GetContent(false);

            if (inputText.Contains(itemAdjective))
            {
                Debug.Log("found adjective " + itemAdjective + " for word " + item.debug_name);

                return item;
            }
        }

        // DONT SEARCH FOR ORIENTATIONS ( en tout cas pas pour les tile items )
        // SEACH IN CURRENT SOCKETS

        // search orientations
        /*foreach (var item in tmpItems)
        {
            if (!item.word.HasAdjective())
            {
                continue;
            }

            string itemAdjective = item.word.GetAdjective.GetContent(false);

            if (inputText.Contains(itemAdjective))
            {
                Debug.Log("found adjective " + itemAdjective + " for word " + item.debug_name);

                return item;
            }
        }*/

        return null;
    }

    public void FindOrientation()
    {
        orientation = Movable.Orientation.None;

        for (int i = 0; i < 8; i+=2)
        {
            Movable.Orientation tmpOrientation = (Movable.Orientation)i;

            string orientationText = Coords.GetOrientationWord(tmpOrientation);


            if (inputText.Contains(orientationText))
            {
                orientation = tmpOrientation;
            }
        }
    }

    public bool HasVerb()
    {
        return verb != null && verb.names.Length > 0 && !string.IsNullOrEmpty(verb.GetName);
    }

    public bool HasItems()
    {
        return items.Count != 0;
    }
    public bool HasItem(int i)
    {
        return i < items.Count;
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

        combination = verb.combinations.Find(x => x.itemIndex == _item.dataIndex);
    }
}
