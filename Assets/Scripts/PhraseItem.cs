using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPhrase
{
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    public Socket socket;

    public string GetText()
    {
        string itemText = Item.ItemListString(itemGroups, false, false);

        // phrases de vision ( vous voyez, remarquez etc... )
        string visionPhrase = Phrase.GetPhrase("tile_visionPhrases");
        // phrases de location ( se trouve, se tient etc ... )
        string locationVerb = Phrase.GetPhrase("tile_locationPhrases");

        // PHRASE ORDER 

        // le type de la phrase ( noms, verbe de vision et position de l'objet
        // , ou position de l'objet, verbe de location et noms
        // etc... )
        int phraseType = Random.Range(0, 5);

        string text = "";

        Item firstItem = itemGroups[0].item;

        switch (phraseType)
        {
            case 0:
                text = itemText + " " + locationVerb + " " + socket.GetText(firstItem);
                break;
            case 1:
                text = socket.GetText(firstItem) + " " + locationVerb + " " + itemText;
                break;
            case 2:
                text = socket.GetText(firstItem) + ", " + visionPhrase + " " + itemText;

                break;
            case 3:
                text = visionPhrase + " " + socket.GetText(firstItem) + " " + itemText;
                break;
            case 4:
                text = socket.GetText(firstItem) + ", " + itemText;
                break;
            default:
                break;
        }

        // mettre la phrase en majuscule
        return TextManager.WithCaps(text);


    }
}