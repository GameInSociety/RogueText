using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[System.Serializable]
public class Property {
    public static List<Property> sharedProps = new List<Property>();
    public string name;
    public bool updated = false;
    public bool enabled = false;
    public bool destroy = false;
    private Item _linkedItem;
    public bool debug_selected = false;

    /// <summary>
    /// STRICTLY FOR TESTING (workaround before having object handling Description of properties)
    /// </summary>
    public DescriptionState state;
    public enum DescriptionState {
        Shared, // The property is shared with the rest of the group
        Unique, // The property is not present in all of the slots
        Remove, // The slots needs to be removed. ( redondant ? )
    }

    public List<Part> parts = new List<Part>();

    public class Part {
        public string key = "";
        public string content = "";
        public Part(string key, string content) {
            this.key = key;
            this.content = content;
        }
    }

    public static Property GetDataProp(string name) {
        var prop = sharedProps.Find(x=> x.name == name);
        if ( name == "contents") {
            Debug.Log($"pourquoi ?");
        }
        if (prop == null) {
            Debug.Log($"no prop with the name : {name}");
        }
        return prop;
    }

    public static void AddPropertyData(Property prop) {
        sharedProps.Add(prop);
    }

    public void Init(Item item) {

        _linkedItem = item;

        bool startEnable = true;
        // enable / disable
        if (name.StartsWith('*')) {
            name = name.Substring(1);
            if (name.StartsWith('?')) {
                var percent = TextUtils.Extract('?', name, out name);
                float r = UnityEngine.Random.value * 100f;
                startEnable = r < int.Parse(percent.Trim('%'));
            } else {
                startEnable = false;
            }
        }

        if (name.Contains('/')) {
            var split = name.Split('/');
            name = split[0];
            for (int i = 1; i < split.Length; i++) {
                AddPart("key", split[i].Trim(' '));
            }
        }


        // enable
        if (startEnable) Enable(); else Disable();

        // v & d
        InitPart("value");
        InitDescription();
    }
    
    

    public void Enable() {
        if (enabled)
            return;
        enabled = true;
        var eventParts = parts.FindAll(x => x.key.StartsWith('E'));
        foreach (var e in eventParts) {
            string eventName = e.key.Remove(0, 2);
            string sequence = e.content;
            WorldEvent.Unsubscribe(eventName, _linkedItem, this, sequence);
        }
    }

    public void Disable() {
        if (!enabled)
            return;
        enabled = false;
        var eventParts = parts.Where(x => x.key.StartsWith('E'));
        foreach (var e in eventParts) {
            string eventName = e.key.Remove(0, 2);
            string sequence = e.content;
            WorldEvent.Unsubscribe(eventName, _linkedItem, this, sequence);
        }
    }

    public void Parse(string[] lines) {
        name = lines[0];
        // no parts, only name
        if (lines.Length == 1) return;

        char[] chars = { '\r', '\t', '\b', '\n', ' ' };
        for (int i = 1; i < lines.Length; i++) {
            try {
                // skip empty lines
                if (string.IsNullOrEmpty(lines[i])) continue;

                if (lines[i].Contains(':')) {
                    // new part
                    var strs = lines[i].Split(':');
                    var text = strs[1].Trim(chars);
                    AddPart(strs[0], text);

                } else {
                    // continue part
                    var str = lines[i].Trim(chars);
                    var part = parts.Last();
                    part.content += string.IsNullOrEmpty(part.content) ? str : $"\n{str}";
                }
            } catch (Exception e) {
                TextManager.Write($"[Error Loading Properties] at part [ {lines[i]} ]", Color.red);
                Debug.LogException(e);
                
            }
        }
    }

    #region parts
    // adding
    public void AddPart(string key, string content) {
        parts.Add(new Part(key,content));
    }

    // setting
    public void SetPart(string key, string content) {
        if (!HasPart(key))
            AddPart(key, content);
        else
            SetContent(key, content);
    }

    // removing
    public void RemovePart(string key) {
        int index = parts.FindIndex(x=> x.key == key);
        parts.RemoveAt(index);
    }

    // checking
    public bool HasPart(string key) {
        return parts.Find(x => x.key == key) != null;
    }
    public Part GetPart(string key) {
        return parts.Find(x=> x.key.Equals(key));
    }
    public void SetContent(string key, string content) {
        GetPart(key).content = content;
    }
    public string GetContent(string key) {

        if (!HasPart(key)) {
            Debug.LogError($"ITEM ({_linkedItem.DebugName}) doesn't have PART ({key}) on PROP ({name})");
            return "error";
        }
        string content = GetPart(key).content;

        // if the key starts with an asterisk, the link will happen everytime
        bool keepLink = content.StartsWith('*');
        string result = LinePart.ParseBrackets(content, _linkedItem);
        if ( !keepLink) {
            GetPart(key).content = result;
        }
        return result;
    }
    #endregion

