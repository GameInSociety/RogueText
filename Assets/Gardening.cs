using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gardening : MonoBehaviour
{
    public static Gardening Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PlayerActionManager.onPlayerAction += HandleOnPlayerAction;
    }

    private void HandleOnPlayerAction(PlayerAction action)
    {
        if ( action.type == PlayerAction.Type.Plant)
        {
            Plant();
        }
    }

    void Plant()
    {
        Item.Remove(InputInfo.GetCurrent.MainItem);
        Item newItem = Item.CreateNewItem("pousse de carotte");
        Tile.current.AddItem(newItem);

        DisplayDescription.Instance.UpdateDescription();
    }

    void Grow()
    {

    }
}
