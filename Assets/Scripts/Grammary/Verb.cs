using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Verb {

	public int col = 0;

    private static List<Verb> _verbs = new List<Verb>();
    public List<Combination> cellEvents = new List<Combination>();

    public bool HasFunctionList(Item item)
    {
        return cellEvents.Find(x => x.itemIndex == item.dataIndex) != null;
    }

    public string GetFunctionList(Item item)
    {
        return cellEvents.Find(x => x.itemIndex == item.dataIndex).content;
    }

    public static List<Verb> GetVerbs
    {
        get
        {
            return _verbs;
        }
    }

    public static void AddVerb(Verb verb)
    {
        _verbs.Add(verb);
    }

    public string question = "";
    public string[] prepositions;

    public string helpPhrase = "";

    public bool universal = false;

	public string[] names;
    public int currentNameIndex = 0;
    public string GetName
    {
        get
        {
            return names[currentNameIndex];
        }
    }
    public string GetPreposition
    {
        get
        {
            if ( currentNameIndex >= prepositions.Length)
            {
                return prepositions[0];
            }

            return prepositions[currentNameIndex];
        }
    }
   

    public void AddCombination (Combination combination)
    {
        this.cellEvents.Add(combination);
    }

    

	public Verb() {
		//
	}

    public bool FoundInText(string text)
    {
        int _nameIndex = 0;
        foreach (var verb_Name in names)
        {
            string boundedVerb = @$"\b{verb_Name}\b";
            // get all the verbs
            if (Regex.IsMatch(text, boundedVerb))
            {
                currentNameIndex = _nameIndex;
                return true;
            }

            ++_nameIndex;
        }

        return false;
    }

	public static Verb Find ( string input_text ) {


        List<Verb> possibleVerbs = new List<Verb>();

        int smallestIndex = 300;

        foreach (var verb in GetVerbs) {

            // get all the verbs
            if (verb.FoundInText(input_text))
            {
                int index = input_text.IndexOf(verb.GetName);
                if (index < smallestIndex)
                {
                    possibleVerbs.Clear();
                    smallestIndex = index;
                }
                possibleVerbs.Add(verb);
                goto NextVerb;
            }

            NextVerb:
            continue;

		}

        // ne marche pas trop 
        // exemple "look at watering can", il prend pense que c'est "water watering can"
        // donc on passe à la position du mot dans la phrase

        // list of verb found in the phrase
        if ( possibleVerbs.Count > 0)
        {
            return possibleVerbs[0];

            /*Verb longestVerb = possibleVerbs[0];

            // find longest verb
            foreach (var verb in possibleVerbs)
            {
                if ( verb.GetName.Length > longestVerb.GetName.Length)
                {
                    longestVerb = verb;
                }
            }

            return longestVerb;*/
        }

        return null;
	}
}