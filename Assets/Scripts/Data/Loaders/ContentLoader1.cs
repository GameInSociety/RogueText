using System.Collections.Generic;
using UnityEngine;

public class SoundLoader : DataDownloader{
    public static SoundLoader Instance;

    private void Awake() {
        Instance = this;
    }

    public override void Load() {
        base.Load();
    }

    public override void FinishLoading() {
        base.FinishLoading();
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;
    }
}
