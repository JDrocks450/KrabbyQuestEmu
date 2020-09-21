using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        var transform = this.transform as RectTransform;
        var screenTransform = GameObject.Find("Canvas").transform as RectTransform;
        //scale map
        var part = transform.rect.height;
        var whole = screenTransform.rect.height;
        var ratio = whole / part;
        transform.localScale = new Vector3(ratio, ratio, 1);
        //apply textures
        for (int i = 1; i < 5; i++)
        {
            var image = transform.GetChild(i).GetComponent<Image>();
            var rectT = ((RectTransform)image.gameObject.transform).rect;
            var sprite = Sprite.Create(TextureLoader.RequestTexture($"Graphics/map{i}.bmp", "0,0,0", true),
                new Rect(0,512 - rectT.height, rectT.width, rectT.height), new Vector2());
            image.sprite = sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
