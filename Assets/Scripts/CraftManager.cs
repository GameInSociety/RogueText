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

        PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    private void HandleOnAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.Craft:
                Craft();
                break;
            case PlayerAction.Type.ReadRecipe:
                ReadRecipe();
                break;
            default:
                break;
        }
    }

    private void ReadRecipe()
    {
        List<int> itemRows = new List<int>();

        foreach (var craftInfo in craftInfos)
        {
            if (craftInfo.requiredToolItemRows != null)
            {
                Debug.Log("craft info : " + Item.dataItems[craftInfo.itemRow].word.text + " is not empty");
                itemRows.Add(craftInfo.itemRow);
            }
        }

        CraftInfo targetCraftInfo;

        Debug.Log("item vaklue : "+ InputInfo.GetCurrent.MainItem.value);
        if (InputInfo.GetCurrent.MainItem.value < 0)
        {


            int i = itemRows[Random.Range(0, itemRows.Count)];
            targetCraftInfo = craftInfos[i];

            InputInfo.GetCurrent.MainItem.value = i;
        }
        else
        {
            targetCraftInfo = craftInfos[InputInfo.GetCurrent.MainItem.value];
        }

        List<Item> requiredItems = new List<Item>();

        foreach (var requiredToolrow in targetCraftInfo.requiredToolItemRows)
            requiredItems.Add(Item.dataItems[requiredToolrow]);

        foreach (var requiredItemRow in targetCraftInfo.requiredItemRows)
            requiredItems.Add(Item.dataItems[requiredItemRow]);

        string list_str = Item.ItemListString(requiredItems, Item.ListSeparator.Commas, false);
        Item targetItem = Item.dataItems[targetCraftInfo.itemRow];
        PhraseKey.WritePhrase("/craft_list/ " + list_str, targetItem);
    }

    private void Craft()
    {
        CraftInfo craftInfo = craftInfos[InputInfo.GetCurrent.MainItem.index];
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
                if (Inventory.Instance.GetItem(Item.dataItems[requiredToolrow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(Item.dataItems[requiredToolrow]);
                }
            }

            foreach (var requiredItemRow in craftInfo.requiredItemRows)
            {
                if (Inventory.Instance.GetItem(Item.dataItems[requiredItemRow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(Item.dataItems[requiredItemRow]);
                }
            }

            if (hasAllItems)
            {
                for (int i = 0; i < craftInfo.requiredItemRows.Length; i++)
                {
                    for (int a = 0; a < craftInfo.requiredItemAmounts[i]; a++)
                    {
                        Inventory.Instance.RemoveItem(craftInfo.requiredItemRows[i]);
                    }
                }

                Inventory.Instance.AddItem(InputInfo.GetCurrent.MainItem);

                PhraseKey.WritePhrase("craft_sucess");

            }
            else
            {
                PhraseKey.WritePhrase("craft_unable");
            }
        }
        
    }

    private void LoadCraftables()
    {
        TextAsset textAsset = Resources.Load("Craft") as TextAsset;

        string[] rows = textAsset.text.Split('&');

        int itemIndex = 0;

        craftInfos = new CraftInfo[Item.dataItems.Count];

        for (int rowIndex = 1; rowIndex < rows.Length - 1; rowIndex++)
        {
            string[] cells = rows[rowIndex].Split(';');

            CraftInfo newCraftInfo = new CraftInfo();

            newCraftInfo.itemRow = Item.dataItems[itemIndex].index;

            if (cells[1].Length > 1)
            {
                string[] toolCellParts = cells[1].Split('\n');

                newCraftInfo.requiredToolItemRows = new int[toolCellParts.Length];
                int requiredToolIndex = 0;
                foreach (var cellPart in toolCellParts)
                {
                    string itemName = cellPart.Trim('"');

                    Item requiredToolItem = Item.dataItems.Find(x => x.word.text == itemName);

                    if (requiredToolItem == null)
                    {
                        Debug.LogError("couldn't find tool for item " + Item.dataItems[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredToolItemRows[requiredToolIndex] = requiredToolItem.index;

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

                    Item requiredItem = Item.dataItems.Find(x => x.word.text == itemName);

                    if (requiredItem == null)
                    {
                        Debug.LogError("couldn't find required item for item " + Item.dataItems[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredItemRows[requiredItemIndex] = requiredItem.index;
                    newCraftInfo.requiredItemAmounts[requiredItemIndex] = itemAmount;

                    ++requiredItemIndex;
                }

                craftInfos[itemIndex] = newCraftInfo;

            }

            itemIndex++;
        }
    }
}
