
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClueManager : MonoBehaviour {

	public static ClueManager Instance;

    public string goal_text;

	public Coords bunkerCoords;

    public Coords clueCoords;

    public string letterContent;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
    public void Init()
    {
        int bunkerID = Random.Range(1, Interior.interiors.Count);
        //bunkerCoords = Interior.interiors.Values.ElementAt(bunkerID).coords;

        int clueID = Random.Range(1, Interior.interiors.Count);
        if ( clueID == bunkerID)
        {
            if ( clueID == 1)
            {
                clueID++;
            }
            else
            {
                clueID--;
            }
        }
        clueCoords = Interior.interiors.Values.ElementAt(clueID).coords;

	}
}
