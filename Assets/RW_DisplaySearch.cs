using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RW_DisplaySearch : RW_Menu
{
    public TMP_InputField _inputField;
    public List<RW_ItemDataButton> _buttons = new List<RW_ItemDataButton>();
    public Transform _parent;

    public void UpdateSearch() {

        string input = _inputField.text;

        foreach (var button in _buttons) {
            RW_PoolManager.Instance.Push("Search_ItemDataButton", button);
            button.Hide();
        }

        if (string.IsNullOrEmpty(input))
            return;

        var itemDatas = ItemData.itemDatas.FindAll(x=> x.name.StartsWith(input));
        Debug.Log($"Results for [{input}] : {itemDatas.Count}");
        foreach (var itemData in itemDatas) {
            var itemDataButton = RW_PoolManager.Instance.Pull("Search_ItemDataButton", _parent) as RW_ItemDataButton;
            itemDataButton.Display(itemData);
            _buttons.Add(itemDataButton as RW_ItemDataButton);
        }
    }
}
