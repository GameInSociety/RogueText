using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPhrase
{
    public List<ItemSocket> itemSockets = new List<ItemSocket>();

    public string itemPosition = "";

    public string GetText()
    {
        string itemText = Item.ItemListString(itemSockets, false, false);

        // phrases de vision ( vous voyez, remarquez etc... )
        string visionPhrase = PhraseManager.Instance.visionPhrases[Random.Range(0, PhraseManager.Instance.visionPhrases.Length)];
        // phrases de location ( se trouve, se tient etc ... )
        string locationVerbs = PhraseManager.Instance.locationPhrases[Random.Range(0, PhraseManager.Instance.locationPhrases.Length)];

        // PHRASE ORDER 

        // le type de la phrase ( noms, verbe de vision et position de l'objet
        // , ou position de l'objet, verbe de location et noms
        // etc... )
        int phraseType = Random.Range(0, 5);

        string text = "";

        switch (phraseType)
        {
            case 0:
                text = itemText + " " + locationVerbs + " " + itemPosition;
                break;
            case 1:
                text = itemPosition + " " + locationVerbs + " " + itemText;
                break;
            case 2:
                text = itemPosition + ", " + visionPhrase + " " + itemText;
                break;
            case 3:
                text = visionPhrase + " " + itemPosition + " " + itemText;
                break;
            case 4:
                text = itemPosition + ", " + itemText;
                break;
            default:
                break;
        }

        // mettre la phrase en majuscule
        return TextManager.WithCaps(text);


    }
}