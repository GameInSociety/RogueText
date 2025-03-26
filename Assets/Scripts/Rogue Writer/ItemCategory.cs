using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCategory
{
    private string m_name = "";
    private List<ItemData> m_itemDatas = new List<ItemData>();
    private List<ItemCategory> m_itemCategories = new List<ItemCategory>();

    public void AddItemData(ItemData itemData) {
        m_itemDatas.Add(itemData);
    }
    public void RemoveCat(ItemCategory cat) {
        if ( !m_itemCategories.Contains(cat) ) {
            Debug.LogError($"category {cat.Name} not contained in {m_name}");
            return; }
        m_itemCategories.Remove(cat);
    }
    public void RemoveItemData(ItemData itemData) {
        if (!ItemDatas.Contains(itemData)) {
            Debug.LogError($"category {itemData.name} not contained in {m_name}");
            return;
        }
        m_itemDatas.Remove(itemData);
    }
    public void AddItemCat(ItemCategory itemCat) {
        m_itemCategories.Add(itemCat);
    }

    public List<ItemCategory> ItemCategories {
        get => m_itemCategories;
    }
    public List<ItemData> ItemDatas {
        get => m_itemDatas;
    }
    public string Name {
        get => m_name;
        set => m_name=value;
    }
}
