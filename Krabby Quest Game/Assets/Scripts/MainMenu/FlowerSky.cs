using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerSky : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var material = Resources.Load("Skyboxes/FlowerSky") as Material;
        material.mainTexture = TextureLoader.RequestTexture("Graphics/cloud.bmp", "0,0,0", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
