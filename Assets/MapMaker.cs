using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    // map scale
    public int width = 50;
    public int height = 50;
    public TMP_InputField inputField_Width;
    public TMP_InputField inputField_Height;

    Texture2D texture;
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        UpdateMapScale();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMapScale() {
        width = int.Parse(inputField_Width.text);
        height = int.Parse(inputField_Height.text);
        UpdateMapScale();
    }

    public void UpdateMapScale() {
        texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                texture.SetPixel(x, y, Color.blue);
            }
        }

        var rect = new Rect(0, 0, width, height);
        image.sprite = Sprite.Create(texture, rect, Vector2.zero);
        image.rectTransform.sizeDelta = new Vector2(width, height);
        inputField_Height.text = "" + height;
        inputField_Width.text = "" + width;
    }
}
