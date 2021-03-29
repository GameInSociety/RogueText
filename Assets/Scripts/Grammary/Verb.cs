using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verb {

	public int col = 0;

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

   
    public List<Combination> combinations = new List<Combination>();

    public void AddCombination (Combination combination)
    {
        this.combinations.Add(combination);
    }

    

	public Verb() {
		//
	}

	public static Verb Find ( string str ) {

		foreach (var verb in GetVerbs) {

			foreach (var name in verb.names) {
				if (name == str) {
					return verb;
				}

			}

		}

		return null;
	}
}