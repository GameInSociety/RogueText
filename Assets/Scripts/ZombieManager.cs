using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZombieManager : MonoBehaviour {
    public static ZombieManager Instance;

    public int count = 20;

    public List<Zombie> zombies = new List<Zombie>();

    private void Awake() {
        Instance = this;
    }

    public void Init() {
        /*for (int i = 0; i < count; i++)
        {
            Zombie zombie = new Zombie();

            zombie.Init();

            zombies.Add(zombie);
        }*/

        for (var i = 0; i < count; i++) {

            var newZombie = Item.Generate_Special("undead") as Zombie;
            newZombie.coords = GameManager.Instance.startCoords;
            newZombie.Move(newZombie.coords);

            MapTexture.Instance.UpdateFeedbackMap();
        }

        TimeManager.Instance.onNextHour += HandleOnNextHour;
    }

    private void HandleOnNextHour() {

        for (var i = 0; i < zombies.Count; i++) {
            var zombie = zombies[i];

            zombie.Advance();

        }

        MapTexture.Instance.UpdateFeedbackMap();
    }
}
