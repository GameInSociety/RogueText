using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = false;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(500, 500, false);

        AdjectiveLoader.Instance.Load();

        PhraseLoader.Instance.Load();

        VerbLoader.Instance.Load();
        ItemLoader.Instance.Load();

        ItemAppearInfoLoader.Instance.Load();
        ItemPositionLoader.Instance.Load();
        PositionsInItemLoader.Instance.Load();

        MapTexture.Instance.CreateMapFromTexture();

        ClueManager.Instance.Init();

        Invoke("StartDelay", 0.01f);
    }

    void StartDelay()
    {
        Player.Instance.Init();
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
