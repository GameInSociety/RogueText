using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking.Match;
using static UnityEditor.Progress;

[System.Serializable]
public class Property
{
    public string name;

    /// <summary>
    /// POURQUOI ENABLE LES PROPS AU LIEU DE LES DETRUIRE ET RECREER ?
    /// parce qu'elle contiennent des events, et que ça serait trop chiant de les remettre dans les cells d'action
    /// et aussi parce que c'est mieux, par rapport à la mémoire etc... au garbage collector
    /// </summary>
    public List<string> parts = new List<string>();
    public bool enabled = false;

    /// <summary>
    ///  LES EVENTS EN QUESTION
    ///  ils donnent lieu à discorde dans le studio, mais on a pas trouvé mieux pour l'instant
    /// </summary>
    public List<Event> events = new List<Event>();

    // l'objet � laquelle la propri�t� est attach�e,
    // trouver un autre moyen parce que niveau m�moire et serialization c'est pas ouf
    // bien dit, le moyen se serait de regarder si on peut pas gérer certaine chose dans l'objet pour avoir le lien
    // ( et pour ça regarder "growing" de Gardening )
    
    //private Item linkedItem;

    //  the max of the potential value, set when  (0=10) 10 = max
    // MAX VALUE SHOULD BE IN THE PARTS
    // battery / 0 / 10
    public int value_max = -1;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    public Property()
    {
    }
    public Property (Property copy)
    {
        name = copy.name;
        events = copy.events;
        enabled = copy.enabled;

        foreach (var part in copy.parts)
        {
            parts.Add(part);
        }

    }
    ///

    #region description
    public string GetDescription()
    {
        if (!enabled)
        {
            return "";
        }

        // get type
        if ( parts.Count == 1)
        {
            return GetPart(0);
        }

        switch (GetPart(0))
        {
            case "state":
                return GetPart(1);
            case "iValue": // invisible value
                return GetPart(1);
            default:
                break;
        }

        // wtf
        // encore plus wtf
        if (GetPart(0).ToLower().Contains("type"))
        {
            return "a " + GetPart(0);
        }

        if (HasValue())
        {
            if (value_max >= 0)
            {
                string[] phrases = new string[3]
            {
                    "almost empty",
                    Random.value < 0.5f ? "half full" : "half empty",
                    "almost full",
            };

                int value = GetValue();

                if (value == 0)
                {
                    return "empty of " + GetPart(1);
                }

                if (value == value_max)
                {
                    return "full of " + GetPart(1);
                }

                float lerp = (float)value / value_max;

                int index = (int)(lerp * phrases.Length);
                index = Mathf.Clamp(index, 0, phrases.Length - 1);
                string text = phrases[index];
                return "" + text + " of " + GetPart(1);
            }
        }

        Debug.Log("default property description");
        return GetPart(0);

    }
    #endregion

    #region time handle

    /// <summary>
    /// �a a pas grand chose � foutre l� quand on y pense
    /// effectivement, on peut mettre cette fonction dans l'Item en lui meme
    /// pour ne pas avoir le lien avec l'item DANS la property, déjà
    /// et aussi, lire plus loin dans la fonctin handonnexthour, mais faire ce truc de dif du temps
    /// </summary>
    public void HandleOnNextHour()
    {
        
    }
    public bool ContainsEvent(string eventName)
    {
        return events.Find(x => x.name == eventName) != null;
    }
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
    public void UpdateParts()
    {
        UpdateParts(parts);
    }
    public void UpdateParts(List<string> changingParts)
    {

        for (int i = 0; i < changingParts.Count; ++i)
        {
            string changinPart = changingParts[i];

            if (changinPart.Contains('?'))
            {
                string[] strs = changinPart.Split('?');
                parts[i] = strs[Random.Range(0, strs.Length)];

                Debug.Log(parts[i]);
                continue;
            }

            // check for max value if none is set
            if ( value_max < 0)
            {
                Debug.Log("truing max");
                int tmpMax = -1;
                if (int.TryParse(changingParts[i], out tmpMax))
                {
                    value_max = tmpMax;
                    Debug.Log("max of " + GetPart(0) + " " + value_max);
                }
            }

            if (changinPart.Contains('='))
            {
                int min = int.Parse(changinPart.Split('=')[0]);
                int max = int.Parse(changinPart.Split('=')[1]);
                value_max = max;
                parts[i] = Random.Range(min, max).ToString();
                continue;
            }

            if (changinPart.StartsWith('+'))
            {
                int dif = int.Parse(changinPart.Remove(0, 1));
                int newValue = GetValue() + dif;
                newValue = Mathf.Clamp(newValue, 0, value_max);
                parts[i] = newValue.ToString();
                continue;
            }

            parts[i] = changinPart;

        }
    }
    #endregion

    public void Enable()
    {
        enabled = true;
        UpdateParts();
    }

    public void Disable()
    {
        enabled = false;
    }

    /// <summary>
    /// il faut donc à terme que les events soient appelé depuis la class "objet"
    /// comme ça, l'abonnement à "OnNextHour" se fait que dans l'objet
    /// et aussi pour ne pas faire passer "linkedItem" en pointeur
    /// ce qui est horrible pour la documentation
    /// et pas dans toutes les props
    /// gros chantier mais à faire à un moment donné
    /// </summary>
    /// <param name="str"></param>

    #region events
    /// THIS CLASS WILL NEVER BE A COPY BECAUSE IT WILL NEVER CHANGE
    [System.Serializable]
    public class Event
    {
        public string name;
        public List<Action> _actions;

        public void AddAction(Action action)
        {
            if ( _actions == null)
            {
                _actions = new List<Action>();
            }

            _actions.Add(action);
        }

        [System.Serializable]
        public class Action
        {
            public string function;
            public string content;
        }
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
    public string GetPart(int i)
    {
        if (i >= parts.Count)
        {
            if (parts.Count == 0)
            {
                Debug.LogError("error property : parts length 0");
                return "error property parts";
            }
            Debug.LogError("error property : try get part " + i + " but out of range");

            return parts[0];
        }

        return parts[i];
    }

    public void SetPart(int i, string str)
    {
        parts[i] = str;
    }

    public bool HasPart(int i)
    {
        if (i < parts.Count)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool HasValue()
    {
        int i = -1;

        foreach (var part in parts)
        {
            if (int.TryParse(part, out i))
            {
                return true;
            }
        }

        return false;
    }
    public int GetValue()
    {
        int i = -1;

        foreach (var part in parts)
        {
            if (int.TryParse(part, out i))
            {
                return i;
            }
        }

        if (i == -1)
        {
            Debug.LogError("couldn't parse");
        }

        return i;
    }
    public void SetValue(int newValue)
    {
        int tmpValue = -1;

        for (int i = 0; i < parts.Count; i++)
        {
            if (int.TryParse(parts[i], out tmpValue))
            {
                parts[i] = parts[i].Replace(parts[i], newValue.ToString());
                return;
            }
        }

        Debug.LogError("couldn't change value because no value parsed");
    }
    #endregion
}

