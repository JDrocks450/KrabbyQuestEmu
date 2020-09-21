using Assets.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FontApplier : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Text>().font = FontCreator.KrabbyQuestFont;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
