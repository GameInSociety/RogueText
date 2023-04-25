using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verb {

	public int col = 0;

    public string inputToFind = "";
    public int indexToFind = -1;

    private static List<Verb> _verbs = new List<Verb>();
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
    public string preposition = "";

    public string helpPhrase = "";

    public bool universal = false;

	public string[] names;
    public int currentNameIndex = 0;
    public string Name
    {
        get
        {
            return names[currentNameIndex];
        }
    }
   
    public List<Combination> combinations = new List<Combination>();

    public void AddCombination (Combination combination)
    {
        this.combinations.Add(combination);
    }

    

	public Verb() {
		//
	}

	public static Verb Find ( string fullPhrase ) {

        List<Verb> possibleVerbs = new List<Verb>();
        List<int> possibleVerbsIndex = new List<int>();

        foreach (var verb in GetVerbs) {

            int synonymIndex = 0;
			foreach (var verb_Name in verb.names) {
                // puttin " " + verb_name + " " to separate with words
                if (fullPhrase.Contains(verb_Name))
                {
                    verb.indexToFind = fullPhrase.IndexOf(verb_Name);
                    possibleVerbs.Add(verb);
                    goto NextVerb;
				}

                ++synonymIndex;

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
            Verb firstVerb = possibleVerbs[0];

            // find longest input used to find the verb
            // "input to find" can also be reused when mentionning verbs
            // that way if the player says "light", it will not say "turn on"
            foreach (var verb in possibleVerbs)
            {
                if ( verb.indexToFind < firstVerb.indexToFind)
                {
                    firstVerb = verb;
                }
            }

            return firstVerb;
        }

		return null;
	}
}