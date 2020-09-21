using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleImageLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize(); // make sure we are able to load textures at this point
        GetComponent<Renderer>().material.mainTexture = TextureLoader.RequestTexture("Graphics/title.bmp", "0,0,0", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
