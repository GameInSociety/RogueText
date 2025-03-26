 using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ContentLoader : DataDownloader{
    public static ContentLoader Instance;
    bool setDescription = false;

    private void Awake() {
        Instance = this;
    }

    public override void Load() {
        base.Load();
    }

    public override void GetCell(int rowIndex, List<string> cells) {
        base.GetCell(rowIndex, cells);

        if (rowIndex == 0)
            return;

        if (!string.IsNullOrEmpty(cells[1]) ) {
            var newStepData = new Step.Data(cells[1]);
            Step._datas.Add(newStepData);
            setDescription = false;
        }

        var stepData = Step._datas.Last();

        string descriptionCell = cells[2];
        if (!string.IsNullOrEmpty(descriptionCell)) {
            // new profile
            var newStepProfile = new Step.Profile(descriptionCell);
            stepData._profiles.Add(newStepProfile);
        }

        var profile = stepData._profiles.Last();

        if (!setDescription) {
            for (int i = 3; i < cells.Count; i++) {
                string cell = cells[i];
                if (string.IsNullOrEmpty(cell))
                    break;
                Slot.Type type = Slot.Type.None;
                switch (cell) {
                    case "none":
                        type = Slot.Type.None;
                        break;
                    case "sequence":
                        type = Slot.Type.Sequence;
                        break;
                    case "text":
                        type = Slot.Type.Text;
                        break;
                    case "item":
                        type = Slot.Type.Item;
                        break;
                    case "property":
                        type = Slot.Type.Property;
                        break;
                    case "value":
                        type = Slot.Type.Value;
                        break;
                    case "check":
                        type = Slot.Type.Check;
                        break;
                    case "tile":
                        type = Slot.Type.Tile;
                        break;
                    default:
                        Debug.LogError($"loading step types : no {cell}");
                        type = Slot.Type.None;
                        break;
                }

                var newSlotData = new Slot.Data();
                newSlotData._type = type;
                profile._slotDatas.Add(newSlotData);
            }
        } else {
            for (int i = 3; i < 3 + profile._slotDatas.Count; i++)
                profile._slotDatas.Last()._description = cells[i];
        }

        setDescription = !setDescription;
    }


}
