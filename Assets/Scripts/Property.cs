using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Property
{

    public enum Type
    {
        boolean,
        delay,
        value,
        type,
        var
    }
    public Type type;
    public string name;
    private string content;
    public string param;

    public Item item;

    public Property()
    {

    }
    public Property(string line)
    {
        string[] propertyLine_parts = line.Split('/');

        // type
        Property.Type _type = (Property.Type)System.Enum.Parse(typeof(Property.Type), propertyLine_parts[0]);
        type = _type;

        name = propertyLine_parts[1];

        SetContent(propertyLine_parts[2]);

        param = propertyLine_parts[3];
    }

    // initialisation
    public void Init(Item _item)
    {
        item = _item;

        switch (type)
        {
            case Type.boolean:

                // si la valeur n'est pas assignée ça doit être un pourçentage, donc assigner au hasard
                if (content.Contains("%"))
                {
                    string percentSTR = content.Remove(content.Length - 1);

                    int percent = int.Parse(percentSTR);

                    content = Random.value * 100 < percent ? "true" : "false";
                }

                if (content != "true" && content != "false")
                {
                    Debug.LogError("content for boolean invalid : " + content + " l(" + content.Length + ")");
                }

                break;
            case Type.delay:

                // si la propriété est un delai, s'abonner au temps
                TimeManager.GetInstance().onNextHour += HandleOnNextHour;

                break;
            case Type.var:

                if ( name == "type")
                {
                    if( content == "?")
                    {
                        content = Item.GetItemOfType(param).debug_name;
                    }
                }
                break;
            default:
                break;
        }
    }

    // setters
    public void SetContent(string _content)
    {
        content = _content;
    }

    public void SetValue(int _value)
    {
        content = _value.ToString();
    }

    public int GetValue()
    {
        int i = 0;

        // ça n'a aucun sens, ça marche pas avec les nombre de plus de 9 trou du cul de merde
        int maxIndex = content.IndexOf('m');
        string value_str;
        if (maxIndex > 0)
        {
            value_str = content.Remove(maxIndex);
        }
        else
        {
            value_str = content;
        }

        if (int.TryParse(value_str, out i))
        {
            return i;
        }

        if ( value_str == "?")
        {
            int a = Random.Range(0, GetMax());
            content = content.Replace("?", a.ToString());
            return a;
        }

        Debug.LogError("couldn't parse : " + value_str + " in property : " + name);
        return i;
    }

    #region max
    public bool HasMax()
    {
        return content.Length == 3 && content[1] == 'm';
    }
    public int GetMax()
    {
        if (!HasMax())
        {
            return 100;
        }

        return int.Parse(content[2].ToString());
    }
    #endregion

    public string GetContent()
    {
        return content;
    }

    public void Write()
    {
        switch (type)
        {
            case Type.boolean:
                Phrase.Write("&le chien (main item)& est " + GetText());
                break;
            case Type.delay:

                Item paramItem = Item.TryGetItem(param);
                if (paramItem == null)
                {
                    string text = "Il reste " + GetText() + " heures avant que &le chien (main item)& devienne " + param;
                    Phrase.Write(text);
                }
                else
                {
                    Phrase.SetOverrideItem(paramItem);
                    string text = "Il reste " + GetText() + " avant que &le chien (main item)& devienne &un chien (override item)&";
                    Phrase.Write(text);
                }
                break;
            case Type.value:

                string[] phrases = new string[7]
                {
                    "vide",
                    "presque vide",
                    "commence à se vider",
                    "à moitié rempli",
                    "commence à se remplir",
                    "presque plein",
                    "plein",
                };

                float lerp = (float)GetValue() / GetMax();
                int index = (int)(lerp * phrases.Length);
                string phrase = phrases[index];
                
                Phrase.Write("&le chien (main item)& est " + phrase);

                break;
            case Type.type:
                Phrase.Write("c'est &un chien (main item)& de type " + content);
                break;
            case Type.var:
                Item item = Item.GetDataItem(content);
                Phrase.SetOverrideItem(item);
                Phrase.Write("c'est &un chien (override item)&");
                break;
            default:
                Phrase.Write("<color=red>no type error</color>");
                break;
        }
    }


    public string GetDebugText()
    {
        return "property of : " + item.word.text + " / " + name + " : " + content;
    }

    public string GetText()
    {
        switch (type)
        {
            case Type.boolean:

                string[] parts = param.Split('?');
                return content == "true" ? parts[0] : parts[1];

            case Type.delay:

                return content + " heures";

            case Type.value:

                return content;

            case Type.var:

                return content;

            default:
                return "<color=red>no type error</color>";
        }

    }

    /// <summary>
    /// ça a pas grand chose à foutre là quand on y pense
    /// </summary>
    public void HandleOnNextHour()
    {
        // en soit à terme, il faudrait que la fonction soit dans la case elle même
        // delay/grow/10/BecomeItem(carotte)
        // pas si compliqué que ça quand on y pense

        // decrease time
        int timeLeft = GetValue();
        --timeLeft;
        SetContent(timeLeft.ToString());

        // don't do anything if the time left is above 0
        if (timeLeft > 0)
        {
            return;
        }
        TimeManager.GetInstance().onNextHour -= HandleOnNextHour;


        // ici devraient être les actions
        switch (name)
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
        }
    }

    #region actions
    public static void ChangeProperty()
    {
        ChangeProperty(PlayerAction.GetCurrent.GetContent(0), PlayerAction.GetCurrent.GetContent(1));
    }
    public static void ChangeProperty(string propName, string newContent)
    {
        Item targetItem = InputInfo.GetCurrent.MainItem;

        if (!targetItem.HasProperty(propName))
        {
            Debug.LogError(targetItem.debug_name + " doesn't have the property " + newContent);
            return;
        }

        Property property = targetItem.GetProperty(propName);

        string currentContent = targetItem.GetProperty(propName).GetContent();

        if (currentContent == newContent)
        {
            Phrase.Write("&le chien (main item)& est déjà " + targetItem.GetProperty(propName).GetText());
            return;
        }

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
                Phrase.Write("&le chien (main item)& est plein");
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
