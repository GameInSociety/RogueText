using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spec {

    // key      = what will be searched in the input => "red", "left" "player", "field"
    // value    = how it will appear in the text => "red", "on the left", "my", "in the field"
    // info     = a key to distinct in code => "ordinal" "container" to see if a spec is already assigned

    public static List<Category> categories = new List<Category>();

    [System.Serializable]
    public class Category {
        public Category(string _name) {
            name = _name;
        }
        public string name;
        public List<string> specs = new List<string>();
        public List<string> tmp_specs = new List<string>();
        public string GetRandomSpec() {
            if (tmp_specs.Count == 0) {
                tmp_specs = new List<string>(specs);
            }

            int rnd = Random.Range(0, tmp_specs.Count);
            string str = tmp_specs[rnd];
            tmp_specs.RemoveAt(rnd);
            return str;
        }
    }

    public static Category GetCat(string name) {
        Category cat = categories.Find(x => x.name == name);
        if (cat == null) {
            Debug.Log($"no cats names {name}");
            return null;
        }
        return cat;
    }

    public Spec(string _searchValue, string _displayValue, string _key, bool _visible) {
        searchValue = _searchValue; displayValue = _displayValue; key = _key; this.visible = _visible;
    }
    public bool Null() {
        return string.IsNullOrEmpty(key);
    }
    public string key;
    public string displayValue;
    public string searchValue;
    public bool visible;
}