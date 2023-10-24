
using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionManager : MonoBehaviour {
    public string targetItemName = "plate";
    private void Update() {
        if ( false) {
            int depth = 1;
            var result = (Item)null; 
            while (result == null) {

                var results = Tile.GetCurrent.getRecursive(depth);
                result = results.Find(x => x.debug_name == targetItemName);
                ++depth;
                if (depth == 10) {
                    Debug.LogError("search broke out");
                    break;
                }
            }

            Debug.Log("found plate : " + result.debug_name);
        }
    }
}