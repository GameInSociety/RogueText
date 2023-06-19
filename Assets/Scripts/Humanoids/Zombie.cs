using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;

public class Zombie : Humanoid
{
    public int stepsAway = 5;

    public const int MaxStepsAway = 9;

    public bool subscribed = false;

    public string[] strings = new string[MaxStepsAway]
    {
        "TRIGGER ATTACK",
        "lauching at you",
        "about to attack",
        "within reach",
        "nearly in front of you",
        "a few steps away",
        "rather close",
        "at an honest distance",
        "at a distance",
    };

    public override void Init()
    {
        base.Init();

        /*List<Tile> tiles = TileSet.current.tiles.Values.ToList().FindAll(x =>
        !x.HasProperty("blocking")
        );

        coords = tiles[UnityEngine.Random.Range(0, tiles.Count)].coords;*/

        currentCarnidal = (Cardinal)(Random.Range(0, 4));

        Move(coords);
    }

    public void Sub()
    {
        if (subscribed)
        {
            return;
        }
        InputInfo.Instance.onAction += HandleOnAction;
        TimeManager.Instance.onNextHour += HandleOnNextHour;
        subscribed = true;
    }

    public void Unsub()
    {
        if (!subscribed)
        {
            return;
        }

        InputInfo.Instance.onAction -= HandleOnAction;
        TimeManager.Instance.onNextHour -= HandleOnNextHour;

        subscribed = false;
    }

    public void SetSteps(int i)
    {
        stepsAway = i;
        stepsAway = Mathf.Clamp(stepsAway, 0, MaxStepsAway);
    }

    private void HandleOnNextHour()
    {
        SetSteps(1);

        WritePosition();
    }

    void HandleOnAction()
    {
        SetSteps(stepsAway - 1);

        if (stepsAway == 0)
        {
            Attack(Player.Instance);
        }
        else
        {
            TextManager.Write("&the dog& is now " + strings[stepsAway], this);
        }
    }

    void Attack(Humanoid humanoid)
    {
        BodyPart targetBodyPart = humanoid.body.GetRandomBodyPart();

        TextManager.Write("&the dog& aims for your ", this);
        TextManager.Add("&dog&", targetBodyPart);

        TextManager.Write("And strikes it");

        Property health = humanoid.GetProperty("health");
        health.SetInt(health.GetInt() - 1);
        health.Describe();

        /*float chanceHitting = Random.value;
        if ( chanceHitting > 0)
        {
            TextManager.Write("And strikes it");
        }*/
    }

    public override void WriteDescription()
    {
        //base.WriteDescription();
        WritePosition();
    }

    void WritePosition()
    {
        if (visible)
        {
            TextManager.Write("&the dog& is " + strings[stepsAway], this);
        }
        else
        {
            visible = true;
            TextManager.Write(strings[stepsAway] + ", there's &a dog&", this);
        }
    }

    public override void Move(Coords targetCoords)
    {

        base.Move(targetCoords);

        TileSet.current.GetTile(targetCoords).AddItem(this);
    }

    public void Advance()
    {
        Coords targetCoords = coords + (Coords)currentCarnidal;

        if (!CanMoveForward(targetCoords))
        {
            Turn();
            return;
        }

        Move(targetCoords);
    }

    public void Turn()
    {
        currentCarnidal += 2;
        if ( currentCarnidal == (Cardinal)0)
        {
            currentCarnidal = 0;
        }
    }
}
