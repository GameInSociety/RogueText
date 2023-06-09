using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Story story;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {


        PhraseLoader.Instance.Load();

        VerbLoader.Instance.Load();
        ItemLoader.Instance.Load();

        FunctionListLoader.Instance.Load();

        AppearInfoLoader.Instance.Load();

        MapTexture.Instance.CreateMapFromTexture();

        Inventory.Init();

        Player.Instance = new Player();
        Player.Instance.Init();
        Player.Instance.Move(Cardinal.None);

        //ZombieManager.Instance.Init();

    }


    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Interior interior = Interior.GetCurrent;
            interior.Genererate();

            TileSet.SetCurrent(interior.tileSet);

            MapTexture.Instance.UpdateInteriorMap();
        }
    }*/
}
