using System.Collections.Generic;
using UnityEngine;

public class VerbLoader : TextParser
{
    public static VerbLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int line_Index, List<string> line)
    {
        base.GetCell(line_Index, line);

        if (line.Count < 3)
        {
            return;
        }

        Verb newVerb = new Verb();

        string name = line[0];

        // check if universal
        if (name[0] == '(')
        {
            name = name.Remove(0, 1);
            newVerb.universal = true;
        }

        // parse all verb synonmys
        newVerb.names = name.Split('\n');
        newVerb.question = line[1];
        newVerb.preposition = line[2];
        if ( line.Count > 3)
        {
            newVerb.universal = line[3] == "universal";
        }

        Verb.AddVerb(newVerb);

    }
}
