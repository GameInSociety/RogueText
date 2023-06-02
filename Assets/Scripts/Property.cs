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

    public string eventContent;

    public bool changed = false;

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
    public List<Event> eventDatas;

   
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
        eventDatas = copy.eventDatas;
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
    public string GetDescription()
    {
        // get prop type
        switch (type)
        {
            case "type":
                if (string.IsNullOrEmpty(value))
                {
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
        Event propEvent = eventDatas.Find(x => x.name == eventName);
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
            Property pendingProp = WorldEvent.current.pendingProps.Find(x => x.name == line);
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

    #region events
    /// THIS CLASS WILL NEVER BE A COPY BECAUSE IT WILL NEVER CHANGE
    
    [System.Serializable]
    public class Event
    {
        public string name;
        public string functionListContent;
        public List<WorldEvent> functions;
    }
    public void AddEvent(Event propertyEvent)
    {
        if (eventDatas == null)
        {
            eventDatas = new List<Event>();
        }

        eventDatas.Add(propertyEvent);
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


    public static List<Property> propertiesToDescribe= new List<Property>();
    public static void DescribeProperties()
    {
        if (!propertiesToDescribe.Any())
        {
            return;
        }
        TextManager.Write("It's ");
        int index = 0;
        for (int i = 0; i < propertiesToDescribe.Count; i++)
        {
            TextManager.Add(propertiesToDescribe[i].GetDescription());
            string link = TextUtils.GetLink(index, propertiesToDescribe.Count);
            TextManager.Add(link);
            index++;
        }

        propertiesToDescribe.Clear();

    }


}

