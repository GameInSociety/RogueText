using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour {

    public static CraftManager Instance;

    public CraftInfo[] craftInfos;

    public struct CraftInfo
    {
        public int itemRow;
        public int[] requiredItemRows;
        public int[] requiredItemAmounts;
        public int[] requiredToolItemRows;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        //LoadCraftables();
    }

    private void ReadRecipe()
    {
        List<int> itemRows = new List<int>();

        foreach (var craftInfo in craftInfos)
        {
            if (craftInfo.requiredToolItemRows != null)
            {
                Debug.Log("craft info : " + ItemManager.Instance.dataItems[craftInfo.itemRow].word.text + " is not empty");
                itemRows.Add(craftInfo.itemRow);
            }
        }

        CraftInfo targetCraftInfo;

        /*Debug.Log("item vaklue : "+ InputInfo.GetCurrent.GetItem(0).value);
        if (InputInfo.GetCurrent.GetItem(0).value < 0)
        {


            int i = itemRows[Random.Range(0, itemRows.Count)];
            targetCraftInfo = craftInfos[i];

            InputInfo.GetCurrent.GetItem(0).value = i;
        }
        else
        {
            targetCraftInfo = craftInfos[InputInfo.GetCurrent.GetItem(0).value];
        }

        List<Item> requiredItems = new List<Item>();

        foreach (var requiredToolrow in targetCraftInfo.requiredToolItemRows)
            requiredItems.Add(ItemManager.Instance.dataItems[requiredToolrow]);

        foreach (var requiredItemRow in targetCraftInfo.requiredItemRows)
            requiredItems.Add(ItemManager.Instance.dataItems[requiredItemRow]);

        string list_str = Item.ItemListString(requiredItems, Item.ListSeparator.Commas, false);
        Item targetItem = ItemManager.Instance.dataItems[targetCraftInfo.itemRow];
        TextManager.WritePhrase("/craft_list/ " + list_str, targetItem);*/
    }

    private void Craft()
    {
        CraftInfo craftInfo = craftInfos[WorldEvent.current.GetCurrentItem().dataIndex];
        if ( craftInfo.requiredToolItemRows == null)
        {
            //Phrase.Write("vous ne pouvez pas fabriquer de " + Action.last.primaryItem.word.GetDescription(Word.Def.Undefined,Word.Preposition.De));
            //Phrase.Write("");
        }
        else
        {
            List<Item> missingItems = new List<Item>();
            bool hasAllItems = true;

            foreach (var requiredToolrow in craftInfo.requiredToolItemRows)
            {
                if (Inventory.Instance.GetItem(ItemManager.Instance.dataItems[requiredToolrow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(ItemManager.Instance.dataItems[requiredToolrow]);
                }
            }

            foreach (var requiredItemRow in craftInfo.requiredItemRows)
            {
                if (Inventory.Instance.GetItem(ItemManager.Instance.dataItems[requiredItemRow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(ItemManager.Instance.dataItems[requiredItemRow]);
                }
            }

            if (hasAllItems)
            {
                for (int i = 0; i < craftInfo.requiredItemRows.Length; i++)
                {
                    for (int a = 0; a < craftInfo.requiredItemAmounts[i]; a++)
                    {
                        //Inventory.Instance.RemoveItem(craftInfo.requiredItemRows[i]);
                    }
                }

                Inventory.Instance.AddItem(WorldEvent.current.GetCurrentItem());

                TextManager.Write("craft_sucess");

            }
            else
            {
                TextManager.Write("craft_unable");
            }
        }
        
    }

    private void LoadCraftables()
    {
        TextAsset textAsset = Resources.Load("Craft") as TextAsset;

        string[] rows = textAsset.text.Split('&');

        int itemIndex = 0;

        craftInfos = new CraftInfo[ItemManager.Instance.dataItems.Count];

        for (int rowIndex = 1; rowIndex < rows.Length - 1; rowIndex++)
        {
            string[] cells = rows[rowIndex].Split(';');

            CraftInfo newCraftInfo = new CraftInfo();

            newCraftInfo.itemRow = ItemManager.Instance.dataItems[itemIndex].dataIndex;

            if (cells[1].Length > 1)
            {
                string[] toolCellParts = cells[1].Split('\n');

                newCraftInfo.requiredToolItemRows = new int[toolCellParts.Length];
                int requiredToolIndex = 0;
                foreach (var cellPart in toolCellParts)
                {
                    string itemName = cellPart.Trim('"');

                    Item requiredToolItem = ItemManager.Instance.dataItems.Find(x => x.word.text == itemName);

                    if (requiredToolItem == null)
                    {
                        Debug.LogError("couldn't find tool for item " + ItemManager.Instance.dataItems[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredToolItemRows[requiredToolIndex] = requiredToolItem.dataIndex;

                    ++requiredToolIndex;
                }


                string[] itemCellParts = cells[2].Split('\n');

                newCraftInfo.requiredItemRows = new int[itemCellParts.Length];
                newCraftInfo.requiredItemAmounts = new int[itemCellParts.Length];

                int requiredItemIndex = 0;
                foreach (var cellPart in itemCellParts)
                {
                    string itemName = cellPart.Trim('"');
                    int itemAmount = 1;

                    if (itemName.Contains(","))
                    {
                        string[] str = itemName.Split(',');

                        itemName = str[0];

                        itemAmount = int.Parse(str[1].Remove(0, 1));
                    }

                    Item requiredItem = ItemManager.Instance.dataItems.Find(x => x.word.text == itemName);

                    if (requiredItem == null)
                    {
                        Debug.LogError("couldn't find required item for item " + ItemManager.Instance.dataItems[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredItemRows[requiredItemIndex] = requiredItem.dataIndex;
                    newCraftInfo.requiredItemAmounts[requiredItemIndex] = itemAmount;

                    ++requiredItemIndex;
                }

                craftInfos[itemIndex] = newCraftInfo;

            }

            itemIndex++;
        }
    }
}
