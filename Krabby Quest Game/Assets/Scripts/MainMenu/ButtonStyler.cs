using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStyler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        Style(GetComponent<Button>());
    }

    public static void Style(Button button)
    {
        var texture = TextureLoader.RequestTexture("Graphics/menubar.jpg", null, true);
        var menuBack = Sprite.Create(texture, new Rect(0,0,texture.width, texture.height), new Vector2());        
        //button.image.sprite = menuBack;
        button.spriteState = new SpriteState()
        {
            highlightedSprite = menuBack,
            pressedSprite = menuBack,
            selectedSprite = menuBack
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
