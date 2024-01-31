using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Body : Item {

    public override void Init() {
        base.Init();
        var bodyParts_Data = ItemData.GetDatasOfType("body part");

        foreach (var index in bodyParts_Data) {
            var bodyPart = ItemData.Generate_Simple(ItemData.itemDatas[index].name);
            CreateChildItem(bodyPart);
        }
    }
}