    #region value
    public void InitPart(string key) {

        if (!HasPart(key))
            return;


        var newContent = GetContent(key);

        if (newContent.Contains("MAX")) {
            var split = newContent.Split("MAX");
            newContent = split[0].Trim (' ');
            SetPart("max", split[1].Trim(' '));
        }

        if ( newContent.Contains('?') ) {
            var split = newContent.Split('?');
            if ( split.Length <= 2 && int.TryParse(split[0], out _)) {
                int min = int.Parse(split[0]);
                int max = int.Parse(split[1]);
                int i = UnityEngine.Random.Range(min, max);
                newContent = $"{i}";
            }else
                newContent = split[UnityEngine.Random.Range(0, split.Length)].Trim(' ');
            
        }
        
        if (newContent.StartsWith('{')) {
            string outText = "";
            var s = TextUtils.Extract('{', newContent, out outText);
            var parts = s.Split('/');
            switch (parts[0]) {
                case "type":
                    string type = ItemData.GetItemData(ItemData.GetRandomDataOfType(parts[1])).name;
                    Debug.Log($"setting value of property {name} to {type}");
                    newContent = type;
                    break;
                case "adj":
                    break;
                case "new tileset":
                    int tilesetId = TileSet.tileSets.Count;
                    newContent = tilesetId.ToString();
                    var tileSet = TileSet.NewTileset();
                    Interior.InitTileSet(_linkedItem, tileSet);
                    break;
                default:
                    break;
            }
        }

        SetContent(key, newContent);
    }
    public string GetTextValue(string key = "value") {
        return GetContent(key);
    }
    // value can be number or seq. but it's alway dynamic
    public bool HasNumValue(string key = "value") {
        return HasPart(key) && GetNumValue(key) > -1;
    }
    public int GetNumValue(string key = "value") {
        int num = 0;
        if (!int.TryParse(GetContent(key), out num))
            return -1;
        return num;
    }
    public void SetValue(string content, bool setChanged = true) {
        SetContent("value", content);
        if ( setChanged)
            SetChanged();

    }
    public void SetValue(int num) {
        if (HasPart("max"))
            num = Math.Clamp(num, 0, GetNumValue("max"));
        if (num < 0)
            num = 0;

        SetContent("value", num.ToString());
        SetChanged();
    }
    public void SetChanged() {
        // if the value has been changed, so that the item description displays it
        updated = true;
        CheckValueEvents();
        if ( WorldAction.active != null) {
            PropertyDescription.Add(_linkedItem, this, WorldAction.active.source, false);
        }
    }
    #endregion

    #region on value
    public enum OnValueCondition {
        Above,
        Below,
        Equals
    }
    void CheckValueEvents() {
        if (!enabled)
            return;

        var valueEvents = parts.FindAll(x => x.key == "OnValue");
        if (valueEvents.Count == 0)
            return;

        foreach (var valueEvent in valueEvents) {
            var content = valueEvent.content;

            Debug.Log($"Looking at On Value : {valueEvent.key}/{valueEvent.content}");

            // getting the condition
            var returnIndex = content.IndexOf('\n');
            var condition_text = content.Remove(returnIndex);
            // 

            var propValue = GetNumValue();
            var targetValue = 0;

            bool call = false;

            var onValCondition = OnValueCondition.Equals;
            if (condition_text.StartsWith("ABOVE")) {
                onValCondition = OnValueCondition.Above;
                condition_text = condition_text.Remove(0, "ABOVE".Length);
            }
            if (condition_text.StartsWith("BELOW")) {
                onValCondition = OnValueCondition.Below;
                condition_text = condition_text.Remove(0, "BELOW".Length);
            }
            condition_text = condition_text.Trim(' ');

            if (condition_text.StartsWith('[')) {
                var key = TextUtils.Extract('[', condition_text, out condition_text);
                var onValuePart = LinePart.Parse(key, _linkedItem, "OnValue Condition");
                targetValue = onValuePart.prop.GetNumValue();
            } else {
                targetValue = int.Parse(condition_text);
            }


            switch (onValCondition) {
                case OnValueCondition.Above:
                    call = propValue > targetValue;
                    break;
                case OnValueCondition.Below:
                    call = propValue < targetValue;
                    break;
                case OnValueCondition.Equals:
                    call = propValue == targetValue;
                    break;
            }

            // getting the sequence
            var sequence = content.Remove(0, returnIndex + 1);
            if (call) {
                Debug.Log("Call");
                var propertyEvent = new WorldAction(_linkedItem, sequence, $"On Value:{name}");
                propertyEvent.StartSequence(WorldAction.Source.Event);
            } else {
                Debug.Log("No call");
            }

        }
    }
    #endregion

