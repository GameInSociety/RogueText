using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionList
{
    public static void TryFunction(string function)
    {
        switch (function)
        {
            case "Move":
                Player.Instance.Move((Cardinal)CellEvent.GetValue(0));
                break;
            case "MoveRel":
                Player.Orientation moveOrientation = (Player.Orientation)CellEvent.GetValue(0);
                Player.Instance.Move(Player.OrientationToCardinal(moveOrientation));
                break;
            case "OrientPlayer":
                Player.Orientation lookOrientation = (Player.Orientation)CellEvent.GetValue(0);
                Player.Instance.Orient(lookOrientation);
                break;
            case "MoveToTargetItem":
                Player.Instance.MoveToTargetItem();
                break;
            case "UseDoor":
                Player.Instance.UseDoor();
                break;
            case "Enter":
                Interior.Get(Player.Instance.coords).Enter();
                break;
            case "DescribeExterior":
                Interior.DescribeExterior();
                break;
            case "DisplayTimeOfDay":
                TimeManager.GetInstance().WriteTimeOfDay();
                break;
            case "PointNorth":
                Coords.WriteDirectionToNorth();
                break;
            case "Write":
                TextManager.Write(CellEvent.GetContent(0));
                break;
            case "SetState":
                ConditionManager.GetInstance().Event_SetCondition();
                break;
            case "Wait":
                TimeManager.GetInstance().Event_Wait();
                break;
            case "PickUp":
                InventoryManager.Event_PickUp();
                break;
            case "CreateInTile":
                InventoryManager.Event_CreateInTile();
                break;
            case "DestroyItem":
                InventoryManager.Event_DestroyItem();
                break;
            case "RequireItem":
                InventoryManager.Event_RequireItem();
                break;
            case "Throw":
                InventoryManager.Event_Throw();
                break;
            case "ChangeProp":
                PropertyManager.Event_ChangeProperty();
                break;
            case "AddProp":
                PropertyManager.Event_AddProperty();
                break;
            case "RemoveProp":
                PropertyManager.Event_RemoveProperty();
                break;
            case "CheckProp":
                PropertyManager.Event_CheckProp();
                break;
            case "CheckPropValue":
                PropertyManager.Event_CheckPropertyValue();
                break;
            case "EnableProp":
                PropertyManager.Event_EnableProperty();
                break;
            case "DisableProp":
                PropertyManager.Event_DisableProperty();
                break;
            case "RequireItemWithProp":
                PropertyManager.Event_RequireItemWithProp();
                break;
            case "Equip":
                Equipment.Instance.Event_Equip();
                break;
            case "Unequip":
                Equipment.Instance.Event_Unequip();
                break;
            case "LookAround":
                Tile.GetCurrent.Describe();
                break;
            case "Describe":
                InputInfo.Instance.GetItem(0).WriteDescription();
                break;
            default:
                Debug.LogError("couldn't find function : " + function);
                break;
        }
    }


}
