using System.Collections.Generic;
using UnityEngine;

public class PhraseLoader : DataDownloader
{
    public static PhraseLoader Instance;

    public List<Phrase.PhraseType> phraseTypes = new List<Phrase.PhraseType>();
    public List<Phrase.Part> parts = new List<Phrase.Part>();

    public const int partsIndex = 2;
    private void Awake() {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0) {
            for (int i = 0; i < partsIndex; i++) {
                var pt = new Phrase.PhraseType();
                pt.key = cells[i];
                Phrase.phraseTypes.Add(pt);
            }
            for (int i = partsIndex; i < cells.Count; i++) {
                if (!string.IsNullOrEmpty(cells[i]))
                    Phrase.parts.Add(new Phrase.Part(cells[i]));
            }
            return;
        }


        for (int i = 0; i < partsIndex; i++) {
            if (!string.IsNullOrEmpty(cells[i])) {
                var newPhrase = new Phrase(cells[i]);
                newPhrase.groupCount = newPhrase.text.Split("item").Length - 1;
                newPhrase.multItemsCount = newPhrase.text.Split("items").Length - 1;
                newPhrase.singleItemsCount = newPhrase.text.Split("[item]").Length - 1;
                Phrase.phraseTypes[i].phrases.Add(newPhrase);
            }
        }

        for (int i = partsIndex;i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                continue;
            Phrase.parts[i-partsIndex].variants.Add(cells[i]);
        }
    }

    public override void FinishLoading() {
        base.FinishLoading();
        phraseTypes = Phrase.phraseTypes;
        parts = Phrase.parts;
    }
}
