using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public Tile tile;

    public bool colorWords = true;

    public string[] itemsOnStart;

    private void Start()
    {
        string str = "/compas_giveNorth/" + " facing";

        

        Debug.Log("phrase");
        Debug.Log(str);
        Debug.Log("phrase");




        foreach (var itemName in itemsOnStart)
        {
            Item item = Item.CreateNew(itemName);
            Inventory.Instance.AddItem(item);
        }
    }

    private static DebugManager _instance;
    public static DebugManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<DebugManager>().GetComponent<DebugManager>();
            }

            return _instance;
        }
    }
}
