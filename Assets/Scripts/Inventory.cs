using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Item {

	public static Inventory Instance;

    public int maxWeight = 15;

    public static void Init()
    {
        Item item = ItemManager.Instance.GetDataItem("inventory");
        var serializedParent = JsonConvert.SerializeObject(item);
        Instance = JsonConvert.DeserializeObject<Inventory>(serializedParent);
    }

}
