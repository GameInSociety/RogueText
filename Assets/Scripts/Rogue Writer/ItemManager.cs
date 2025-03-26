using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // Singleton
    public static ItemManager Instance;

    // Data
    private ItemCategory m_InitCategory;

    private void Awake() {
        Instance = this;

        m_InitCategory = new ItemCategory();
        m_InitCategory.Name = "All Items";
    }

    public void AddCategory(string name) {
        var newCat = new ItemCategory();
        newCat.Name = name;
        InitCategory.ItemCategories.Add(newCat);
    }

    #region Tools
    public ItemCategory InitCategory {
        get => m_InitCategory;
    }
    #endregion
}
