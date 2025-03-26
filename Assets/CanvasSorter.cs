using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasSorter : MonoBehaviour
{
    public static CanvasSorter Instance;

    private void Awake() {
        Instance = this;
    }

    public void Sort() {
        var menus = GetComponentsInChildren<RW_Menu>();
        Debug.Log($"menu count : {menus.Length}");
        for (int i = 0; i < menus.Length; i++) {
            menus[i].Canvas.sortingOrder = i+1;
        }
    }
}
