using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public string playerOrientation = "north";
    public string targetOrientation = "front";
    public string relativeCardinal = "?";
    public int index;
    public Coords coords;

    private void OnDrawGizmos() {
        int orientationNum = Humanoid.cardinals.IndexOf(playerOrientation);
        int cardinalNum = Humanoid.orientations.IndexOf(targetOrientation);
        index = (orientationNum + cardinalNum) % Humanoid.orientations.Count;
        relativeCardinal = Humanoid.cardinals[index];
        coords = Humanoid.GetCoordsFromCardinal(relativeCardinal);
    }
}
