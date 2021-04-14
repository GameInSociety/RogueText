
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClueManager : MonoBehaviour {

	public static ClueManager Instance;

    public string goal_text;

	public Coords bunkerCoords;

    public Coords clueCoords;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
    public void Init()
    {

        int bunkerID = Random.Range(1, Interior.interiors.Count);
        bunkerCoords = Interior.interiors.Values.ElementAt(bunkerID).coords;

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

        ActionManager.onAction += HandleOnAction;

	}

    private void HandleOnAction(Action action)
    {
        switch (action.type)
        {
            case Action.Type.GiveClue:
                DisplayBunkerSurrounding();
                break;
            case Action.Type.MoveAway:
                CheckMoveAway();
                break;
            default:
                break;
        }
    }

    private void CheckMoveAway()
    {
        if (Interior.GetCurrent.coords == bunkerCoords)
        {
            Item.Remove(InputInfo.GetCurrent.MainItem);

            DisplayDescription.Instance.AddToDescription
                ("Derrière le tableau, un trou béant se dévoile.\n" +
                "Après avoir rampé de longues minutes, une grotte apparait.\n" +
                "Des centaines de gens vivent ici.\n" +
                "Ils marchandent, parlent, dorment et flannent. C'est le début d'une nouvelle ère\n" +
                "\n" +
                "Vous avez gagné.");

            DisplayInput.Instance.EndInput();
        }
    }

	public void GetClueText() {

		string str = "";

	}

    void DisplayBunkerSurrounding ()
	{
		List<Direction> directions = new List<Direction> ();

        int clueAmount = Random.Range(1, 2);

        Direction randomDirection = (Direction)Random.Range(0, 8);

		string positionPhrase = PhraseManager.Instance.positionPhrases [Random.Range (0, PhraseManager.Instance.positionPhrases.Length)];
		string locationPhrase = PhraseManager.Instance.locationPhrases [Random.Range (0, PhraseManager.Instance.locationPhrases.Length)];

		string str = "";

        string facingPart = "";
        string placePart = "";

        string direction_str = Coords.GetWordsDirection(randomDirection).GetContent(Word.ContentType.ArticleAndWord , Word.Definition.Defined, Word.Preposition.A, Word.Number.Singular);

        facingPart += direction_str;

        Tile tile = TileSet.map.GetTile( bunkerCoords + (Coords)randomDirection);

        string wordGroup = tile.tileItem.word.GetContent(Word.ContentType.FullGroup, Word.Definition.Undefined, Word.Preposition.None, Word.Number.Singular);

        placePart = wordGroup + " " + locationPhrase;

        str = placePart + " " + facingPart;

        DisplayFeedback.Instance.Display("On parle ici d'un bunker où les gens sont en sécurité." +
            "\n" +
            "On dit qu'" + str + " de l'abri");

	}

}
