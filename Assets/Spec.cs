[System.Serializable]
public class Spec {

    // key      = what will be searched in the input => "red", "left" "player", "field"
    // value    = how it will appear in the text => "red", "on the left", "my", "in the field"
    // info     = a key to distinct in code => "ordinal" "container" to see if a spec is already assigned

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