using System.Collections.Generic;
using UnityEngine;

public class PhraseLoader : DataDownloader
{
    public static PhraseLoader Instance;

    public List<Phrase.PhraseType> phraseTypes = new List<Phrase.PhraseType>();
    public List<Phrase.Part> parts = new List<Phrase.Part>();

    public const int partsIndex = 0;
    private void Awake() {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0) {
            for (int i = 0; i < partsIndex; i++) {
                var pt = new Phrase.PhraseType();
            }
            for (int i = partsIndex; i < cells.Count; i++) {
                if (!string.IsNullOrEmpty(cells[i]))
                    Phrase.parts.Add(new Phrase.Part(cells[i]));
            }
            return;
        }

        for (int i = partsIndex;i < cells.Count; i++) {
            if (string.IsNullOrEmpty(cells[i]))
                continue;
            Phrase.parts[i-partsIndex].variants.Add(cells[i]);
        }
    }

    public override void FinishLoading() {
        base.FinishLoading();
        parts = Phrase.parts;
    }
}
