using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Text>().text = LevelObjectManager.LoadingPercentage * 100 + "%";
        if (LevelObjectManager.IsDone)
            transform.parent.gameObject.SetActive(false);
    }
}
