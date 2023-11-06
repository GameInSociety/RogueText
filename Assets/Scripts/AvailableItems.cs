using System.Collections.Generic;
using System.Security.Policy;
using System.Security.Principal;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Linq;
using static UnityEditor.Progress;

[System.Serializable]
public class AvailableItems {

    private static AvailableItems _inst;
    public static AvailableItems Get {
        get {
            if ( _inst == null )
                _inst = new AvailableItems();

            return _inst;
        }
    }
    
    public List<Item> currItems = new List<Item>();
    public List<Item> recentItems = new List<Item>();
    public List<Item> debug_presentInInput = new List<Item>();

    public Item findInTargetText(string text) {
        return currItems.Find(x => x.containedInText(text));
    }
    public Item getItemOfType(string type) {
        return currItems.Find(x => x.HasInfo(type));
    }
    public Item getItemOfProperty(string property) {
        return currItems.Find(x => x.hasProperty(property));
    }
    public Item getItemOfName(string name) {
        return currItems.Find(x => x.HasWord(name));
    }

    public void removeFromWorld(Item targetItem) {
        foreach (var item in AvailableItems.Get.currItems) {
            if (item.hasItem(targetItem)) {
                Debug.Log($"removing {targetItem.debug_name} from {item.debug_name}");
                item.RemoveItem(targetItem);
                return;
            }
        }
        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }

    public List<Item> searchInText(string text) {

        List<Item> its = currItems.FindAll(x => x.containedInText(text));
        debug_presentInInput = its;
        return its;
    }


    public static void updateItems() {

        var newAvailableItems = new List<Item>();
        
        // the tile and all it's contained items
        newAvailableItems.AddRange(Tile.GetCurrent.getRecursive(2));
        // it's important that it's after the tile, otherwise the parser will search the inventory first (ex: take plate, you already have it)
        // add spec "my" ou "inventory"// add different keys to specs (my+inventory+in bag)
        // ah non. parce que ça peut être un container aussi. "take field plate". "take bag plate". ça y est toujorus ça ?
        // the surrouning tiles
        newAvailableItems.AddRange(Tile.GetCurrent.getExits());
        // the player and all it's things
        newAvailableItems.AddRange(Player.Instance.getRecursive(5));

        Get.currItems = newAvailableItems;
    }
}
