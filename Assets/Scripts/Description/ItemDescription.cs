using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Utilisé pour décrire un meme group d'objet. 
/// Contient un group de slots regroupé par leur type d'objets ou de propriétés
/// </summary>
[System.Serializable]
public class ItemDescription {
    /// <summary>
    /// L'id permet de formet les groupe. Il peut être le nom d'un objet ou un type de Property.
    /// </summary>
    public string id;
    public List<Info> _infos = new List<Info>();
    public List<Info> GetInfos() => _infos;
    public string GetID() => id;

    /// <summary>
    /// an item slot is items grouped by property TYPE ( property names : (material:wooden, orientation: left, size:small etc...)
    /// </summary>
    [System.Serializable]
    public class Info {
        /// <summary>
        /// ID for inspector
        /// </summary>
        public string id;
        /// <summary>
        /// Described Item
        /// </summary>
        public Item _item;

        /// <summary>
        /// Described Properties
        /// </summary>
        public List<Property> props = new List<Property>();

        public Info(Item item) {
            this._item = item;
            id = item.DebugName;
        }
    }

    public ItemDescription(string id) {
        this.id = id;
    }

    public void AddInfo(Item item) {
        var info = new Info(item);
        _infos.Add(info);
    }
    public void AddInfo(Info slot) {
        _infos.Add(slot);
    }

    #region Splitting
    // Renvoie les slots, regroupées par la nature de l'Item.
    public List<ItemDescription> SplitByName() {

        var tmp_itemDescriptions = new List<ItemDescription>();

        foreach (var info in _infos) {
            // Search an existing slot group with matching NAME.
            var itemDescription = tmp_itemDescriptions.Find(x => x.GetID() == info._item.DebugName);
            if (itemDescription == null) {
                itemDescription = new ItemDescription(info._item.DebugName);
                tmp_itemDescriptions.Add(itemDescription);
            }
            itemDescription.AddInfo(info);
        }

        return tmp_itemDescriptions;
    }

    // Sets if each property's description is shared among other slots, or is unique 
    public void SetPropertyLinks() {
        foreach (var info in GetInfos()) {
            foreach (var prop in info.props) {
                var sharedProps = GetInfos().FindAll(x => x.props.Find(p => p.GetDescription() == prop.GetDescription()) != null);
                // If the shared properties are the same amount of the number of slots in the groups, Shared, otherwise, Unique.
                prop.state = sharedProps.Count() == GetInfos().Count() ? Property.DescriptionState.Shared : Property.DescriptionState.Unique;
            }
        }
    }

    // Get the property type that will split the slots
    public string GetPropertyForSplit() {
        var splitProperty = GetInfos().First().props.Find(x => x.state == Property.DescriptionState.Unique);
        if (splitProperty == null)
            return "";
        return splitProperty.name;
    }

    // Create groups for each different description of the target propety.
    public List<ItemDescription> SplitByProperty(string propertyType) {

        var tmp_itemDescriptions = new List<ItemDescription>();

        // Check for shared property types in slots
        foreach (var info in GetInfos()) {
            var refProp = info.props.Find(x => x.name == propertyType);
            // Find matching SlotGroup for property
            var itemDescription = tmp_itemDescriptions.Find(x => refProp.GetDescription() == x.GetID());
            if (itemDescription == null) {
                itemDescription = new ItemDescription(refProp.GetDescription());
                tmp_itemDescriptions.Add(itemDescription);
            }
            itemDescription.AddInfo(info);
        }
        return tmp_itemDescriptions;

    }

    // Remove all unique properties that are not used in the Property Split 
    public void CleanUselessProperties(string propertyType) {
        foreach (var info in GetInfos()) {
            info.props.RemoveAll(x => x.name != propertyType);
        }
    }

    // This removes unique or unused properties from each slot.
    public void CleanProperties() {
        // No need to clean if only one slot
        if (GetInfos().Count == 1)
            return;
        foreach (var slot in GetInfos()) {
            foreach (var prop in slot.props) {
                // Check if all the slots have the same 
                var matchingProps = GetInfos().FindAll(x => x.props.Find(x =>
                // Ignore current prop
                x!=prop &&
                // Check if same prop content
                x.GetDescription() == prop.GetDescription() &&
                // Check if same prop type
                x.name == prop.name
                ) != null);
                Debug.Log($"matching props for [{prop.name}.{prop.GetDescription()}] : {matchingProps.Count}");
                // If not all the slots (-1, minus current prop) have this prop with this content, remove them
                if (matchingProps.Count != GetInfos().Count-1)
                    prop.state = Property.DescriptionState.Remove;
            }
            // Remove all marked props
            int count = slot.props.RemoveAll(x => x.state == Property.DescriptionState.Remove);
        }
    }
    #endregion

    #region grammar & text
    /// <summary>
    /// The / A
    /// </summary>
    public bool definite;
    /// <summary>
    /// The target grammar of the slot
    /// </summary>
    private Property grammar;

