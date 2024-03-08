using UnityEngine;

public class Test : MonoBehaviour {

    public string tile  = "north";
    public Coords coords;

    private void Update() {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D)) {
            Tile.GetCurrent.WriteDescription();
        }
    }

    private void OnDrawGizmos() {
        //tile = TileSet.GetCurrent.GetTile(coords).debug_name;
    }
}
