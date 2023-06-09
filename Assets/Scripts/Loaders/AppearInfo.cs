using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppearInfo
{
    public string name; // for debug porposes ( the inspector )

    public bool usableAnytime = false;

    public List<ItemInfo> itemInfos= new List<ItemInfo>();

    [System.Serializable]
    public struct ItemInfo
    {
        public string name; // for debug purposes
        public int itemIndex;
        public int chanceAppear;
        public int amount;

        public string GetItemName()
        {
            return ItemManager.Instance.dataItems[itemIndex].debug_name;
        }
    }


    public static AppearInfo GetAppearInfo(int itemIndex)
    {
        return ItemManager.Instance.appearInfos[itemIndex];
    }
}
