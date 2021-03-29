using System.Collections.Generic;

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

        if ( line.Count < 4 )
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
        newVerb.names = name.Split(new string[] { " / " }, System.StringSplitOptions.None);
        newVerb.question = line[1];
        newVerb.preposition = line[2];
        newVerb.helpPhrase = line[3];

        Verb.AddVerb(newVerb);

    }
}
