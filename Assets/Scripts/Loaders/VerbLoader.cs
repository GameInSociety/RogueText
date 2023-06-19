using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class VerbLoader : TextParser
{
    public static VerbLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int line_Index, List<string> cells)
    {
        base.GetCell(line_Index, cells);

        if (cells.Count < 3)
        {
            return;
        }

        Verb newVerb = new Verb();


        if ( string.IsNullOrEmpty(cells[0]) )
        {
            return;
        }
        // parse all verb synonmys
        char[] charsToTrim = { '\n', '\t', '\r', ' ' };
        string[] names = cells[0].Split('\n');

        /*foreach (var item in names)
        {
            Debug.Log(line_Index + item);
        }*/
        
        newVerb.names = new string[names.Length];

        for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {
            newVerb.names[nameIndex] = names[nameIndex].TrimEnd(charsToTrim);
        }

        newVerb.question = cells[1];
        newVerb.prepositions = cells[2].Split('\n');
        for (int i = 0; i < newVerb.prepositions.Length; i++)
        {
            if (newVerb.prepositions[i] == "*" )
            {
                newVerb.prepositions[i] = newVerb.prepositions[i].Remove(0, 1);
            }
        }

        Verb.AddVerb(newVerb);

    }
}
