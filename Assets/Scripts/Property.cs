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
using static UnityEditor.Progress;

[System.Serializable]
public class Property
{
    public string name;
    public bool enabled = false;
    public string type;
    // might be a list later
    // is a string or a numeric
    public string value;

    //  the max of the potential value, set when  (0=10) 10 = max
    // MAX VALUE SHOULD BE IN THE PARTS
    // battery / 0 / 10
    public int value_max = -1;

    public delegate void OnEmptyValue();
    public OnEmptyValue onEmptyValue;

    /// <summary>
    /// POURQUOI ENABLE LES PROPS AU LIEU DE LES DETRUIRE ET RECREER ?
    /// parce qu'elle contiennent des events, et que ça serait trop chiant de les remettre dans les cells d'action
    /// et aussi parce que c'est mieux, par rapport à la mémoire etc... au garbage collector
    /// </summary>

    /// <summary>
    ///  LES EVENTS EN QUESTION
    ///  ils donnent lieu à discorde dans le studio, mais on a pas trouvé mieux pour l'instant
    /// </summary>
    public List<Event> events;

    // l'objet � laquelle la propri�t� est attach�e,
    // trouver un autre moyen parce que niveau m�moire et serialization c'est pas ouf
    // bien dit, le moyen se serait de regarder si on peut pas gérer certaine chose dans l'objet pour avoir le lien
    // ( et pour ça regarder "growing" de Gardening )
    
    //private Item linkedItem;

   
    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    public Property()
    {
    }
    public Property (Property copy)
    {
        name = copy.name;
        type = copy.type;
        value = copy.value;
        events = copy.events;
    }
    public void Init ()
    {
        if (type.StartsWith('$'))
        {
            type = type.Remove(0, 1);
            enabled = false;
        }
        else
        {
            Enable();
        }

        // the choice between many names
        // tomato?apple?carot
        if (name.Contains('?'))
        {
            string[] parts = name.Split('?');

            if (name.Contains('%'))
            {
                // fucked up , trouver mieux ou mettre dans une autre fonction
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];

                    if (part.Contains("%"))
                    {
                        int percentIndex = part.IndexOf('%');
                        string parseTarget = part.Remove(percentIndex);


                        int percent = int.Parse(parseTarget);

                        float result = Random.value * 100;
                        if (result < percent)
                        {
                            name = part.Remove(0,percentIndex+1);
                            break;
                        }
                    }
                    else
                    {
                        name = part;
                        break;
                    }

                }

            }
            else
            {
                name = parts[Random.Range(0, parts.Length)];
            }

            return;
        }

        // a percent of chance


        if (!string.IsNullOrEmpty(value))
        {
            if (value.Contains('?'))
            {
                string[] strs = value.Split('?');
                SetValue(strs[Random.Range(0, strs.Length)]);
                return;
            }

            if (HasInt())
            {
                value_max = GetInt();
            }
            else if (value.Contains('='))
            {
                int min = int.Parse(value.Split('=')[0]);
                int max = int.Parse(value.Split('=')[1]);
                value_max = max;
                int tmpValue = Random.Range(min, max);
                SetInt(tmpValue);
                return;
            }
        }
    }

    public void SetValue(string str)
    {
        value = str;
    }

    #region description
    public void WriteDescription()
    {
        TextManager.Add(GetDescription());
    }
    public string GetDescription()
    {
        // get prop type
        switch (type)
        {
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
            default:
                break;
        }

        // wtf
        // encore plus wtf
        if (type.ToLower().Contains("type"))
        {
            return "a " + name;
        }

        Debug.Log("default property description");
        return name;

    }
    #endregion

    #region events
    public Event FindEvent(string eventName)
    {
        Event propEvent = events.Find(x => x.name == eventName);
        if ( propEvent == null)
        {
            Debug.LogError("couldn't find event : " + eventName + " on property " + name);
        }

        return propEvent;
    }
    #endregion

    #region update
    public void Update(string line)
    {
        bool add = false;
        bool remove = false;

        if (line.StartsWith('+'))
        {
            line = line.Remove(0, 1);
            add = true;
        }

        if (line.StartsWith('-'))
        {
            line = line.Remove(0, 1);
            remove = true;
        }

        if (line.StartsWith('*'))
        {
            line = line.Remove(0, 1);
            Property pendingProp = FunctionManager.pendingProps.Find(x => x.name == line);
            line = pendingProp.value;

            int valueNeeded = pendingProp.value_max - pendingProp.GetInt();

            int i = pendingProp.GetInt() - valueNeeded;
            pendingProp.SetInt(i);
        }

        int dif = 0;

        if ( int.TryParse(line, out dif))
        {
            int newValue = dif;
            if ( add)
            {
                newValue = GetInt() + dif;
            }
            if ( remove)
            {
                newValue= GetInt() - dif;
            }

            SetInt(newValue);
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            name = line;
        }
        else if ( value != line)
        {
            SetValue(line);
            return;
        }

        

    }
    #endregion

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;

        
    }

    public void RemoveEvents()
    {
        if (events == null)
        {
            return;
        }

        foreach (var e in events)
        {
            WorldEvent.Remove(e.name, FunctionManager.GetCurrentItem(), this, Tile.GetCurrent);
        }
    }

    #region events
    /// THIS CLASS WILL NEVER BE A COPY BECAUSE IT WILL NEVER CHANGE
    
    [System.Serializable]
    public class Event
    {
        public string name;
        public string functionList;
    }
    public void AddEvent(Event propertyEvent)
    {
        if (events == null)
        {
            events = new List<Event>();
        }

        events.Add(propertyEvent);
    }
    #endregion

    #region setters & getters
    public bool HasInt()
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int i = -1;

        return int.TryParse(value, out i);
    }
    public int GetInt()
    {
        int i = -1;

        if (int.TryParse(value, out i))
        {
            return i;
        }

        if (i == -1)
        {
            Debug.LogError("couldn't parse");
        }

        return i;
    }
    public void SetInt(int newValue)
    {
        newValue = Mathf.Clamp(newValue, 0, value_max);

        SetValue(newValue.ToString());

        if ( newValue <= 0)
        {
            if (onEmptyValue != null)
            {
                onEmptyValue();
            }
        }

    }
    #endregion
}

