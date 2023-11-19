using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldAction {

    public static List<WorldAction> worldActions = new List<WorldAction>();

    public Item item;
    public Coords tileCoords;
    public int tileSetId;
    public Tile tile => TileSet.GetTileSet(tileSetId).GetTile(tileCoords);
    string sequence;

    public static WorldAction current;

    public bool stop = false;

    public WorldAction(Item item, Coords tileCoords, int tileSetId, string sequence) {
        this.item = item;
        this.tileCoords = tileCoords;
        this.tileSetId = tileSetId;
        this.sequence = sequence;
    }

    public static WorldAction Add(WorldAction tEvent) {
        worldActions.Add(tEvent);
        return tEvent;
    }

    #region call
    public void Call() {

        current = this;
        Debug.Log("calling event : ");
        Debug.Log($"item : {item.debug_name}");

        string[] lines = sequence.Split('\n');
        foreach (var line in lines) {
            Function.Call(this, line);
            if (stop) {
                Debug.Log($"text {sequence} stopped at {line}");
                stop = false;
                break;
            }
        }
    }
    public void Stop() {
        stop = true;
    }
    #endregion

}
