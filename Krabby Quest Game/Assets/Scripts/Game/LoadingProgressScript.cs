using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressScript : MonoBehaviour
{
    GameObject bar;
    Text progress;
    // Start is called before the first frame update
    void Start()
    {
        bar = transform.GetChild(1).GetChild(1).gameObject;
        progress = transform.GetChild(2).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        progress.text = LevelObjectManager.LoadingPercentage.ToString("P");
        float width = LevelObjectManager.LoadingPercentage * 400;
        (bar.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        if (LevelObjectManager.IsDone)
            gameObject.SetActive(false);
    }
}
