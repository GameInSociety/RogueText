using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_ItemDataButton : RW_Draggable
{
    public TextMeshProUGUI uiText_Name;
    private ItemData m_ItemData;

    public ItemData ItemData {
        get => m_ItemData;
        set => m_ItemData=value;
    }

    public void Display(ItemData itemData) {

        m_ItemData = itemData;

        Show();

        uiText_Name.text = ItemData.name;
    }

    public override bool CanMerge(RW_Draggable target) {
        var itemCatButton = target.GetComponent<RW_ItemCategoryButton>();
        if (itemCatButton != null)
            return true;
        return base.CanMerge(target);
    }

    public override void Merge(RW_Draggable target) {
        base.Merge(target);

        var itemCatButton = target.GetComponent<RW_ItemCategoryButton>();
        if (itemCatButton != null) {
            RW_DisplayCategory.Instance._displayedCategory.RemoveItemData(ItemData);
            itemCatButton.ItemCat.AddItemData(ItemData);
            RW_DisplayCategory.Instance.UpdateCat();
            FadeOut();
        }
    }

    public override void Select() {
        base.Select();

        var displayItem = RW_PoolManager.Instance.Pull("DisplayItem") as RW_DisplayItem;
        displayItem.Display(ItemData);
    }

    private void OnDisable() {
        RW_PoolManager.Instance.Push("ItemDataButton", this);
        Hide();
    }
}
