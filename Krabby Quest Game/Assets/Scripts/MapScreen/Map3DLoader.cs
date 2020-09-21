using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map3DLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameInitialization.Initialize();
        var transform = this.transform;
        var objectMaterial = Resources.Load("Materials/Object Material") as Material;
        //apply textures
        for (int i = 0; i < 4; i++)
        {
            var image = transform.GetChild(i).GetComponent<Renderer>();
            var sprite = TextureLoader.RequestTexture($"Graphics/map{4-i}.bmp", null, true);
            image.material = Instantiate(objectMaterial);
            image.material.mainTexture = sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
