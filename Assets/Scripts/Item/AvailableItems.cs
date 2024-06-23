using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;

public static class AvailableItems {
    private static List<Item> currItems = new List<Item>();
    public static List<Item> testItems = new List<Item>();
    private static Item noItem;

    public static List<Category> categories = new List<Category>();

    public class Category {
        public string name;
        public List<Item> items = new List<Item>();
        public Category(string _name) {
            name = _name;
        }

        public void AddItems(List<Item> items) {
            foreach (Item item in items) {
                AddItem(item);
            }
        }
        public void AddItem(Item item) {
            if (items.Contains(item)) {
                Debug.Log($"{name} already contains ({item._debugName})");
                return;
            }
            items.Add(item);
            currItems.Add(item);
        }
    }

    public static Item NoItem() {

        if (noItem == null)
            noItem = ItemData.Generate_Simple("no item");
        return noItem;
    }

    public static Item getItemOfProperty(string property) {
        return currItems.Find(x => x.HasProp(property));
    }
    public static Item getItemOfName(string name) {
        return currItems.Find(x => x.HasWord(name));
    }

    public static void RemoveFromWorld(Item targetItem) {

    }

    // items are updated :
    // - on input
    // - when an item is added / removed / transfered
    public static void UpdateItems() {

        Debug.Log($"Available Items: The Current Tile should reset here");
    }
    public static void Add(string cat, List<Item> items) {
        var category = categories.Find(x => x.name == cat);
        if ( category == null) {
            category = new Category(cat);
            categories.Add(category);
        }
        category.AddItems(items);
    }
    public static void Add(string cat, Item item) {
        Add (cat, new List<Item>() { item});
    }

    public static void Clear(string cat) {
        var category = categories.Find(x=> x.name == cat);
        if (category == null) {
            Debug.LogError($"Available Items : no category named {cat}");
            return;
        }
        int count = currItems.RemoveAll(x => category.items.Contains(x));
        Debug.Log($"removed {count} items from {cat}" );
        categories.Remove(category);
    }

    public static List<Item> GetAll() {
        return currItems;
    }
}