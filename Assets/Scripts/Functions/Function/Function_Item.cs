using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_Item : Function {
    public override void Call() {
        base.Call();
        Call(this);
    }

    void pickUp() {
        if (Player.Instance.Inventory.hasItem(targetItem())) {
            TextManager.write("inventory_pickUp_already", targetItem());
        } else {
            targetItem().removeFromTile();

            var count = ItemParser.GetCurrent.numericValueInInput;

            // say before it's in the inventory ( that way it doesn't say "your plate")
            if (count > 1)
                TextManager.write("You took " + count + " &dogs&", targetItem());
            else
                TextManager.write("You took &the dog&", targetItem());

            Player.Instance.Inventory.addItem(targetItem());
            
        }
    }

    void @throw() {
        var item = Player.Instance.Inventory.GetItem(targetItem().word.text);

        if (item == null) {
            TextManager.write("inventory_throw_nothing");
            return;
        }

        // remove && add
        Player.Instance.Inventory.RemoveItem(item);
        _ = FunctionSequence.current.tile.addItem(targetItem());

        TextManager.write("inventory_throw_sucess", targetItem());
    }


    void destroy() {
        Item.Destroy(targetItem());
    }

    void create() {
        var amount = 1;
        if (HasValue(1)) {
            amount = ParseParam(1);
        }

        // if the target item starts with '*', getting the value of an other property
        // sprout gets value "vegetableType" pour savoir en quoi elle va pousser
        var item_name = GetParam(0);
        if (item_name.StartsWith('*')) {
            var targetPropertyName = item_name.Remove(0, 1);
            item_name = targetItem().GetProperty(targetPropertyName).value;
        }

        var item = FunctionSequence.current.tile.addItem(item_name);

        for (var i = 1; i < amount; i++) {
            _ = FunctionSequence.current.tile.addItem(item_name);
        }

        if (FunctionSequence.current.tile == Tile.GetCurrent) {
            TextManager.write("tile_addItem", item);
        }
    }

    void require() {
        if (HasParam(1)) {
            var item_name = GetParam(0);
            var targetItem = AvailableItems.Get.getItemOfName(item_name);

            if (targetItem == null) {
                // found no item in container, inventory or tile
                // break flow of actions
                targetItem = Item.GetDataItem(item_name);
                TextManager.write("item_require", targetItem);
                FunctionSequence.current.Stop();
                return;
            }
            return;
        }


        Debug.LogError("REQUIRE STOP WORKING BECAUSE ONLY ONE ITEM IN FUNCTION SEQUENCE.");
        Debug.LogError("USE *search INSTEAD ?");
        // no target item, just ask for a second item
        //if (!HasItem(1))
        if (false) {
            //group.WaitForSpecificItem("item_noSecondItem");
            FunctionSequence.current.Stop();
            return;
        }
    }

    void describe() {

        Debug.Log("item count " + ItemParser.GetCurrent.potentialItems.Count);
        foreach (var item in ItemParser.GetCurrent.potentialItems) {
            item.writeDescription();
        }
    }


}
