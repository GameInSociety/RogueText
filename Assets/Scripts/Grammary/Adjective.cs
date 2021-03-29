using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adjective {

    public static List<List<Adjective>> adjectives = new List<List<Adjective>>();

    public bool beforeWord = false;
    public string _text;

    public enum Type
	{
		Rural,
		Urbain,
        Item,

		Any,
	}

	public string GetContent ( Word.Genre genre , Word.Number num) {

		string adj = _text;

		if (genre == Word.Genre.Feminine) {

            int a = 0;
            bool foundEnding = false;
            foreach (var ending in AdjectiveLoader.Instance.maleTerminaisons)
            {
                if (adj.EndsWith(ending))
                {
                    adj = adj.Replace(ending, AdjectiveLoader.Instance.femaleTerminaisons[a]);
                    foundEnding = true;
                    break;
                }

                ++a;
            }

            if ( !foundEnding && adj[adj.Length-1] != 'e')
            {
                adj += "e";
            }

		}

		if (num == Word.Number.Plural) {
			adj += "s";
		}

		return adj;

	}


	public static Adjective GetRandom ( Adjective.Type type ) {
		return adjectives [(int)type] [Random.Range (0, adjectives [(int)type].Count)];
	}
}
