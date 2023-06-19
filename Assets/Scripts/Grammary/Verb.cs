using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Verb {

	public int col = 0;

    private static List<Verb> _verbs = new List<Verb>();
    public class Sequence
    {
        public int id;
        public string content;
    }
    public List<Sequence> cells = new List<Sequence>();

    public bool HasCell(Item item)
    {
        return cells.Find(x => x.id == item.dataIndex) != null;
    }

    public Sequence GetSequence(Item item)
    {
        return cells.Find(x => x.id == item.dataIndex);
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
   

    public void AddCell (int id, string content)
    {
        Sequence cell = new Sequence();
        cell.id = id;
        cell.content = content;
        cells.Add(cell);
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

    private static Verb current;
    public static Verb GetCurrent
    {
        get
        {
            return current;
        }
    }

    public static void FindVerbInText(string text)
    {
        Verb verb = Get(text);
        SetCurrent(verb);
    }
    public static void SetCurrent(Verb verb)
    {
        DebugManager.Instance.verb = verb;
        current = verb;
    }

	public static Verb Get ( string str ) {

        List<Verb> potentialVerbs = GetVerbs.FindAll(x => x.FoundInText(str));

        if ( potentialVerbs.Count == 0)
        {
            Debug.LogError("no verbs found");
            return null;
        }

        Verb longestVerb = potentialVerbs[0];
        foreach (var item in potentialVerbs)
        {
            if ( item.GetName.Length > longestVerb.GetName.Length)
            {
                longestVerb = item;
            }
        }

        return longestVerb;
    }

    public static Verb FindInData(string str)
    {
        Verb verb = _verbs.Find(x  => x.names[0] == str);

        if ( verb == null)
        {
            Debug.LogError("couldn't find verb in " + str);
            return null;
        }

        return verb;
    }

    public static bool HasCurrent
    {
        get
        {
            return current != null;
        }
    }

    public static void Clear()
    {
        current = null;
    }
}