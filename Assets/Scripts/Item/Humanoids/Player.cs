
using UnityEngine;

[System.Serializable]
public class Player : Humanoid {

    public static Player Instance;

    public override void Init() {
        base.Init();
    }

    #region vision
    public bool canSee() {
        if (TimeManager.partOfDay == TimeManager.PartOfDay.Night) {
            if (HasItemWithProp("source of light")) {
                TextManager.Write("lamp_on");
                return true;
            } else {
                TextManager.Write("lamp_off");
                return false;
            }
        }

        return true;
    }
    #endregion

    #region movement
    public override void Move(Coords targetCoords) {
        // cancel if path blocked
        if (!CanMoveForward(targetCoords)) {
            WriteBlockFeedback(targetCoords);
            return;
        }

        // GetMainItem of all, advance time ( and items states etc... )
        TimeManager.AdvanceTime();

        base.Move(targetCoords);

        Tile.SetCurrent(TileSet.GetCurrent.GetTile(coords));

        // generate tile items
        Tile.GetCurrent.Describe();

        MapTexture.Instance.UpdateFeedbackMap();
    }

    public void MoveToTile(Tile tile) {
        Debug.Log("moving to tile : " + tile.debug_name + " / " + tile.debug_randomID);

        if (tile.coords == coords) {
            TextManager.Write("you are already there");
            return;
        }

        Move(tile.coords);

    }


    #endregion

    public void WriteBlockFeedback(Coords c) {
        var targetTile = TileSet.GetCurrent.GetTile(c);

        if (targetTile == null) {
            Debug.LogError("no tile : " + c.ToString());
            TextManager.Write("blocked_void");
            return;
        }

        switch (targetTile.debug_name) {
            case "hill":
                TextManager.Write("blocked_hill");
                break;
            case "mountain":
                TextManager.Write("blocked_mountain");
                break;
            case "sea":
                TextManager.Write("blocked_sea");
                break;
            case "lake":
                TextManager.Write("blocked_lake");
                break;
            case "river":
                TextManager.Write("blocked_river");
                break;
            default:
                break;
        }
    }

    public override void Orient(Orientation orientation) {
        TextManager.Write($"you're now facing {orientation.ToString()}");

        base.Orient(orientation);
    }
}