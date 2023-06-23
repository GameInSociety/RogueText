using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance;

    public int count = 20;

    public List<Zombie> zombies = new List<Zombie>();

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        /*for (int i = 0; i < count; i++)
        {
            Zombie zombie = new Zombie();

            zombie.Init();

            zombies.Add(zombie);
        }*/

        for (int i = 0; i < count; i++)
        {
            Zombie newZombie = Item.CreateFromDataSpecial("undead") as Zombie;
            newZombie.coords = Player.Instance.coords;
            newZombie.Move(newZombie.coords);

            MapTexture.Instance.UpdateFeedbackMap();
        }

        TimeManager.Instance.onNextHour += HandleOnNextHour;
    }

    private void HandleOnNextHour()
    {

        for (int i = 0; i < zombies.Count; i++)
        {
            Zombie zombie = zombies[i];

            zombie.Advance();

        }

        MapTexture.Instance.UpdateFeedbackMap();
    }
}
