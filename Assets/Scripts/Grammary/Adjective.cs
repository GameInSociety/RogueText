using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Adjective {

    public static List<AdjectiveGroup> adjectiveGroups = new List<AdjectiveGroup>();
    public static List<List<Adjective>> adjectives = new List<List<Adjective>>();

    public bool beforeWord = false;
    public string _text;

    public Adjective()
    {

    }

    public Adjective (Adjective copy)
    {
        this.beforeWord = copy.beforeWord;
        this._text = copy._text;
    }

    public enum Type
	{
		Rural,
		Urbain,
        Item,

		Any,
	}

	public string GetContent ( Word.Genre genre , bool plural) {

		string adj = _text;

        if ( Tile.GetCurrent != null)
        {

        }

		if (genre == Word.Genre.Feminine) {

            int a = 0;
            bool foundEnding = false;
            foreach (var ending in AdjectiveLoader.Instance.maleTerminaisons)
            {
                /*Debug.Log("adjective : " + adj);
                Debug.Log("ending : " + ending);*/

                if (adj.EndsWith(ending))
                {
                    adj = adj.Remove(adj.Length - ending.Length);
                    adj += AdjectiveLoader.Instance.femaleTerminaisons[a];
                    foundEnding = true;
                    break;
                }

                ++a;
            }

            if (adj.Length > 1 && !foundEnding && adj[adj.Length-1] != 'e')
            {
                adj += "e";
            }

		}

		if (plural) {
			adj += "s";
		}

		return adj;

	}

    public static List<Adjective> GetAll(string _name)
    {
        AdjectiveGroup adjectiveGroup = adjectiveGroups.Find(x => x.name == _name);


        if (adjectiveGroup == null)
        {
            Debug.LogError("couldn't find adjective group : " + _name);
            adjectiveGroup = adjectiveGroups[0];
        }
        List<Adjective> adjectives = new List<Adjective>(adjectiveGroup.adjectives);
        return adjectives;
    }

    public static Adjective GetRandom ( string _name ) {

        AdjectiveGroup adjectiveGroup = adjectiveGroups.Find(x => x.name == _name);

        if ( adjectiveGroup == null)
        {
            Debug.LogError("couldn't find adjective group : " + _name);
            adjectiveGroup = adjectiveGroups[0];
        }

        return adjectiveGroup.adjectives[Random.Range(0, adjectiveGroup.adjectives.Count)];
	}
}

public class AdjectiveGroup
{
    public string name;
    public List<Adjective> adjectives = new List<Adjective>();
}