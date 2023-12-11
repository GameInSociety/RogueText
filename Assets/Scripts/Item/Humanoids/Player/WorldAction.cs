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
    public string debug_name;

    public ItemGroup itemGroup;
    public Coords tileCoords;
    public int tileSetId;
    public Tile tile => TileSet.GetTileSet(tileSetId).GetTile(tileCoords);
    public string sequence;

    public static WorldAction current;

    public bool failed = false;

    public WorldAction(Item item, string sequence) {
        debug_name = $"{item.debug_name} ({sequence.Remove(5)})";
        itemGroup = new ItemGroup(0, Word.Number.Singular);
        itemGroup.items = new List<Item> { item };
        itemGroup.debug_name = item.debug_name;
        this.sequence = sequence;
    }

    public WorldAction(Item item, Coords tileCoords, int tileSetId, string sequence) {
        debug_name = $"{item.debug_name} ({sequence.Remove(5)})";
        itemGroup = new ItemGroup(0, Word.Number.Singular);
        itemGroup.debug_name = item.debug_name;
        itemGroup.items = new List<Item> { item };
        this.tileCoords = tileCoords;
        this.tileSetId = tileSetId;
        this.sequence = sequence;
    }
    public WorldAction(ItemGroup itemGroup, Coords tileCoords, int tileSetId, string sequence) {
        debug_name = $"{itemGroup.first.debug_name} ({sequence.Remove(5)})";
        this.itemGroup = itemGroup;
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
        failed = false;

        var sequences = sequence.Split("\n%\n");

        for (int i = 0; i < sequences.Length; i++) {
            var currSeq = sequences[i];
            string[] lines = currSeq.Split('\n');
            foreach (var line in lines) {
                Function.Call(this, line);
                if (failed) {
                    Debug.Log($"seq {currSeq} stopped at {line}");
                    break;
                }
            }
            if ( failed )
                continue;
            break;
        }
    }
    public void Fail() {
        failed = true;
    }
    #endregion

}
