using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhraseLoader : TextParser
{
    public static PhraseLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if ( cells[0].Length > 0)
        {
            Phrase newPhrase = new Phrase();
            newPhrase.key = cells[0];


            Phrase.phrases.Add(newPhrase);
        }

        Phrase.phrases[Phrase.phrases.Count-1].values.Add(cells[1]);

    }
}
