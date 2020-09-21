using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var material = GetComponent<Terrain>().materialTemplate; //var material = Instantiate((Material)Resources.Load("Materials/Terrain Material"));
        material.mainTexture = TextureLoader.RequestTexture("Graphics/floorbeach.jpg", null, true);
        //material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
