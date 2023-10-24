using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppearInfo {
    public string name; // for debug porposes ( the inspector )

    public bool usableAnytime = false;

    public List<ItemInfo> itemInfos = new List<ItemInfo>();

    [System.Serializable]
    public struct ItemInfo {
        public string name; // for debug purposes
        public int chance;
        public int amount;
    }

    public bool CanContainItems() {
        return itemInfos.Count > 0;
    }


    public static AppearInfo GetAppearInfo(int itemIndex) {
        return Item.appearInfos[itemIndex];
    }
}
