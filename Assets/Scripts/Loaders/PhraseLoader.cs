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

        if (cells.Count < 2) {
            return;
        }

        PhraseKey newPhrase = new PhraseKey();
        newPhrase.key = cells[0];

        string[] parts = cells[1].Split('\n');

        PhraseKey.phraseKeys.Add(newPhrase);

        foreach (var part in parts)
        {
            PhraseKey.phraseKeys[PhraseKey.phraseKeys.Count - 1].values.Add(part);
        }


    }
}
