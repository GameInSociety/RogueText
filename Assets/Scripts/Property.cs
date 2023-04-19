using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

[System.Serializable]
public class Property
{
    // les parties
    // et si, par soucis de sauvegarde, de serialisation, etc les parts �taient tout le temps un string
    // qu'on split � chaque fois qu'on y accede 
    private string[] _parts;

    // l'objet � laquelle la propri�t� est attach�e,
    // trouver un autre moyen parce que niveau m�moire et serialization c'est pas ouf
    // bien dit, le moyen se serait de regarder si on peut pas gérer certaine chose dans l'objet pour avoir le lien
    // ( et pour ça regarder "growing" de Gardening )
    private Item linkedItem;

    //  the max of the potential value, set when  (0=10) 10 = max
    public int value_max;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    public Property()
    {

    }
    public Property(string line, Item item)
    {
        UpdateParts(_parts);
        linkedItem = item;
        _parts = line.Split('/');
    }
    ///

    /// GET THE PARTS
    public string GetLine(){
        string line = "";
        for (int i = 0; i < _parts.Length; ++i){

            line += _parts[i];

            if (i < _parts.Length-1){
                line += "/";
            }
        }

        return line;
    }
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

    #region description
    public string GetDescription()
    {
        if ( _parts.Length == 1)
        {
            return "it's a " + GetPart(0);
        }

        foreach (var part in _parts)
        {
            if ( part == "subTime")
            {
                if ( GetValue() == 0){
                    return "it's " + GetPart(0);
                }

                string[] phrases = new string[5]
                {
                    "empty",
                    "almost empty",
                    Random.value < 0.5f ? "half full" : "half empty",
                    "almost full",
                    "full",
                };

                float lerp = (float)GetValue() / value_max;
                int index = (int)(lerp * phrases.Length);
                string text = phrases[index];
                return "it's " + text;
                //return "only " + GetValue() + GetPart(0) + " left";
            }
        }

        Debug.Log("default property description");
        return GetPart(0);

    }
    public void Write()
    {
        PhraseKey.WriteHard(GetDescription());
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
        // decrease time
        int timeLeft = GetValue();
        --timeLeft;
        SetValue(timeLeft);

        // don't do anything if the time left is above 0
        if (timeLeft > 0)
        {
            return;
        }

        // name/10/subTime/ITEM?
        // check if there's a third part
        // if the third part is an item, transform into item
        // else, add it as property ?

        // consequences for each part after "subTime"
        for(int i = 3; i < _parts.Length; ++i){

            // get the text of the part
            string part = GetPart(i);
            
            // check if it's an item
            Item tmpItem = Item.FindByName(part);
            if (tmpItem != null){
                // transform the property item into a new item ( pas très souple mais à voir ça peut faire le taf)
                Item newItem = Item.CreateNew(tmpItem);
                Debug.Log("found item " + part);
                Item.Destroy(linkedItem);

                /// IMPORTANT //
                // actuelement l'objet est ajouté dans la tile actuelle parce qu'il n'y a pas de lien vers la tle dans la property
                // POUR résoudre ça, il faut check la différence de temps quand le joueur arrive dans une tile
                // EXEMPLE :
                // dry/5/subTime/gardening#0#unsubTime
                // growing/10/subTime/carrot
                // quand on arrive dans la tile, on check toutes les heures passées depuis la sub
                // on loop tout ça et le monticule sera dry avant de pouvoir grow ( t'as compris ?)
                // comme ça, beaucoup moins de suscription, mais juste une heure à retenir par rappor au début
                // c'est bien, en tout cas ça parait bien

                Tile.GetCurrent.AddItem(newItem);
                PhraseKey.WritePhrase("gardening_grew", newItem);
            }
            else{
                // check if changing or adding property
                string line = part.Replace('#', '/');
                string[] tmpParts = line.Split('/');;
                
                if ( linkedItem.HasProperty(tmpParts[0])){
                    Property tmpProperty = linkedItem.GetProperty(tmpParts[0]);
                    tmpProperty.UpdateParts(tmpParts);
                }
                else{
                    linkedItem.AddProperty(line);
                }
                
            }
        }

        

        UnsubscribeToTime();


        // ici devraient �tre les actions
        // maintenant c'est juste un �tat
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
    public void SubscribeToTime()
    {
        TimeManager.GetInstance().onNextHour += HandleOnNextHour;
        
    }
    public void UnsubscribeToTime()
    {
        TimeManager.GetInstance().onNextHour -= HandleOnNextHour;
    }

    public void UpdateParts(string[] tmpParts){
        foreach (string part in tmpParts)
        {
            int value = 0;
            if ( int.TryParse(part, out value ) ){
                int newValue = GetValue() + value;
                newValue = Mathf.Clamp(newValue, 0, value_max);
                SetValue(newValue);
                continue;
            }

            if ( part.Contains("subTime")){
                SubscribeToTime();
                continue;
            }

            if (part.Contains("unsubTime")){
                UnsubscribeToTime();
                continue;
            }
            
        }
    }
    #endregion
}
