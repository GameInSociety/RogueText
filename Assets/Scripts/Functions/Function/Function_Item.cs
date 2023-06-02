using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function_Item : Function
{
    public override void Call()
    {
        base.Call();

        if ( GetParam(0) == "pick up")
        {
            if (Inventory.Instance.HasItem(WorldEvent.current.GetCurrentItem()))
            {
                TextManager.Write("inventory_pickUp_already", WorldEvent.current.GetCurrentItem());
            }
            else
            {
                WorldEvent.current.GetCurrentItem().PickUp();
            }
        }


        if ( GetParam(0) == "throw")
        {
            Item item = Inventory.Instance.GetItem(WorldEvent.current.GetCurrentItem().word.text);

            if (item == null)
            {
                TextManager.Write("inventory_throw_nothing");
                return;
            }

            // remove && add
            Inventory.Instance.RemoveItem(item);
            WorldEvent.current.tile.AddItem(WorldEvent.current.GetCurrentItem());

            TextManager.Write("inventory_throw_sucess", WorldEvent.current.GetCurrentItem());
        }

        if ( GetParam(0) == "destroy")
        {
            Item item;

            if (HasParams())
            {
                string item_name = GetParam(1);
                item = ItemManager.Instance.FindInWorld(item_name);

                if (item == null)
                {
                    Debug.LogError("couldn't find item " + GetParam(1));
                    return;
                }
            }
            else
            {
                item = WorldEvent.current.GetCurrentItem();

            }

            Item.Destroy(item);
        }


        if (GetParam(0) == "create")
        {
            int amount = 1;
            if (HasValue(2))
            {
                amount = ParseParam(2);
            }

            // if the target item starts with '*', getting the value of an other property
            // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
            string item_name = GetParam(1);
            if (item_name.StartsWith('*'))
            {
                string targetPropertyName = item_name.Remove(0, 1);
                item_name = WorldEvent.current.GetCurrentItem().GetProperty(targetPropertyName).value;
            }

            Item item = ItemManager.Instance.CreateInTile(WorldEvent.current.tile, item_name);

            for (int i = 1; i < amount; i++)
            {
                ItemManager.Instance.CreateInTile(WorldEvent.current.tile, item_name);
            }

            if ( WorldEvent.current.tile == Tile.Current)
            {
                TextManager.Write("tile_addItem", item);
            }
        }

        if (GetParam(0) == "require")
        {
            if (HasParam(1))
            {

                string item_name = GetParam(1);
                Item targetItem = ItemManager.Instance.FindInWorld(item_name);

                if (targetItem == null)
                {
                    // found no item in container, inventory or tile
                    // break flow of actions
                    targetItem = ItemManager.Instance.GetDataItem(item_name);
                    TextManager.Write("item_require", targetItem);
                    WorldEvent.current.Break();
                    return;
                }

                TextManager.Write("you use &the dog (override)&", targetItem);
                return;
            }


            Debug.Log("action require any item ?");
            // no target item, just ask for a second item
            if (!WorldEvent.current.HasItem(1))
            {
                TextManager.Write("item_noSecondItem", WorldEvent.current.GetCurrentItem());
                InputInfo.Instance.WaitForItem();
                WorldEvent.current.Break();
                return;
            }
        }

        if ( GetParam(0) == "describe")
        {
            WorldEvent.current.GetCurrentItem().WriteDescription();
        }
    }
}