    #region description
    void InitDescription() {
        if (!HasPart("description"))
            return;

        string description = GetPart("description").content;
        int typeIndex = description.IndexOf('(');
        int b = 0;
        while (typeIndex >= 0) {
            var type = TextUtils.Extract('(', description, out description);
            if (type == "strict") {
                SetPart("strict", "y");
            } else if ( type == "b") {
                SetPart("before", "y");
            }
            else if (type.StartsWith("s/")) {
                var setup = type.Remove(0, 2);
                SetPart("description setup", setup == "x" ? "" : setup);
            } else
                SetPart("description type", type);

            typeIndex = description.IndexOf('(');
            ++b;
            if (b == 10) {
                Debug.LogError($"description init break");
                break;
            }
        }
        if (!HasPart("description setup")) {
            SetPart("description setup", "is");
        }
        if (!HasPart("description type"))
            SetPart("description type", "none");

        GetPart("description").content = description;
    }
    public string GetDisplayDescription() {

        /*if (!enabled) {
            if (updated)
                return $"no longer {GetCurrentDescription()}";
            else return "";
        }*/

        if (!updated)
            return GetDescription();

        var newDescription = GetDescription();
        var result = "";
        if (HasPart("current description")) {
            if (newDescription != GetContent("current description"))
                result = $"now {newDescription}";
            else
                result = $"still {newDescription}";
        } else
            result = $"{newDescription}";
        SetPart("current description", newDescription);
        updated = false;
        return result;
    }

    public bool CanBeDescribed() {

        if (!HasPart("description"))
            return false;

        var type = GetContent("description type");
        if (type.EndsWith("%")) {
            float currentPercent = Mathf.Round((float)GetNumValue() / GetNumValue("max") * 100f);
            int targetPercent = 0;
            bool below = false;
            if (type.StartsWith('<')) {
                type = type.Substring(1);
                below = true;
            }

            if (!int.TryParse(type.TrimEnd('%'), out targetPercent)) {
                Debug.LogError($"could't parse prop description type {type}");
                return false;
            }
            if (below ? currentPercent <= targetPercent : currentPercent >= targetPercent) {
                PropertyDescription.LOG($"[{name}] {currentPercent}/{targetPercent} / value={GetNumValue()} / ({type})", Color.green);
                return true;
            } else {
                PropertyDescription.LOG($"[{name}] {currentPercent}/{targetPercent} / value={GetNumValue()} / ({type})", Color.red);
                return false;
            }
        }
        switch (type) {
            case "always":
                PropertyDescription.LOG($"[{name}] (Always)", Color.green);  
                return true;
            case "on change":
                if (HasPart("changed")) {
                    PropertyDescription.LOG($"[{name}] (Changed)", Color.green);
                    return true;
                }
                PropertyDescription.LOG($"[{name}] (Unchanged)", Color.red);
                return false;
            case "never":
            case "hidden":
                    PropertyDescription.LOG($"[{name}] (Hidden)", Color.green);
                return false;
            default:
                PropertyDescription.LOG($"[{name}] (Default)", Color.red);
                return false;
                if (updated) {
                    PropertyDescription.LOG($"[{name}] (Updated)", Color.green);
                    return false;
                } else {
                    PropertyDescription.LOG($"[{name}] (No Update)", Color.red);
                    return false;
                }
        }


    }

    public string GetDescription() {
        if (!HasPart("description")) {
            //Debug.LogError($"{name} of {_linkedItem.DebugName} doesnt have description");
            return "";
        }

        string description = GetContent("description");

        if (description.Contains("/")) {
            // get description array
            var split = description.Split('/');
            for (int i = 0; i < split.Length; i++)
                split[i] = split[i].Trim(' ');
            
            // get value && max
            int max = HasPart("max") ? GetNumValue("max") : GetNumValue();
            int value = GetNumValue();
            var lerp = (float)value / max;
            if (value == 0) return split[0];
            if ( value == max) return split[split.Length - 1];  
            if (HasPart("strict"))
                return split[(int)(lerp * split.Length)];
            return split[(int)(lerp * (split.Length - 2))];
        }
        return description;
    }

    /// <summary>
    /// Check if 
    /// </summary>
    /// <returns></returns>
    public bool Visible() {

        if (!HasPart("description"))
            return false;

        if (HasPart("condition")) {
            var condition = GetContent("condition");
            var split = condition.Split('/');
            var prop = Player.Instance.GetProp(split[0]);
            if (prop != null && split[1] != prop.GetTextValue())
                return false;
        }
        return true;
    }

    #endregion

    public void Destroy() {
        destroy = true;
    }
}


