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
            enabled = true;
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
                value = strs[Random.Range(0, strs.Length)];
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

    #region description
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
    public void Update(string line)
    {
        bool add = false;

        if (line.StartsWith('+'))
        {
            line = line.Remove(0, 1);
            add = true;
        }

        if (line.StartsWith('*'))
        {
            line = line.Remove(0, 1);
            Property pendingProp = PropertyManager.Instance.pendingProps.Find(x => x.name == line);
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

            SetInt(newValue);
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            name = line;
        }
        else if ( value != line)
        {
            value = line;
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
        public bool subbed = false;
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

        value = newValue.ToString();

        if ( newValue <= 0)
        {

            if (PropertyManager.Instance.onEmptyValue != null)
            {
                PropertyManager.Instance.onEmptyValue();
            }
        }

    }
    #endregion
}

