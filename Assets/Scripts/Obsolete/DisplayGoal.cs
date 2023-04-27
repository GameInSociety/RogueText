using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayGoal : DisplayText {

    public static DisplayGoal Instance;

    private bool displayedGoal = false;

    public string goal = "";

    private void Awake()
    {
        Instance = this;
    }

    public override void Start()
    {
        base.Start();

        TextAsset goal_TextAsset = Resources.Load("GoalText") as TextAsset;
    }

    public void UpdateText()
    {
        if ( displayedGoal )
        {
            Hide();
            return;
        }

        Coords dir = Player.Instance.coords - ClueManager.Instance.clueCoords;
        Cardinal cardinal = Coords.GetCardinalFromCoords(dir);
        string direction_str = Coords.GetWordsDirection(cardinal).GetContent("au chien");

        goal = goal.Replace("CLUEPOSITION",direction_str);

        displayedGoal = true;
    }
}
