using System.Collections.Generic;
using UnityEngine;

public class PropertyTest : MonoBehaviour
{
    private static PropertyTest instance;
    public static PropertyTest Instance {
        get {
            if ( instance == null) {
                instance = GameObject.FindObjectOfType(typeof(PropertyTest)) as PropertyTest;
            }

             return instance;
        }
    }

    public List<Property> properties = new List<Property>();
}
