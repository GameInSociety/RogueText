
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class RW_ItemCategoryButton : RW_Draggable
{
    public TextMeshProUGUI uiText_Name;
    private ItemCategory m_ItemCat;

    public bool navigation = false;
    public int navigationIndex = -1;

    public void Display(ItemCategory itemCat) {

        m_ItemCat = itemCat;

        Show();

        uiText_Name.text = ItemCat.Name;
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
            RW_DisplayCategory.Instance._displayedCategory.RemoveCat(ItemCat);
            itemCatButton.ItemCat.AddItemCat(ItemCat);
            RW_DisplayCategory.Instance.UpdateCat();
            FadeOut();
        }
    }

    private void OnDisable() {
        if (navigation) {
            RW_PoolManager.Instance.Push("ItemCategoryNavigationButton", this);
        } else {
            RW_PoolManager.Instance.Push("ItemCategoryButton", this);
        }

        Hide();
    }


    public override void Select() {
        base.Select();
        
        EngineManager.Instance.DisplayCategory.Display(ItemCat);
    }

    public ItemCategory ItemCat {
        get => m_ItemCat;
    }

}
