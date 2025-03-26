using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_DisplayCategory : MonoBehaviour
{
    public static RW_DisplayCategory Instance;
    public Vector2 buffer;
    public Vector2 decal;

    public RectTransform parent;

    List<RW_Draggable> buttons = new List<RW_Draggable>();

    public ItemCategory _displayedCategory;

    private void Awake() {
        Instance = this;
    }

    public void Display(ItemCategory category) {

        _displayedCategory = category;

        UpdateCat();
    }

    public void UpdateCat() {
        parent.gameObject.SetActive(false);
        Invoke($"UpdateCatDelay", 0f);
    }

    void UpdateCatDelay() {

        parent.gameObject.SetActive(true);
        buttons.Clear();

        // Displaying categories
        for (int i = 0; i < _displayedCategory.ItemCategories.Count; i++) {
            var newButton = RW_PoolManager.Instance.Pull("ItemCategoryButton", parent) as RW_ItemCategoryButton;
            newButton.Display(_displayedCategory.ItemCategories[i]);
            buttons.Add(newButton);
        }

        // Displaying items
        for (int i = 0; i < _displayedCategory.ItemDatas.Count; i++) {
            var newButton = RW_PoolManager.Instance.Pull("ItemDataButton", parent) as RW_ItemDataButton;
            newButton.Display(_displayedCategory.ItemDatas[i]);
            buttons.Add(newButton);
        }

        Debug.Log($"Item Datas : {_displayedCategory.ItemDatas.Count}");
        Debug.Log($"Item Cats : {_displayedCategory.ItemCategories.Count}");
        Debug.Log($"Buttons : {buttons.Count}");

        // Add Category to Navigation
        RW_CategoryNavigation.Instance.DisplayCategory(_displayedCategory);

        // Updating canvas to get full size with text
        Canvas.ForceUpdateCanvases();

        var pos = new Vector2(buffer.x, buffer.y);
        var maxWidth = parent.rect.width - (buffer.x - decal.x);

        for (int i = 0; i < buttons.Count; i++) {
            // pos
            float nextX = pos.x + buttons[i].GetRectTransform.rect.width + decal.x;
            if (nextX >= maxWidth) {
                pos.x = buffer.x;
                pos.y += decal.y;
            }

            buttons[i].GetRectTransform.anchoredPosition = pos;

            pos.x = pos.x + buttons[i].GetRectTransform.rect.width + decal.x;
        }
    }

    
}
