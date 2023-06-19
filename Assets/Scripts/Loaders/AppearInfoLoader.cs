using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AppearInfoLoader : DataDownloader
{
    public static AppearInfoLoader Instance;

    public bool debug = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        if ( cells.Count == 0)
        {
            return;
        }

        if (rowIndex < 2)
        {
            return;
        }

        if (string.IsNullOrEmpty(cells[0]))
        {
            return;
        }
    }
}
