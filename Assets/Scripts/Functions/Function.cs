using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public static class Function {

    public static List<Part> parts = new List<Part>();
    static Item item;
    static bool continueOnFail;
    static bool failed = false;

    public static void TryCall( string _line, Item _item) {

        LOG($"\n[{_item.debug_name}] {_line}", Color.yellow);

        string line = _line;

        // target item
        item = _item;

        // search for []
        var functionName = line;

        // continue on fail
        continueOnFail = false;
        if (line.StartsWith('*')) {
            LOG("continue on fail", Color.gray);
            continueOnFail = true;
            line = line.Substring(1);
        }

        parts.Clear();
        // get options
        if (line.Contains('(')) {
            functionName = line.Remove(line.IndexOf('(')).Trim(' ');
            if (!TryInitParts(line)) {
                return;
            }
        }

        MethodInfo info = typeof(Function).GetMethod(
            functionName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        failed = false;

        try {
            info.Invoke(null, null);
        } catch (Exception e) {
            Debug.LogError($"<color=yellow>item: </color>{item.debug_name}\n" +
                $"<color=magenta>line: </color>{line}");
            Debug.LogException(e);
            Fail("Unity Error");
            
        }

        if (!failed) {
            foreach (var part in parts) {
                if (part.HasProp()) {
                    var it = part.HasItem() ? part.item : item;
                    PropertyDescription.Add(it , part.prop, WorldAction.current.source);
                }
            }
        }
    }

    #region write
    static void write() {
        TextManager.Write(GetText(0));
    }
    #endregion

    #region time
    static void triggerEvent() {
        WorldEvent.TriggerEvent(GetText(0));
    }
    #endregion

    #region items
    static void transferTo() {

        if (!HasPart(0) || !GetPart(0).HasItem()) {
            Fail($"no item for transferTo Action");
            return;
        }

        var targetItem = HasPart(1) ? GetItem(0) : item;
        var container = HasPart(1) ? GetItem(1) : GetItem(0);

        var itmText = ItemDescription.DescribeItems(targetItem);

        if (targetItem.HasProp("weight") && container.HasProp("weight") && container.HasProp("capacity")) {
            // get target item pick up props
            var ci_wProp = item.GetProp("weight");
            int ti_weigh = GetItem(0).GetProp("weight").GetNumValue();
            int ti_cap = GetItem(0).GetProp("capacity").GetNumValue();
            // get current item group weight
            int ci_weight = ci_wProp.GetNumValue() * WorldAction.current.GetItems().Count;
            // check if weight goes above capicity
            if (ti_weigh + ci_weight >= ti_cap) {
                Fail($"{itmText} is too big or heavy for {GetItem(0).GetText($"the dog")} ");
                return;
            }
        }

        if (container.hasItem(targetItem)) {
            Fail($"{container.GetText("the dog")} already contains {targetItem.GetText("the dog")}");
            return;
        }
        targetItem.TransferTo(container);

        TextManager.Write($"{itmText} is now in {container.GetText("the dog")}");
    }
    static void destroy() {
        var targetItem = HasPart(0) ? GetItem(0) : item;
        TextManager.Write($"{targetItem.GetText("the dog")} disappeared");
        Item.Destroy(targetItem);
    }

    static void createItem() {

        // createItem (WHAT ITEM, *HOW MUCH, *WHERE)

        var amount = parts.Count == 2 ? GetPart(1).value : 1;

        var itemName = "";
        if (GetPart(0).HasProp())
            itemName = GetProp(0).GetTextValue();
        else
            itemName = GetText(0);

        var newItem = WorldAction.current.tile.CreateChildItem(itemName);
        Debug.Log($"creating item {newItem.debug_name}");

        for (var i = 1; i < amount; i++) {
            _ = WorldAction.current.tile.CreateChildItem(itemName);
        }

        if (WorldAction.current.tile == Tile.GetCurrent) {
            TextManager.Write($"{newItem.GetText("a dog")} appeared");
        } else {
            TextManager.Write($"target tile : {Tile.GetCurrent.coords.ToString()} / event tile : {WorldAction.current.tile.coords.ToString()}");
        }
    }

    static void describe() {
        // the default describe function, 
        if (parts.Count == 0) {
            if (ItemParser.GetCurrent != null && ItemParser.GetCurrent.GetPart(0).properties.Count > 0) {
                TextManager.Write($"{item.GetText("the dog")} {ItemParser.GetCurrent.GetPart(0).properties[0].GetDescription()}");
                return;
            }
            TextManager.Write($"{ItemDescription.DescribeItems(item)}");
            if (item.HasVisibleProps()) {
                TextManager.Write(Property.GetDescription(item.GetVisibleProps()));
            }
            if (item.HasChildItems()) {
                string childItemDescription = ItemDescription.DescribeItems(item.GetChildItems());
                TextManager.Write($"there's {childItemDescription} {item.GetWord().preposition}");
            }
            return;
        }

        // the specific description of the tile, quite to complicated to do in data
        if (GetText(0) == "_TILE") {
            Tile.GetCurrent.Describe();
            return;
        }

        // describing a specific prop in the item parser {"how much does bag weigh"}
        if (GetPart(0).HasProp()) {
            Debug.Log($"has prop");
            var describedItem = GetPart(0).HasItem() ? GetItem(0) : item;
            TextManager.Write($"{describedItem.GetText("the lone dog")} {GetProp(0).GetDescription()}");
            return;
        }

        Debug.Log($"when is it supposed to reach this");
        ItemDescription.DelayDescription($"it's ", GetItem(0));
    }
    #endregion

    #region props
    static void disable() {
        var prop = GetProp(0);
        if (!prop.enabled) {
            var text = prop.HasPart("description") ? prop.GetDescription() : prop.name;
            Fail($"{item.GetText("the dog")} is not {text}");
        }
        prop.enabled = false;
    }

    static void enable() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var prop = GetProp(0);
        if (prop.enabled) {
            var text = prop.HasPart("description") ? prop.GetDescription() : prop.name;
            Fail($"{item.GetText("the dog")} is already {text}");
            return;
        }
        prop.enabled = true;
        Property_CheckEvents(targetItem, prop);
        Debug.Log($"checking events for : {prop.name}");
        prop.SetChanged();
    }

    static void trigger() {
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var actName = GetText(0);
        var itemAct = targetItem.GetData().acts.Find(x=> x.triggers[0] == actName);
        var action = new WorldAction(item, WorldAction.current.tileInfo, itemAct.seq);
        action.Call(); 
    }

    static void breakIf() {
        var feedback = "";
        bool fail = false;

        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;
        var targetProp = GetProp(0);

        var specialFeedback = "";
        if ( HasPart(2))
            specialFeedback = GetPart(2).text;

        switch (GetText(1)) {
            case "enabled":
                fail = targetProp == null || targetProp != null && targetProp.enabled;
                feedback = $"{targetItem.GetText("the dog")} is {targetProp.GetDescription()}";
                break;
            case "disabled":
                fail = targetProp == null || targetProp != null && !targetProp.enabled;
                feedback = $"{targetItem.GetText("the dog")} isn't {targetProp.GetDescription()}";
                break;
            case "max":
                fail = targetProp.GetNumValue() >= targetProp.GetNumValue("max");
                feedback = $"the {targetProp.name} is already full";
                break;
            case "empty":
                fail = targetProp.GetNumValue() == 0;
                feedback = $"{targetItem.GetText("the dog")} is {targetProp.GetDescription()}";
                break;
            case "not empty":
                fail = targetProp.GetNumValue() > 0;
                feedback = $"{targetItem.GetText("the dog")} is already {targetProp.GetDescription()}";
                break;
            case "not same":
                fail = targetProp.GetNumValue() != GetProp(2).GetNumValue();
                specialFeedback = "";
                feedback = $"this {targetItem.debug_name}({targetProp.GetNumValue()})" +
                    $"doesn't match this {GetItem(2).debug_name}({GetProp(2).GetNumValue()})";
                break;
            default:
                break;
        }

        if (fail) {
            var str = string.IsNullOrEmpty(specialFeedback) ? feedback : specialFeedback;
            Fail($"{str}");
        }
    }
    static void IF() {
        Debug.Log($"IF");

        int value = GetProp(0).GetNumValue();
        bool success = TextUtils.GetCondition(GetPart(1).text, value);

        if (success) {
            Debug.Log($"success");
            // do nothing, just continue functions
        } else {
            Debug.Log($"fail, going to next ~~");
            // tell world action to wait for next ~~
            WorldAction.current.goToNextPart = true;
        }
    }

    static void add() {
        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        int addValue = 0;
        int nextValue = 0;

        if (GetPart(1).HasValue()) {
            addValue = GetPart(1).value;
            nextValue = targetProp.GetNumValue() + addValue;
        } else {
            var sourceProp = GetProp(1);
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;

            addValue = sourceProp.GetNumValue();
            nextValue = targetProp.GetNumValue() + addValue;
            if (addValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            // source prop changing
            if (targetProp.HasPart("max")) {
                int max = targetProp.GetNumValue("max");
                int dif = max - nextValue;
                sourceProp.SetValue(-dif);
            } else {
                int dif = nextValue;
                sourceProp.SetValue(-dif);
            }
            Property_CheckEvents(sourceItem, sourceProp);

        }

        targetProp.SetValue(nextValue);
        Property_CheckEvents(targetItem, targetProp);
    }

    static void sub() {

        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        int subValue = 0;
        if (GetPart(1).HasValue()) {
            subValue = GetPart(1).value;
        } else {
            var sourceItem = GetPart(1).HasItem() ? GetItem(1) : item;
            var sourceProp = GetProp(1);

            subValue = sourceProp.GetNumValue();
            if (subValue == 0 && WorldAction.current.source == WorldAction.Source.PlayerAction) {
                Fail($"{sourceItem.GetText("the dog")} doesn't have any {GetProp(1).name} left");
                return;
            }
            sourceProp.SetValue(subValue - targetProp.GetNumValue());
        }

        targetProp.SetValue(targetProp.GetNumValue() - subValue);
        Property_CheckEvents(targetItem, targetProp);
    }

    static void set() {

        // check if we're setting an abstract item
        if (GetPart(0).text.StartsWith('#')) {
                var abstractItem = GetPart(1).text == "this" ? item : GetItem(1);
            WorldData.SetAbstractItem(GetPart(0).text.Remove(0, 1), abstractItem);
            Debug.Log($"setting abstract item : {GetPart(0).text} to : {abstractItem.debug_name}");
            return;
        }

        var targetProp = GetProp(0);
        var targetItem = GetPart(0).HasItem() ? GetItem(0) : item;

        if (!targetProp.enabled)
            return;

        // getting target targetPropValue
        /*if (targetProp.GetPart("value").content == GetText(1) && WorldAction.current.source == WorldAction.Source.PlayerAction) {
            Debug.Log($"item : {targetItem.debug_name}");
            Debug.Log($"prop : {targetProp.name}");
            Fail($"{targetItem.GetText("the dog")} is already {targetProp.GetDescription()}");
            return;
        }*/

        if (GetPart(1).HasProp()) {
            var sourceProp = GetProp(1);
            targetProp.SetValue(sourceProp.GetTextValue());
            // commenté parce que ça fourait la merde avec les coords, elles, qui ne se transf_re pas
            //sourceProp.SetValue(0);
        } else
            targetProp.SetValue(GetPart(1).text);

        Property_CheckEvents(targetItem, targetProp);
    }
    static void Property_CheckEvents(Item targetItem, Property prop) {
        var valueEvents = prop.parts.FindAll(x=>x.key == "OnValue");
        if(valueEvents.Count == 0) return;
        
        foreach (var valueEvent in valueEvents){
            bool call = false;

            var content = valueEvent.content;

            // getting the condition
            var returnIndex = content.IndexOf('\n');
            var conditon = content.Remove(returnIndex);
            var targetValue = 0;
            if (conditon.TrimEnd('>', '<').StartsWith('[')) {
                var propName = TextUtils.Extract('[', conditon, out conditon);
                targetValue = targetItem.GetProp(propName).GetNumValue();
            } else
                targetValue = prop.GetNumValue();

            if (conditon.StartsWith('>')) {
                // if it's under X
                int i = int.Parse(conditon.Remove(0, 1));
                call = targetValue > i;
            } else if (conditon.StartsWith('<')) {
                // if it's above X
                int i = int.Parse(conditon.Remove(0, 1));
                call = targetValue < i;
            } else {
                int i = int.Parse(conditon);
                call = targetValue == i;
            }

            // getting the sequence
            var sequence = content.Remove(0, returnIndex + 1);
            if (call) {
                var tileEvent = new WorldAction(targetItem, WorldAction.current.tileInfo, sequence);
                tileEvent.Call();
            }
        }
    }
    #endregion

    #region parts
    public class Part {
        public string text;
        public string log;
        public Item item;
        public Tile tile;
        public Property prop;
        public int value = -1;

        public bool HasItem() {
            return text.Contains('!');
        }

        public bool HasTile() {
            return text.Contains('[');
        }
        public bool HasProp() {
            return text.Contains('>');
        }
        public bool HasValue() {
            return value >= 0;
        }

        public Part (string text) {
            this.text = text;
        }

        public bool TryInit(Item defaultItem) {

            item = defaultItem;

            // if it's a tile, no item or prop
            if (HasTile()) {
                tile = FetchTile();
                item = tile;
                if (tile != null)
                    LOG($"Found Tile : {tile.debug_name}", Color.green);
                return tile != null;
            }

            // item
            if (HasItem()) {
                item = FetchItem(text);
                if (item == null)
                    return false;
                LOG($"Found Item : {item.debug_name}", Color.green);
            }

            // prop
            if (HasProp()) {
                prop = FetchProp(text, item);
                if (prop == null)
                    return false;
                LOG($"Found Prop : {prop.name}", Color.green);
                if (prop.HasPart("value")) LOG($"value : {prop.GetTextValue()}", Color.green);
            }

            // value
            int v = -1;
            if (int.TryParse(text, out v)) {
                LOG($"Found Num Value : {text}", Color.green);
                value = v;
            }
            return true;
        }

        Item FetchItem(string key) {
            // key
            if (key.Contains('>'))
                key = key.Remove(key.IndexOf('>'));
            key = key.Remove(0, key.IndexOf('!') + 1);
            key = key.Trim(' ');
            // search
            var result = ItemLink.SearchItem(key);
            if (result == null) {
                LOG($"not found", Color.red);
                return null;
            }
            return result;
        }
        Tile FetchTile() {

            var key = TextUtils.Extract('[', text, out text);
            LOG($"Looking for Tile with coords : {key}", Color.cyan);

            var tilesetId = Player.Instance.tilesetId;
            // check for other tile set
            if (key.Contains('{')) {
                Debug.Log($"getting new tile set for : {key}");
                var tilesetKey = TextUtils.Extract('{', key, out key);
                Debug.Log($"new key:{key}");
                Debug.Log($"tilesetkey:{key}");
                if (tilesetKey.Contains('>')) {
                    Debug.Log($"it's a prop");
                    var tsIt = item;
                    if (tilesetKey.Contains('!')) {
                        tsIt = FetchItem(tilesetKey);
                        if (tsIt == null)
                            return null;
                        Debug.Log($"in item {tsIt.debug_name}");
                    }
                    var tilesetProp = FetchProp(tilesetKey, tsIt);
                    tilesetId = tilesetProp.GetNumValue();
                } else {
                    tilesetId = int.Parse(tilesetKey);
                }
                Debug.Log($"new target id tileset id : {tilesetId}");
            }

            // check for addition
            var split = key.Split('+');
            var coords = new Coords();
            foreach (var s in split) {
                // get maybe item
                var it = item;
                if (s.Contains('!')) {
                    it = FetchItem(s);
                    if (it == null)
                        return null;
                }

                // get maybe prop
                var newCoords = Coords.zero;
                if (s.Contains('>')) {
                    var cProp = FetchProp(s, it);
                    if (cProp == null)
                        return null;
                    newCoords = Coords.PropToCoords(cProp, tilesetId);
                    cProp.SetValue(Coords.CoordsToText(newCoords));
                } else
                    newCoords = Coords.TextToCoords(s, tilesetId);
                coords += newCoords;
            }

            Debug.Log($"getting coords : {coords} in tileset {Player.Instance.GetProp("tileset").GetTextValue()}");
            var result = TileSet.tileSets[tilesetId].GetTile(coords);
            if (result == null) {
                Fail($"no tile with coords : {coords} found");
                return null;
            }
            Debug.Log($"target tile is : {item.debug_name}");
            return result;
        }
        Property FetchProp(string key, Item it ) {
            key = key.Remove(0, key.IndexOf('>') + 1);
            key = key.Trim(' ');
            var result = ItemLink.GetProperty(key, it);
            return result;
        }
    }
    static bool TryInitParts(string text) {
        var paramerets_all = TextUtils.Extract('(', text, out text);
        var stringSeparators = new string[] { ", " };
        var split = paramerets_all.Split(stringSeparators, StringSplitOptions.None);
        foreach (var s in split) {
            var part = new Part(s.Trim(' '));
            LOG($"Initing new Part : [{s}]", Color.magenta);
            parts.Add(part);
            if (!part.TryInit(item)) {
                LOG($"Parts Init Failed", Color.red);
                return false;
            }
        }

        return true;
    }

    static bool HasPart(int i) { return i < parts.Count; }

    static Part GetPart(int i) { return parts[i]; }

    static Item GetItem(int i) {
        return parts[i].item;
    }
    static Property GetProp(int i) {
        return parts[i].prop;
    }
    static Tile GetTile(int i) { return parts[i].tile;}
    public static string GetText(int i) {
        return parts[i].text;
    }

    public static void Fail(string message) {
        failed = true;
        if (continueOnFail)
            return;

        WorldAction.current.Fail(message);
        LOG($"[FAIL] {message}", Color.red);
    }
    #endregion

    public static string log;
    public static void LOG(string message, Color color) {
        var txt_color = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string str = $"\n{txt_color}{message}</color>";
        log += str;
    }
}