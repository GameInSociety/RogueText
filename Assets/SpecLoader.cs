using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpecLoader : DataDownloader
{
    public static SpecLoader Instance;

    private void Awake() {
        Instance = this;
    }

    [System.Serializable]
    public class SpecCategory {
        public SpecCategory(string _name) {
            name = _name;
        }
        public string name;
        public List<string> specs = new List<string>();
        public string GetRandom() {
            return specs[Random.Range(0, specs.Count)];
        }
    }
    public List<SpecCategory> cats = new List<SpecCategory>();
    
    public SpecCategory GetCat(string name) {
        SpecCategory cat = cats.Find(x=> x.name == name);
        if ( cat == null) {
            Debug.Log($"no cats names {name}");
            return null;
        }
        return cat;
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        for (int i = 0; i < cells.Count; i++) {
            if (rowIndex == 0) {
                cats.Add(new SpecCategory(cells[i]));
            } else {
                if (string.IsNullOrEmpty(cells[i]) )
                    continue;
                cats[i].specs.Add(cells[i]);
            }
        }
    }
}
