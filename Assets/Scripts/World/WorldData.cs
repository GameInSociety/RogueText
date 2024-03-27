using System.Collections.Generic;
using UnityEngine;

public static class WorldData {
    public static List<Item> globalItems = new List<Item>();
    public static Item GetGlobalItem(string key) {
        return globalItems.Find(x=>x.debug_name == key);
    }
    public static Item anyItem;

    private static List<AbstractItem> abstractItems = new List<AbstractItem>();
    public static List<Item> GetAbstractItems() {
        var its = new List<Item>();
        foreach (var absIt in abstractItems) {
            its.Add(absIt.item);
        }
        return its;
    }
    public class AbstractItem {
        public AbstractItem(string key, Item item) {
            this.key = key;
            this.item = item;
        }
        public string key;
        public Item item;
    }

    public static void Init() {
        var anyItemIndex = ItemData.GetItemDataIndex("any item");
        var anyItemData = ItemData.itemDatas[anyItemIndex];
        anyItem = ItemData.Generate_Simple(anyItemData.name);

        var globalIndexes = ItemData.GetDatasOfType("global");
        foreach (var index in globalIndexes) {
            var itemData = ItemData.itemDatas[index];
            var newItem = ItemData.Generate_Simple(itemData.debugName);
            globalItems.Add(newItem);
        }
    }

    public static void SetAbstractItem(string key, Item item) {
        int index = abstractItems.FindIndex(x => x.key == key);
        if ( index < 0) {
            abstractItems.Add(new AbstractItem(key, item));
            return;
        }
        abstractItems[index].item = item;
    }
    public static Item GetAbstractItem(string key) {
        int index = abstractItems.FindIndex(x => x.key == key);
        if (index < 0) {
            Debug.LogError($"[GETTING ABSTRACT ITEM] No abstract item with key : {key}");
            return null;
        }
        return abstractItems[index].item;

    }
}
