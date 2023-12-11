using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Body : Item {

    public override void Init() {
        base.Init();
        var bodyParts_Data = ItemData.GetDatasOfType("body part");

        foreach (var index in bodyParts_Data) {
            var bodyPart = ItemData.Generate_Simple(ItemData.itemDatas[index].name);
            CreateChildItem(bodyPart);
        }
    }

    public void Equip(Item item) {
        var properties = item.GetProps("equipment");

        var tmpParts = new List<BodyPart>();
        foreach (var prop in properties) {
            var part = GetChildItems().Find(x => x.debug_name == prop.name) as BodyPart;
            if (part == null) {
                Debug.LogError("couldn't find part : " + prop.name);
                return;
            }

            var equipedItem = part.GetChildItems().Find(x => x.GetProp("equipment").GetPart("target").content == prop.GetPart("target").content);

            if (equipedItem != null) {
                TextManager.Write("I'm already wearing &a dog&", equipedItem);
                return;
            }

            tmpParts.Add(part);
        }
        if (tmpParts.Count > 0) {
            AvailableItems.RemoveFromWorld(item);
            foreach (var bodyPart in tmpParts) {
                _ = bodyPart.CreateChildItem(item);
                bodyPart.WriteDescription();
                Debug.Log("apply to part : " + bodyPart.debug_name);
            }
        }

    }

    public void Unequip(Item item) {
        if (Player.Instance.GetBody.hasItem(item)) {

            if (item.GetChildItems().Count == 0) {
                TextManager.Write("you're not wearing anything on &the dog&", item);
            }

            // target part : body part
            // removing everything from body part
            foreach (var containedItem in item.GetChildItems()) {
                var props = containedItem.GetProps("equipment");
                if (props.Count > 0)
                    Debug.LogError(containedItem.debug_name + " is in multiple body parts, il faut faciliter ce truc ( simple )");

                Tile.GetCurrent.CreateChildItem(containedItem);
            }


            item.GetChildItems().Clear();
            item.WriteDescription();
            return;
        }

        if (Player.Instance.GetBody.GetChildItems(3).Find(x => x.hasItem(item)) != null) {
            // target part : an item
            // removing item from body part
            var properties = item.GetProps("equipment");
            var tmpParts = new List<BodyPart>();
            foreach (var prop in properties) {
                Debug.Log("prop : " + prop.name);
                var part = GetChildItems().Find(x => x.debug_name == prop.name) as BodyPart;
                if (part == null) {
                    Debug.LogError("couldn't find part : " + prop.name);
                    return;
                }

                part.RemoveItem(item);
            }
            TextManager.Write("&the dog& is on the ground", item);
            _ = Tile.GetCurrent.CreateChildItem(item);
            return;
        }

        TextManager.Write("You're not wearing any &dog&", item);

    }
}