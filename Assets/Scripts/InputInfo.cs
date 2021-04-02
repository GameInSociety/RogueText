using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Verb verb;
    public List<Item> items = new List<Item>();
    public Combination combination;
    public Player.Orientation orientation;
    public Direction direction;

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

    public void FindVerb()
    {
        string str = DisplayInput.Instance.inputParts[0];

        verb = Verb.Find(str);

        if (verb == null)
        {
            Debug.LogError("couldn't find verb : " + str);
        }
    }

    public void FindItems()
    {
        actionOnAll = false;
        items.Clear();

        if (DisplayInput.Instance.text.Contains("tous"))
        {
            actionOnAll = true;
        }

        List<Item> possibleItems = new List<Item>();

        foreach (var inputpart in DisplayInput.Instance.inputParts)
        {
            Item item = Item.FindInWorld(inputpart);
            if  (item != null)
            {
                item.inputToFind = inputpart;
                possibleItems.Add(item);
                Debug.Log("found in world : " + item.word.text);
            }
        }

        if (possibleItems.Count == 0)
        {
            return;
        }

        Item mostProbableItem = possibleItems[0];

        foreach (var item in possibleItems)
        {
            if (item.inputToFind.Length > mostProbableItem.inputToFind.Length)
            {
                mostProbableItem = item;
            }
            //Debug.Log("possible items in phrase : " + item.word.text);
        }

        //Debug.Log("the most probable item is : " + mostProbableItem.word.text);

        items.Add(mostProbableItem);
    }

    public void FindOrientation()
    {
        orientation = Player.Orientation.None;

        foreach (var inputPart in DisplayInput.Instance.inputParts)
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
