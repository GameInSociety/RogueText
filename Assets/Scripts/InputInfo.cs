using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using System.Reflection;

[System.Serializable]
public class InputInfo : MonoBehaviour
{
    public static InputInfo Instance;

    // static
    public string inputText;

    // phrase content
    public Verb verb;

    public Combination combination;

    public List<Item> potentialItems = new List<Item>();

    public bool hasValueInText = false;

    public int valueInText = 0;

    public bool itemConfusion = false;

    public bool waitForVerb = false;
    public bool waitForItem = false;

    private void Awake()
    {
        Instance = this;
    }

    public void WaitForVerb()
    {
        waitForVerb = true;
    }

    public void WaitForItem()
    {
        waitForItem = true;
    }

    public void ParseText(string str)
    {
        inputText = str;

        if (string.IsNullOrEmpty(inputText))
        {
            Debug.Log("input is empty");
            return;
        }

        inputText = inputText.TrimEnd(' ');


        if (waitForVerb)
        {
            FindVerb();
            if ( !VerbInInput())
            {
                ClearItems();
            }
            waitForVerb = false;
        } else if (waitForItem)
        {
            FindItems();
            if (!ItemsInInput())
            {
                ClearVerb();
            }
            waitForItem = false;
        }
        else
        {
            FindAll();
        }

        DisplayInputFeedback();

        if ( !VerbInInput() || !ItemsInInput())
        {
            return;
        }

        FunctionManager.CallFunctions();
    }

    private void FindAll()
    {
        FindNumber();

        FindVerb();

        FindItems();

    }


    void ClearItems()
    {
        potentialItems.Clear();
    }

    void ClearVerb()
    {
        verb = null;
    }

    void DisplayInputFeedback()
    {
        if (itemConfusion)
        {
            if (VerbInInput())
            {
                TextManager.Write("input_itemConfusion", potentialItems[0]);
                //WaitForVerb();
            }
            else
            {
                TextManager.Write("Which &dog (override)&", potentialItems[0]);
            }
            return;
        }

        // check if ANYTHING has been recognized
        if (!ItemsInInput() && !VerbInInput())
        {
            TextManager.Write("input_nothingRecognized");
            return;
        }

        // check if verb has been recognized
        if (!VerbInInput())
        {
            WaitForVerb();
            TextManager.Write("input_noVerb", potentialItems[0]);
            return;
        }
        
        // check if item has been recognized
        if (VerbInInput() && !ItemsInInput())
        {
            WaitForItem();
            TextManager.Write("input_noItem");
            return;
        }



        
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


    public void FindVerb()
    {

        ClearVerb();

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


    public void FindItems()
    {
        if (!waitForItem)
        {
            ClearItems();
        }

        itemConfusion = false;

        List<Item> availableItems = AvailableItems.GetItems.FindAll(x => x.ContainedInText(inputText));
        potentialItems.AddRange(availableItems);

        if (potentialItems.Count > 1)
        {
            // there is more than one item detected in the input text

            if (SameContainer())
            {

                // check if all items are the same
                if (potentialItems.TrueForAll(x => x.debug_name == potentialItems.First().debug_name))
                {
                    if (potentialItems[0].word.defaultNumber == Word.Number.Plural)
                    {
                        FunctionManager.SetItems(potentialItems);
                    }
                    else
                    {
            // the items all come from the same container, so take first one
                        FunctionManager.SetItem(potentialItems.First());
                    }

                    return;
                }

                FunctionManager.SetItems(potentialItems);
                //Function.AddItem(potentialItems[0]);
                return;
            }

            // they come from different containers
            Item specItem = GetSpecificItem();
            if ( specItem != null)
            {
                FunctionManager.AddItem(specItem);
                return;
            }

            // there are no specific item in the input, so throw item confusion

            itemConfusion = true;
            WaitForItem();
            return;
        }

        if (potentialItems.Count == 1)
        {
            // no confusion, no need for specification
            FunctionManager.SetItem(potentialItems[0]);
        }

        // try verb alone
        if ( potentialItems.Count == 0 && VerbInInput())
        {
            Item verbItem = ItemManager.Instance.GetDataItem("verbe seul");
            if (verb.HasFunctionList(verbItem))
                FunctionManager.SetItem(verbItem);
        }
    }

    Item GetSpecificItem()
    {
        foreach (var containerItem in potentialItems)
        {
            foreach (var specificItem in potentialItems)
            {
                if (containerItem.HasItem(specificItem))
                {
                    Debug.Log(containerItem.debug_name + " has item " + specificItem.debug_name);
                    return specificItem;
                }
            }
        }

        return null;
    }

    bool SameContainer()
    {
        // check if items come from different container
        Item containedItem = null;
        foreach (var item in AvailableItems.GetItems)
        {
            if ( potentialItems.Find(x=> item.HasItem(x) ) != null)
            {
                if (containedItem == null)
                {
                    containedItem = item;
                }
                else
                {
                    if (containedItem != item)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public bool VerbInInput()
    {
        return verb != null && verb.names.Length > 0 && !string.IsNullOrEmpty(verb.GetName);
    }

    public bool ItemsInInput()
    {
        return potentialItems.Count > 0;
    }

    public void FindFunctionList(Verb _verb, Item _item)
    {
        if ( verb == null)
        {
            return;
        }

        if ( _item == null)
        {
            return;
        }

        combination = verb.cellEvents.Find(x => x.itemIndex == _item.dataIndex);
    }
}
