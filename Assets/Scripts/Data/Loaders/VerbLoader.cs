using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class VerbLoader : TextParser {
    public static VerbLoader Instance;

    public Verb[] verbs_debug;

    private void Awake() {
        Instance = this;
    }

    public override void GetCell(int line_Index, List<string> cells) {
        base.GetCell(line_Index, cells);

        if (cells.Count < 3) {
            return;
        }

        var newVerb = new Verb();


        if (string.IsNullOrEmpty(cells[0])) {
            return;
        }
        // parse all verb synonmys
        char[] charsToTrim = { '\n', '\t', '\r', ' ' };
        var names = cells[0].Split('\n');

        /*foreach (var item in contents)
        {
            Debug.Log(line_Index + item);
        }*/

        newVerb.debug_word = names[0];
        newVerb.words = new string[names.Length];

        for (var nameIndex = 0; nameIndex < names.Length; nameIndex++) {
            newVerb.words[nameIndex] = names[nameIndex].TrimEnd(charsToTrim);
        }

        newVerb.question = cells[1];
        newVerb.prepositions = cells[2].Split('\n');
        for (var i = 0; i < newVerb.prepositions.Length; i++) {
            if (newVerb.prepositions[i] == "*") {
                newVerb.prepositions[i] = newVerb.prepositions[i].Remove(0, 1);
            }
        }

        Verb.AddVerb(newVerb);

    }

    public override void FinishLoading() {
        base.FinishLoading();

        VerbLoader.Instance.verbs_debug = Verb.verbs.ToArray();
    }
}
