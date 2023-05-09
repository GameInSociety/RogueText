using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    private Item currentItem;

    public List<PropertyEventGroup> propertyGroups = new List<PropertyEventGroup>();

    [System.Serializable]
    public class PropertyEventGroup
    {
        public Item item;
        public Property property;

        
    }

    public List<PropertyEventGroup> FindPropertyEventGroup(string eventName)
    {
        return propertyGroups.FindAll(x => x.property.ContainsEvent(eventName) && x.property.enabled);
    }

    public void AddPropertyEvent (Property property, Item item)
    {
        PropertyEventGroup newGroup = new PropertyEventGroup();
        newGroup.item = item;
        newGroup.property = property;

        propertyGroups.Add(newGroup);
    }


    private void Awake()
    {
        instance= this;
    }

    private void Start()
    {
        TimeManager.GetInstance().onNextHour += HandleOnNextHour;
        TimeManager.GetInstance().onRaining += HandleOnRaining;
        PropertyManager.Instance.onEmptyValue += HandleOnEmptyValue;
    }

    #region events
    
    public void HandleOnNextHour()
    {
        foreach (var propertyGroup in FindPropertyEventGroup("subHours"))
        {
            Property property = propertyGroup.property;

            int timeLeft = property.GetInt();
            --timeLeft;
            property.SetInt(timeLeft);

            // don't do anything if the time left is above 0
            if (timeLeft > 0)
            {
                continue;
            }

            CallEvent(property, "subHours", propertyGroup.item);

            property.Disable();
        }
    }
    public void HandleOnRaining()
    {
        Debug.Log("raining");

        foreach (var propertyGroup in FindPropertyEventGroup("subRain"))
        {
            Property property = propertyGroup.property;
            CallEvent(property, "subRain", propertyGroup.item);
        }
    }
    public void HandleOnEmptyValue()
    {

        /// il y a un bug parce que ce event est appel� de n'importe ou et �a brise le flow des trucs
        Debug.Log("on empty value");

        foreach (var propertyGroup in FindPropertyEventGroup("subEmpty"))
        {

            Property property = propertyGroup.property;

            Debug.Log("property " + property.name + " of " + propertyGroup.item.debug_name + " is " + property.GetInt());


            if ( property.GetInt() == 0)
            {
                CallEvent(property, "subEmpty", propertyGroup.item);
            }
        }
    }
    #endregion


    public void CallEvent(Property prop, string _event, Item _item)
    {
        currentItem = _item;
        Property.Event propertyEvent = prop.FindEvent(_event);

        Debug.Log("calling event " + propertyEvent.name + " of " + prop.name);

        foreach (Property.Event.Action _action in propertyEvent._actions)
        {
            // this should be the exact same os player actions
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
        // actuelement l'objet est ajout� dans la tile actuelle parce qu'il n'y a pas de lien vers la tle dans la property
        // POUR r�soudre �a, il faut check la diff�rence de temps quand le joueur arrive dans une tile
        // EXEMPLE :
        // dry/5/subTime/gardening#0#unsubTime
        // growing/10/subTime/carrot
        // quand on arrive dans la tile, on check toutes les heures pass�es depuis la sub
        // on loop tout �a et le monticule sera dry avant de pouvoir grow ( t'as compris ?)
        // comme �a, beaucoup moins de suscription, mais juste une heure � retenir par rappor au d�but
        // c'est bien, en tout cas �a parait bien
        // aussi est-ce que les events peuvent pas �tre les actions normales de PlayerAction
        // il faudrait refaire le systeme en faisant passer l'objet en param�tre ( pas InputInfo.CurrentItem )
        // de toutes fa�ons tu voulais changer �a parce ue y'a des "aciton" dans toutes les classes et c'est pas ouf

        // if item starts with '*', geting the value of an other property
        // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
        if (content.StartsWith('*'))
        {
            string targetPropertyName = content.Remove(0, 1);
            content = currentItem.GetProperty(targetPropertyName).name;
            Debug.Log("getting " + targetPropertyName + " on " +currentItem.debug_name);
        }

        Item newItem = ItemManager.Instance.CreateInTile(Tile.GetCurrent, content);
        TextManager.WritePhrase("&a dog (override)& is now here", newItem);
    }
    public void Event_DestroyItem(string content)
    {
        Item item = ItemManager.Instance.FindInWorld (content);
        Item.Destroy(item);
    }
    public void Event_ChangeProp(string content)
    {
        // encore une preuve qu'il faut que les actions des "events" soient dans les memes que player action
        // et qu'il faut changer player action, parce que c'est plus ouf
        // car la s�paration se fait d�j� dans input
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
            Debug.LogError("no linked item, vas � mazargues");
        }

        PropertyManager.Instance.Action_RemoveProperty(currentItem, content);
    }

}