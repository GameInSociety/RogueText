using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayFeedback : DisplayText{

	public static DisplayFeedback Instance;

	void Awake () {
        Instance = this;
	}

	public override void Start ()
	{
		base.Start ();

        uiText.text = "";
    }

    public override void Display(string str)
    {
        /*Tween.Bounce(transform);

        str = Phrase.ReplaceItemInString(str);

        AudioInteraction.Instance.StartSpeaking(str);

        base.Display(str);*/

        Phrase.Write(str);
    }

    public void DisplayHelp()
    {
        string str = "";

        Display(str);
    }

    public void DisplayTimeOfDay()
    {
        string str = "";

        if ( TimeManager.GetInstance().timeOfDay == 12)
        {
            str = "Il est midi";
        }
        else if(TimeManager.GetInstance().timeOfDay == 0)
        {
            str = "Il est minuit";
        }
        else if (TimeManager.GetInstance().timeOfDay < 12)
        {
            str = "Il est " + TimeManager.GetInstance().timeOfDay + "h du matin";
        }
        else 
        {
            str = "Il est " + (TimeManager.GetInstance().timeOfDay-12) + "h du soir";
        }

        Display(str);
    }


    public void PointNorth()
    {
        string facing = Coords.GetOrientationText(Coords.GetFacing(Player.Instance.direction));
        string str = "Le nord est " + facing;

        Display(str);
    }

    private void DescribeItem()
    {
        
    }

    public void DescribeExterior()
    {
        Direction dir = Direction.East;

        if (Player.Instance.coords.x < 0)
        {
            dir = Direction.West;
        }

        Coords tCoords = TileSet.map.playerCoords + (Coords)dir;

        Tile tile = TileSet.map.GetTile(tCoords);

        string str = "Par la fenêtre, vous apercevez " + tile.WriteDescription();

        if ( tile == null)
        {
            str = "la fenêtre est bloquée par une haie, vous ne voyez rien...";
        }

        Display(str);

    }
}
