using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

[System.Serializable]
public class Property
{
    // les parties
    // et si, par soucis de sauvegarde, de serialisation, etc les parts étaient tout le temps un string
    // qu'on split à chaque fois qu'on y accede 
    private string[] _parts;

    // l'objet à laquelle la propriété est attachée,
    // trouver un autre moyen parce que niveau mémoire et serialization c'est pas ouf
    private Item linkedItem;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    public Property()
    {

    }
    public Property(string line)
    {
        _parts = line.Split('/');
    }
    ///

    /// GET THE PARTS
    public string GetPart(int i)
    {
        if ( i >= _parts.Length)
        {
            if ( _parts.Length == 0)
            {
                Debug.LogError("error property : parts length 0");
                return "error property parts";
            }
            Debug.LogError("error property : try get part " + i + " but out of range");

            return _parts[0];
        }

        return _parts[i];
    }

    public void SetPart(int i, string str)
    {
        _parts[i] = str;
    }

    public bool HasPart(int i)
    {
        if ( i < _parts.Length)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // initialisation
    public void Init(Item _item)
    {
        linkedItem = _item;

        // if contains a number, its a content ( ex : water/5 )
        // if contains "=" (0=10) its a range

        // if contains "delay", it's a delay
        // consider puttin days, hours, minutes etc...

        // if contains ?, show between X values ( opened?closed?half opened )
        // if contains %, only a chance of having the property ( ex : dirty/50% )

        // EXEMPLE CONTEXTUEL
        // flashlight
        // battery/0=5
        // TURN OFF
        // off : stop delay
        // on : start delay


        /// NEW
        for (int i = 0; i < _parts.Length; i++)
        {
            string part = _parts[i];

            // value range
            // fetch range ("=")
            // gives random number between X=X
            if (part.Contains('='))
            {
                int min = int.Parse(part.Split('=')[0]);
                int max = int.Parse(part.Split('=')[1]);
                _parts[i] = Random.Range(min, max).ToString();
            }

            // delay
            // the valueSSS goes down every hour
            if (part.Contains("delay"))
            {
                TimeManager.GetInstance().onNextHour += HandleOnNextHour;
            }

            // random state
            // ex : opened?closed?haldopened
            if(part.Contains("?"))
            {
                string[] strs = part.Split('?');
                _parts[i] = strs[Random.Range(0, strs.Length)];
            }
        }
    }

    public int GetValue()
    {
        int i = -1;

        foreach (var part in _parts)
        {
            int.TryParse(part, out i);
        }

        if ( i == -1)
        {
            Debug.LogError("couldn't parse");
        }

        return i;
    }
    public void SetValue(int newValue)
    {
        int tmpValue = -1;

        for (int i = 0; i < _parts.Length; i++)
        {
            if (int.TryParse(_parts[i], out tmpValue))
            {
                _parts[i] = newValue.ToString();
                return;
            }
        }

        Debug.LogError("couldn't change value because no value parsed");
    }

    public string GetDescription()
    {
        if ( _parts.Length == 1)
        {
            return "it's a " + GetPart(0);
        }

        foreach (var part in _parts)
        {
            if ( part == "delay")
            {
                /*string[] phrases = new string[5]
                {
                    "empty",
                    "almost empty",
                    Random.value < 0.5f ? "half full" : "half empty",
                    "almost full",
                    "full",
                };

                float lerp = (float)GetValue() / GetMax();
                int index = (int)(lerp * phrases.Length);
                string text = phrases[index];*/

                return "only " + GetValue() + GetPart(0) + " left";
            }
        }

        Debug.Log("default property description");
        return GetPart(0);

    }
    public void Write()
    {
        Debug.Log("debug stuff");
        //PhraseKey.Write("item_description");
    }

    /// <summary>
    /// ça a pas grand chose à foutre là quand on y pense
    /// </summary>
    public void HandleOnNextHour()
    {
        // decrease time
        int timeLeft = GetValue();
        --timeLeft;
        SetValue(timeLeft);

        // don't do anything if the time left is above 0
        if (timeLeft > 0)
        {
            return;
        }

        TimeManager.GetInstance().onNextHour -= HandleOnNextHour;


        // ici devraient être les actions
        // maintenant c'est juste un état
        // battery = 0 ? s'allume pas
        // dry = ?
        // grow = ?
        /*switch (name)
        {
            case "dry":
                Gardening.Dry(this);
                break;
            case "grow":
                Gardening.Grow(this);
                break;
            default:
                //Debug.LogError("timer property : " + name + " is dead end");
                break;
        }*/
    }

    #region actions
    public static void ChangeProperty()
    {
        ChangeProperty(PlayerAction.GetCurrent.GetContent(0));
    }
    public static void ChangeProperty(string _property)
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        if (!targetItem.HasProperty(_property))
        {
            // peut être la rajouter ? si elle n'existe pas, pour être sur ça peut être interessant
            Debug.LogError(targetItem.debug_name + " doesn't have the property " + _property);
            return;
        }

        Property property = targetItem.GetProperty(_property);

        // check if value is +1 or -5
        if (newContent.Contains("+") || newContent.Contains("-"))
        {
            int currentValue = targetItem.GetProperty(propName).GetValue();

            string sign_str = newContent[0].ToString();
            string amount_str = newContent[1].ToString();

            int amount = int.Parse(amount_str);
            int result = 0;

            switch (sign_str)
            {
                case "+":
                    result = currentValue + amount;
                    break;

                case "-":
                    result = currentValue - amount;
                    break;

                default:
                    result = 1;
                    break;
            }

            if (currentValue>= property.GetMax()||result >= property.GetMax())
            {
                result = Mathf.Clamp(result, 0, property.GetMax());
                PhraseKey.Write("&le chien (main item)& est plein");
            }

            // change target content
            // un peu chiant de devoir remettre le "m" à chaque fois mais en plus ça marche peut être pas
            newContent = result.ToString() + "m" + property.GetMax();
        }

        targetItem.GetProperty(propName).SetContent(newContent);

        targetItem.GetProperty(propName).Write();
    }

    public static void AddProperty()
    {
        switch (PlayerAction.GetCurrent.GetContentCount())
        {
            case 1:
                AddProperty( PlayerAction.GetCurrent.GetContent(0) );
                break;
            case 2:
                Item item = Item.FindInWorld(PlayerAction.GetCurrent.GetContent(1));
                if (item != null)
                {
                    Debug.Log("found item : " + item.debug_name);
                }
                else
                {
                    Debug.LogError("couldn't find item : " + PlayerAction.GetCurrent.GetContent(1));
                }
                AddProperty( item,  PlayerAction.GetCurrent.GetContent(0) );
                break;
            default:
                break;
        }
    }

    public static void AddProperty(string str)
    {
        AddProperty(InputInfo.GetCurrent.MainItem, str);
    }
    public static void AddProperty(Item item, string str)
    {
        Property property;
        // désolé, gabriel du futur, c'est vraiment dégueulasse ce que tu fais et ça va poser des problemes
        if (str == "Main Item Prop")
        {
            property = InputInfo.GetCurrent.MainItem.properties[0];
            Debug.Log("adding the same property");
        }
        else
        {
            property = new Property(str);
        }

        item.AddProperty(property);
    }
    #endregion
}
