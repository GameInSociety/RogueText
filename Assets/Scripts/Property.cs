using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class Property {
    public string name;
    public bool enabled = false;
    public string type;
    // might be a list later
    // is a string or a numeric
    public string value;

    public string eventContent;

    //  the max of the potential value, set when  (0=10) 10 = max
    // MAX VALUE SHOULD BE IN THE PARTS
    // battery / 0 / 10
    public int value_max = -1;

    public bool alwaysDescribe = false;
    public string[] descriptions;
    public bool changed = false;

    /// <summary>
    /// POURQUOI ENABLE LES PROPS AU LIEU DE LES DETRUIRE ET RECREER ?
    /// parce qu'elle contiennent des events, et que ça serait trop chiant de les remettre dans les cells d'action
    /// et aussi parce que c'est mieux, par rapport à la mémoire etc... au garbage collector
    /// </summary>

    /// <summary>
    ///  LES EVENTS EN QUESTION
    ///  ils donnent lieu à discorde dans le studio, mais on a pas trouvé mieux pour l'instant
    /// </summary>
    public List<EventData> eventDatas;


    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    public Property() {
    }
    public Property(Property copy) {


        name = copy.name;
        type = copy.type;
        value = copy.value;
        eventDatas = copy.eventDatas;
        descriptions = copy.descriptions;
        alwaysDescribe = copy.alwaysDescribe;
    }
    public void Init() {

        if (type.StartsWith('$')) {
            type = type.Remove(0, 1);
            enabled = false;
        } else {
            enabled = true;
        }

        // the choice between many names
        // tomato?apple?carot
        if (name.Contains('?')) {
            var parts = name.Split('?');

            if (name.Contains('%')) {
                // fucked up , trouver mieux ou mettre dans une autre fonction
                for (var i = 0; i < parts.Length; i++) {
                    var part = parts[i];

                    if (part.Contains("%")) {
                        var percentIndex = part.IndexOf('%');
                        var parseTarget = part.Remove(percentIndex);


                        var percent = int.Parse(parseTarget);

                        var result = Random.value * 100;
                        if (result < percent) {
                            name = part.Remove(0, percentIndex + 1);
                            break;
                        }
                    } else {
                        name = part;
                        break;
                    }

                }

            } else {
                name = parts[Random.Range(0, parts.Length)];
            }

            return;
        }

        // a percent of chance


        if (!string.IsNullOrEmpty(value)) {
            if (value.Contains('?')) {
                var strs = value.Split('?');
                SetValue(strs[Random.Range(0, strs.Length)]);
                return;
            }

            if (HasInt()) {
                value_max = GetInt();
            } else if (value.Contains('=')) {
                var min = int.Parse(value.Split('=')[0]);
                var max = int.Parse(value.Split('=')[1]);
                value_max = max;
                var tmpValue = Random.Range(min, max);
                SetInt(tmpValue);
                return;
            }
        }
    }

    public void SetValue(string str) {
        value = str;
    }

    #region description
    public string GetDescription() {
        if (descriptions != null) {
            var i = GetInt();
            var lerp = i / (float)value_max;
            var index = (int)(lerp * descriptions.Length);
            index = descriptions.Length - index;
            index = (int)Mathf.Clamp(index, 0f, descriptions.Length - 1);
            if (index >= descriptions.Length || index < 0) {
                Debug.LogError("PROPERTY : " + name + " (" + index + ") " + " range (" + descriptions.Length + ")");
                foreach (var item in descriptions) {
                    Debug.Log(item);
                }
                return "/ " + name + " / error in description";
            }
            return descriptions[index] + " (" + i + ")";
            return descriptions[index] + " (" + i + "/" + value_max + ")";
        }

        // get prop type
        switch (type) {
            case "type":
                if (string.IsNullOrEmpty(value)) {
                    return "a " + name;
                }
                return "a " + value;
            case "state":
                return name;
            case "iValue": // invisible value, not added in decription
                return name;
            case "value":
                return "has " + value;
            case "container":
                return TextUtils.GetPropertyContainerText(this);
            case "dir":
                return "goes " + value;
            case "equip":
                return "something you can wear";
            default:
                break;
        }

        // wtf
        // encore plus wtf
        if (type.ToLower().Contains("type")) {
            return "a " + name;
        }

        return name;

    }
    #endregion

    #region events
    public bool HasEvent(string eventName) {
        return eventDatas != null && eventDatas.Find(x => x.eventName == eventName) != null;
    }
    public EventData GetEvent(string eventName) {
        return eventDatas.Find(x => x.eventName == eventName);
    }
    #endregion
    public bool destroy = false;
    public void Destroy() {
        destroy = true;
    }

    #region update
    public void Update(string line) {
        //Describe();

        var add = false;
        var remove = false;

        if (line.StartsWith('+')) {
            line = line.Remove(0, 1);
            add = true;
        }

        if (line.StartsWith('-')) {
            line = line.Remove(0, 1);
            remove = true;
        }

        if (line.StartsWith('*')) {
            line = line.Remove(0, 1);
            line = line.Remove(line.IndexOf('*'));

            var prop = ItemParser.GetCurrent.SearchPropertyOfItemInInput(line);

            if (prop == null) {
                TextManager.write("Nothing here has " + line);
                FunctionSequence.current.Stop();
                return;
            }

            line = prop.value;
        }

        int dif;
        if (int.TryParse(line, out dif)) {
            var newValue = dif;
            if (add) {
                newValue = GetInt() + dif;
            }
            if (remove) {
                newValue = GetInt() - dif;
            }

            SetInt(newValue);
            return;
        }

        if (string.IsNullOrEmpty(value)) {
            name = line;
        } else if (value != line) {
            SetValue(line);
            return;
        }

    }
    #endregion

    #region events
    /// THIS CLASS WILL NEVER BE A COPY BECAUSE IT WILL NEVER CHANGE

    public class EventData {
        public string eventName;
        public string cellContent;
    }
    public void AddEventData(EventData propertyEvent) {
        if (eventDatas == null) {
            eventDatas = new List<EventData>();
        }

        eventDatas.Add(propertyEvent);
    }
    #endregion

    #region setters & getters
    public bool HasInt() {
        if (string.IsNullOrEmpty(value)) {
            return false;
        }
        /* Modification non fusionnée à partir du projet 'Assembly-CSharp.Player'
        Avant :
                int i;
                return int.TryParse(value, out i);
        Après :
                return int.TryParse(value, out i);
        */

        return int.TryParse(value, out _);
    }
    public int GetInt() {
        int i;
        if (int.TryParse(value, out i)) {
            return i;
        }

        if (i == -1) {
            Debug.LogError("couldn't parse");
        }

        return i;
    }
    public void SetInt(int newValue) {
        newValue = Mathf.Clamp(newValue, 0, value_max);
        SetValue(newValue.ToString());

        if (newValue <= 0) {
            ItemEvent.callEventOnProp("subEmpty", this);
        }

    }
    #endregion





}

