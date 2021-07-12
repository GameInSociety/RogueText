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

		PlayerActionManager.onPlayerAction += HandleOnAction;
    }

    public override void Display(string str)
    {
        Tween.Bounce(transform);

        str = Phrase.ReplaceItemInString(str);

        AudioInteraction.Instance.StartSpeaking(str);

        base.Display(str);
    }

    void HandleOnAction (PlayerAction action)
	{
		switch (action.type) {
		case PlayerAction.Type.Display:
			    Display (action.contents [0]);
			    break;
            case PlayerAction.Type.DescribeExterior:
                DescribeExterior();
                break;
            case PlayerAction.Type.DisplayTimeOfDay:
                DisplayTimeOfDay();
                break;
            case PlayerAction.Type.DescribeItem:
                DescribeItem();
                break;
            case PlayerAction.Type.PointNorth:
                PointNorth();
                break;
            case PlayerAction.Type.DisplayHelp:
                DisplayHelp();
                break;
            default:
			break;
		}
	}

    private void DisplayHelp()
    {
        string str = "";

        Display(str);
    }

    private void DisplayTimeOfDay()
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


    private void PointNorth()
    {
        string facing = Coords.GetOrientationText(Coords.GetFacing(Player.Instance.direction));
        string str = "Le nord est " + facing;

        Display(str);
    }

    private void DescribeItem()
    {
        Item item = InputInfo.GetCurrent.MainItem;

        string str = "";
        int count = 0;

        foreach (var verb in Verb.GetVerbs)
        {
            foreach (var combination in verb.combinations)
            {
                if ( combination.itemIndex == item.index)
                {
                    if (verb.helpPhrase.Length > 2)
                    {
                        if (count == 0)
                        {
                            str += verb.helpPhrase.Replace("ITEM", item.word.GetContent("le chien"));
                        }
                        else
                        {
                            if (item.word.genre == Word.Genre.Masculine)
                            {
                                str += "\n" + verb.helpPhrase.Replace("ITEM", "il");
                            }
                            else
                            {
                                str += "\n" + verb.helpPhrase.Replace("ITEM", "elle");
                            }
                        }

                        ++count;
                    }
                }
                
            }
            
        }

        if ( count == 0)
        {
            Display("Vous ne vous pouvez pas faire grand chose avec " + item.word.GetContent("le chien"));
        }
        else
        {
            Display(str);
        }

    }

    private void DescribeExterior()
    {
        Direction dir = Direction.East;

        if (Player.Instance.coords.x < 0)
        {
            dir = Direction.West;
        }

        Coords tCoords = TileSet.map.playerCoords + (Coords)dir;

        Tile tile = TileSet.map.GetTile(tCoords);

        string str = "Par la fenêtre, vous apercevez " + tile.GetDescription();

        if ( tile == null)
        {
            str = "la fenêtre est bloquée par une haie, vous ne voyez rien...";
        }

        Display(str);

    }
}
