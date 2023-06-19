using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Body : Item
{
    public List<Condition> conditions;

    public void Init()
    {
        foreach (Part part in Enum.GetValues(typeof(Part)))
        {
            string itemName = TextManager.ToLowercaseNamingConvention(part.ToString(), true);
            Item item = Item.CreateFromData(itemName);
            var serializedParent = JsonConvert.SerializeObject(item);
            BodyPart bodyPart = JsonConvert.DeserializeObject<BodyPart>(serializedParent);
            AddItem(bodyPart);
        }
    }

    public enum Part
    {
        Head,
        Neck,
        Chest,
        Arms,
        LeftArm,
        RightArm,
        Hands,
        LeftHand,
        RightHand,
        Legs,
        LeftLeg,
        RightLeg,
        Feet,
        LeftFoot,
        RightFoot,
    }

    public void Equip(Item item)
    {
        List<Property> properties = item.GetPropertiesOfType("equip");

        List<BodyPart> tmpParts = new List<BodyPart>();
        foreach (var prop in properties)
        {
            BodyPart part = GetContainedItems.Find(x => x.debug_name == prop.name) as BodyPart;
            if (part == null)
            {
                Debug.LogError("couldn't find part : " + prop.name);
                return;
            }

            Item equipedItem = part.GetContainedItems.Find(x => x.GetPropertyOfType("equip").value == prop.value);

            if (equipedItem != null)
            {
                TextManager.Write("I'm already wearing &a dog&", equipedItem);
                return;
            }

            tmpParts.Add(part);
        }
        if ( tmpParts.Count > 0)
        {
            Item.RemoveFromContainer(item);
            foreach (var bodyPart in tmpParts)
            {
                bodyPart.AddItem(item);
                bodyPart.WriteDescription();
                Debug.Log("apply to part : " + bodyPart.debug_name);
            }
        }




    }

    public void Unequip(Item item)
    {
        if (Player.Instance.body.HasItem(item)){

            if ( item.GetContainedItems.Count == 0)
            {
                TextManager.Write("you're not wearing anything on &the dog&", item);
            }

            // target part : body part
            // removing everything from body part
            foreach (var containedItem in item.GetContainedItems)
            {
                var props = containedItem.GetPropertiesOfType("equip");
                if ( props.Count > 0 )
                {
                    Debug.LogError(containedItem.debug_name + " is in multiple body parts, il faut faciliter ce truc ( simple )");
                }

                Tile.GetCurrent.AddItem(containedItem);
            }


            item.GetContainedItems.Clear();
            item.WriteDescription();
            return;
        }

        if (Player.Instance.body.GetAllItems().Find(x => x.HasItem(item)) != null)
        {
            // target part : an item
            // removing item from body part
            List<Property> properties = item.GetPropertiesOfType("equip");
            List<BodyPart> tmpParts = new List<BodyPart>();
            foreach (var prop in properties)
            {
                Debug.Log("prop : " + prop.name);
                BodyPart part = GetContainedItems.Find(x => x.debug_name == prop.name) as BodyPart;
                if (part == null)
                {
                    Debug.LogError("couldn't find part : " + prop.name);
                    return;
                }

                part.RemoveItem(item);
            }
            TextManager.Write("&the dog& is on the ground", item);
            Tile.GetCurrent.AddItem(item);
            return;
        }

        TextManager.Write("You're not wearing any &dog&", item);

    } 

    public BodyPart GetRandomBodyPart()
    {
        return GetContainedItems[UnityEngine.Random.Range(0, GetContainedItems.Count)] as BodyPart;
    }

    public Item GetPart(Part part)
    {
        return GetContainedItems[(int)part];
    }

}