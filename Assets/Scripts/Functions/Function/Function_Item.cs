using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_Item : Function
{
    public override void TryCall()
    {
        base.TryCall();
        Call(this);
    }

    void pickUp()
    {
        if (Inventory.Instance.HasItem(GetItem()))
        {
            TextManager.Write("inventory_pickUp_already", GetItem());
        }
        else
        {
            Item.Remove(GetItem());

            Inventory.Instance.AddItem(GetItem());


            int count = GetItems.Count;

            if (count > 1)
            {
                if (FunctionSequence.current.itemIndex == 0)
                {
                    TextManager.Write("You took " + count + " &dogs&", GetItem());
                }
            }
            else
            {
                TextManager.Write("You took &the dog&", GetItem());
            }
        }
    }

    void @throw()
    {
        Item item = Inventory.Instance.GetItem(GetItem().word.text);

        if (item == null)
        {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Inventory.Instance.RemoveItem(item);
        FunctionSequence.current.tile.AddItem(GetItem());

        TextManager.Write("inventory_throw_sucess", GetItem());
    }


    void destroy()
    {
        Item.Destroy(GetItem());
    }

    void create()
    {
        int amount = 1;
        if (HasValue(1))
        {
            amount = ParseParam(1);
        }

        // if the target item starts with '*', getting the value of an other property
        // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
        string item_name = GetParam(0);
        if (item_name.StartsWith('*'))
        {
            string targetPropertyName = item_name.Remove(0, 1);
            item_name = GetItem().GetProperty(targetPropertyName).value;
        }

        Item item = ItemManager.Instance.CreateInTile(FunctionSequence.current.tile, item_name);

        for (int i = 1; i < amount; i++)
        {
            ItemManager.Instance.CreateInTile(FunctionSequence.current.tile, item_name);
        }

        if (FunctionSequence.current.tile == Tile.GetCurrent)
        {
            TextManager.Write("tile_addItem", item);
        }
    }

    void require()
    {
        if (HasParam(1))
        {
            string item_name = GetParam(0);
            Item targetItem = AvailableItems.Find(item_name);

            if (targetItem == null)
            {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = ItemManager.Instance.GetDataItem(item_name);
                TextManager.Write("item_require", targetItem);
                FunctionSequence.current.Break();
                return;
            }

            //TextManager.Write("you use &the dog (override)&", targetItem);
            return;
        }


        Debug.Log("action require any item ?");
        // no target item, just ask for a second item
        if (!HasItem(1))
        {
            CurrentItems.WaitForSpecificItem("item_noSecondItem");
            FunctionSequence.current.Break();
            return;
        }
    }

    void describe()
    {
        GetItem().WriteDescription();
    }

    
}
