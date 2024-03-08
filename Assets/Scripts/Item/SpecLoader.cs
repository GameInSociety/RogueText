using System.Collections.Generic;

public class SpecLoader : DataDownloader
{
    public static SpecLoader Instance;

    private void Awake() {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        for (int i = 0; i < cells.Count; i++) {
            if (rowIndex == 0) {
                Spec.categories.Add(new Spec.Category(cells[i]));
            } else {
                if (string.IsNullOrEmpty(cells[i]) )
                    continue;
                Spec.categories[i].specs.Add(cells[i]);
            }
        }
    }
}
