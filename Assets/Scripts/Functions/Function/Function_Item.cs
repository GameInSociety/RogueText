using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_Item : Function
{
    public override void Call()
    {
        base.Call();
        Call(this);
    }

    void pickUp()
    {
        if (Player.Inventory.HasItem(GetItem()))
        {
            TextManager.Write("inventory_pickUp_already", GetItem());
        }
        else
        {
            Item.RemoveFromContainer(GetItem());

            Player.Inventory.AddItem(GetItem());

            int count = FunctionSequence.current.items.Count;

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
        Item item = Player.Inventory.GetItem(GetItem().word.text);

        if (item == null)
        {
            TextManager.Write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Player.Inventory.RemoveItem(item);
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

        Item item = FunctionSequence.current.tile.CreateItem(item_name);

        for (int i = 1; i < amount; i++)
        {
            FunctionSequence.current.tile.CreateItem(item_name);
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
            Item targetItem = AvailableItems.SearchByName(item_name);

            if (targetItem == null)
            {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = Item.GetDataItem(item_name);
                TextManager.Write("item_require", targetItem);
                FunctionSequence.current.Break();
                return;
            }
            return;
        }


        Debug.LogError("REQUIRE STOP WORKING BECAUSE ONLY ONE ITEM IN FUNCTION SEQUENCE.");
        Debug.LogError("USE *search INSTEAD ?");
        // no target item, just ask for a second item
        //if (!HasItem(1))
        if (false)
        {
            //group.WaitForSpecificItem("item_noSecondItem");
            FunctionSequence.current.Break();
            return;
        }
    }

    void describe()
    {
        GetItem().WriteDescription();
    }

    
}
