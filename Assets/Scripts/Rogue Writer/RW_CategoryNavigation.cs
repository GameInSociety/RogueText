using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RW_CategoryNavigation : MonoBehaviour
{
    public static RW_CategoryNavigation Instance;

    private List<ItemCategory> m_Categories = new List<ItemCategory>();



    public RectTransform parent;

    private void Awake() {
        Instance = this;
    }

    public void DisplayCategory(ItemCategory category) {

        StartCoroutine(Crotine(category));
    }

    IEnumerator Crotine(ItemCategory category) {

        parent.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        parent.gameObject.SetActive(true);

        // Check if category is already in navigation
        int index = m_Categories.FindIndex(x => x == category);
        if (index >= 0) {
            for (int i = m_Categories.Count-1; i >= index; i--)
                m_Categories.RemoveAt(i);
        }

        m_Categories.Add(category);

        // Displaying categories
        for (int i = 0; i < m_Categories.Count; i++) {

            var button = RW_PoolManager.Instance.Pull("ItemCategoryNavigationButton", parent) as RW_ItemCategoryButton;
            button.Display(m_Categories[i]);
            button.GetRectTransform.SetSiblingIndex(i);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
    }

    public void SelectCategory(ItemCategory itemCategory) {
        
    }

}