    /// <summary>
    /// Get the item text and its properties.
    /// </summary>
    /// <param name="debug"></param>
    /// <returns></returns>
    public string GetText(bool debug = false) {

        grammar = RefItem.GetProp("grammar");

        // The properties that will be displayed.
        var props = _infos.First().props;

        string textprops = GetPropertiesDescription(props) == "" ? "" : $" ({GetPropertiesDescription(props)})";
        var result = $"{GetArticleText(GetItemText())} {GetItemText()}{textprops}";
        return result;

        // get noun
        string item_name = GetItemText();
        // Get props that are not ajdectives ( "You're standing in a clearing" => you're standing in" )
        string props_hook = GetPropertiesDescription(props.FindAll(x => x.HasPart("before article")), false);
        // Get props before word ( "Wooden door" );
        string props_before = GetPropertiesDescription(props.FindAll(x => x.HasPart("before word")), true);
        // Get props after word ( "door is wooden" );
        string props_after = GetPropertiesDescription(props.FindAll(x => !x.HasPart("before word")&& !x.HasPart("before article")), true);

        // Merge 
        props_before = string.IsNullOrEmpty(props_before) ? "" : $"{props_before} ";
        props_after= string.IsNullOrEmpty(props_after) ? "" : $" {props_after}";
        props_hook = string.IsNullOrEmpty(props_hook) ? "" : $"{props_hook} ";
        var phrase = $"{props_before}{item_name}{props_after}";
        phrase = phrase.Trim(' ');

        // get articles
        string article = GetArticleText(phrase);
        // put phrase together
        var text = $"{props_hook}{article} {phrase}";
        text = text.Trim(' ');

        DescriptionManager.Instance.describedItems.Add(RefItem.dataIndex);
        return text;
    }


    public string GetPropertiesDescription(List<Property> props, bool showSetup = false) {
        if (props.Count == 0)
            return "";
        string text = "";

        // get text in properties
        var props_txt = props.Select(x => x.GetDisplayDescription()).ToList();
        props_txt.RemoveAll(x => string.IsNullOrEmpty(x));
        if (props_txt.Count == 0)
            return "";

        // add properties
        for (int i = 0; i < props_txt.Count; i++) {
            var setup = props[i].GetContent("description setup");
            if (setup == "start") {
                if (RefItem.dataIndex == Tile.GetCurrent.dataIndex) {
                    setup = "continue";
                }
            }

            // on verra
            if (setup == "is")
                setup = MultipleItems() ? "are" : "is";

            // trim for some reason
            props_txt[i]= props_txt[i].Trim(' ');

            // aggregate prop texts
            if (showSetup) {
                var b = showSetup && i > 0 && props[i - i].GetContent("description setup") == props[i].GetContent("description setup");
                string hook = b ? "" : $"{setup} ";
                text += $"{hook}{props_txt[i]}{TextUtils.GetCommas(i, props_txt.Count)}";
            } else {
                text += $"{props_txt[i]}{TextUtils.GetCommas(i, props_txt.Count)}";

            }
        }
        return text.Trim(' ');
    }

    bool MultipleItems() {
        if (grammar.HasPart("number"))
            return grammar.GetPart("number").content == "plural";
        return _infos.Count > 1;
    }

    string GetItemText() {
        // check self ref
        // example : the player => you
        if (grammar != null) {
            if (grammar.HasPart("self ref")) {
                var selfRef = grammar.GetContent("self ref");
                // THEN = first time described ( in current description ) and the other times
                // ex : (undead THEN he) => the undead walks, he hits you
                if (selfRef.Contains("THEN")) {
                    var split = selfRef.Split(" THEN ");
                    selfRef = FirstTimeDescribed() ? split[0] : split[1];
                }
                // X = nothing
                if (selfRef == "X")
                    return "";
                // normal = the default name ( ex:undead )
                if (selfRef != "normal")
                    return selfRef;
            }
        }

        string dog = MultipleItems() ? "dogs" : "dog";
        return RefItem.GetText($"{dog}");
    }

    // ???
    bool FirstTimeDescribed() {
        return !DescriptionManager.Instance.describedItems.Contains(RefItem.dataIndex);
    }

    /// <summary>
    /// Getting the main item of the slots
    /// </summary>
    public Item RefItem {
        get {
            return _infos.First()._item;
        }
    }

    private string GetArticleText(string text) {
        var article = "";
        var grammar = RefItem.GetProp("grammar");
        if (grammar.HasPart("article")) {
            article = grammar.GetContent("article");
            if (article.Contains(" THEN ")) {
                var split = article.Split(" THEN ");
                if (FirstTimeDescribed()) {
                    // first time
                    article = split[0];
                } else {
                    // second time
                    article = split[1];
                }
            }

            if (article == "X") {
                return "";
            }
            if (article != "normal") {
                return article;
            }
        }

        if (grammar.HasPart("definite"))
            definite = true;

        return definite ? "the" : (MultipleItems() ? Phrase.Part.GetRandom("article_mult") : (StartWithVowel(text) ? "an" : "a"));
    }

    private bool StartWithVowel(string text) {
        char[] vowels = new char[] {
            'a','e','i','o','u'
        };
        int i = text.Trim(' ').IndexOfAny(vowels);
        return i == 0;
    }
    #endregion
}