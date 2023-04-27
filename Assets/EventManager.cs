using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    private Item currentItem;

    private void Awake()
    {
        instance= this;
    }

    public void CallEvent(Property prop, string _event, Item _item)
    {
        currentItem = _item;
        Property.Event propertyEvent = prop.FindEvent(_event);

        Debug.Log("calling event " + propertyEvent.name + " of " + prop.name);

        foreach (Property.Event.Action _action in propertyEvent._actions)
        {
            Debug.Log("calling action : " + _action.function);

            switch (_action.function)
            {
                case "DestroyItem":
                    Event_DestroyItem(_action.content);
                    break;
                case "CreateItem":
                    Event_AddItem(_action.content);
                    break;
                case "DisableProp":
                    Event_DisableProp(_action.content);
                    break;
                case "EnableProp":
                    Event_EnableProp(_action.content);
                    break;
                case "AddProp":
                    Event_AddProp(_action.content);
                    break;
                case "RemoveProp":
                    Event_RemoveProp(_action.content);
                    break;
                case "ChangeProp":
                    Event_ChangeProp(_action.content);
                    break;
                default:
                    Debug.LogError("PROPERTY EVENT : couldn't find function " + _action.function);
                    break;
            }

        }

        // name/10/subTime/ITEM?
        // check if there's a third part
        // if the third part is an item, transform into item
        // else, add it as property ?
    }

    public void Event_AddItem(string content)
    {
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
        // aussi est-ce que les events peuvent pas être les actions normales de PlayerAction
        // il faudrait refaire le systeme en faisant passer l'objet en paramètre ( pas InputInfo.CurrentItem )
        // de toutes façons tu voulais changer ça parce ue y'a des "aciton" dans toutes les classes et c'est pas ouf

        // if item starts with '*', geting the value of an other property
        // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
        if (content.StartsWith('*'))
        {
            string targetPropertyName = content.Remove(0, 1);
            content = currentItem.GetProperty(targetPropertyName).name;
            Debug.Log("getting " + targetPropertyName + " on " +currentItem.debug_name);
        }

        Item newItem = ItemManager.Instance.CreateInTile(Tile.GetCurrent, content);
        PhraseKey.WritePhrase("&a dog (override item)& is now here", newItem);
    }
    public void Event_DestroyItem(string content)
    {
        Item item = Item.FindInWorld(content);
        Item.Destroy(item);
    }
    public void Event_ChangeProp(string content)
    {
        // encore une preuve qu'il faut que les actions des "events" soient dans les memes que player action
        // et qu'il faut changer player action, parce que c'est plus ouf
        // car la séparation se fait déjà dans input
        string[] parts = content.Split(", ");

        PropertyManager.Instance.Action_ChangeProperty(currentItem, parts[0], parts[1]);
    }
    public void Event_EnableProp(string content)
    {
        PropertyManager.Instance.Action_EnableProperty(currentItem, content);
    }
    public void Event_DisableProp(string content)
    {
        PropertyManager.Instance.Action_DisableProperty(currentItem, content);
    }
    public void Event_AddProp(string content)
    {
        PropertyManager.Instance.Action_AddProperty(currentItem, content);
    }
    public void Event_RemoveProp(string content)
    {
        if (currentItem == null)
        {
            Debug.LogError("no linked item, vas à mazargues");
        }

        PropertyManager.Instance.Action_RemoveProperty(currentItem, content);
    }

}
