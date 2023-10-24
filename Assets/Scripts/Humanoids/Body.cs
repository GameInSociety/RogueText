using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Body : Item {
    public List<Condition> conditions;

    public override void Init(Item copy) {
        base.Init(copy);

        foreach (Part part in Enum.GetValues(typeof(Part))) {
            var itemName = TextManager.ToLowercaseNamingConvention(part.ToString(), true);
            var bodyPart = Item.Generate_Special(itemName) as BodyPart;
            _ = addItem(bodyPart);
        }
    }

    public enum Part {
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

    public void Equip(Item item) {
        var properties = item.GetPropertiesOfType("equip");

        var tmpParts = new List<BodyPart>();
        foreach (var prop in properties) {
            var part = getContainedItems.Find(x => x.debug_name == prop.name) as BodyPart;
            if (part == null) {
                Debug.LogError("couldn't find part : " + prop.name);
                return;
            }

            var equipedItem = part.getContainedItems.Find(x => x.GetPropertyOfType("equip").value == prop.value);

            if (equipedItem != null) {
                TextManager.write("I'm already wearing &a dog&", equipedItem);
                return;
            }

            tmpParts.Add(part);
        }
        if (tmpParts.Count > 0) {
            AvailableItems.Get.removeFromWorld(item);
            foreach (var bodyPart in tmpParts) {
                _ = bodyPart.addItem(item);
                bodyPart.writeDescription();
                Debug.Log("apply to part : " + bodyPart.debug_name);
            }
        }

    }

    public void Unequip(Item item) {
        if (Player.Instance.GetBody.hasItem(item)) {

            if (item.getContainedItems.Count == 0) {
                TextManager.write("you're not wearing anything on &the dog&", item);
            }

            // target part : body part
            // removing everything from body part
            foreach (var containedItem in item.getContainedItems) {
                var props = containedItem.GetPropertiesOfType("equip");
                if (props.Count > 0) {
                    Debug.LogError(containedItem.debug_name + " is in multiple body parts, il faut faciliter ce truc ( simple )");
                }

                _ = Tile.GetCurrent.addItem(containedItem);
            }


            item.getContainedItems.Clear();
            item.writeDescription();
            return;
        }

        if (Player.Instance.GetBody.getRecursive(3).Find(x => x.hasItem(item)) != null) {
            // target part : an item
            // removing item from body part
            var properties = item.GetPropertiesOfType("equip");
            var tmpParts = new List<BodyPart>();
            foreach (var prop in properties) {
                Debug.Log("prop : " + prop.name);
                var part = getContainedItems.Find(x => x.debug_name == prop.name) as BodyPart;
                if (part == null) {
                    Debug.LogError("couldn't find part : " + prop.name);
                    return;
                }

                part.RemoveItem(item);
            }
            TextManager.write("&the dog& is on the ground", item);
            _ = Tile.GetCurrent.addItem(item);
            return;
        }

        TextManager.write("You're not wearing any &dog&", item);

    }

    public BodyPart GetRandomBodyPart() {
        return getContainedItems[UnityEngine.Random.Range(0, getContainedItems.Count)] as BodyPart;
    }


    public Item GetPart(Part part) {
        return getContainedItems[(int)part];
    }

}