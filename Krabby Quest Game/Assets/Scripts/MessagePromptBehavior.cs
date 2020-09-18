using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePromptBehavior : MonoBehaviour
{
    static GameObject ScreenDim;
    static Text TextComponent;
    // Start is called before the first frame update
    void Start()
    {
        ScreenDim = gameObject;
        ScreenDim.SetActive(false);
        TextComponent = transform.GetChild(1).GetComponent<Text>();
    }

    public static void ShowMessage(string text)
    {
        TextComponent.text = text;
        ScreenDim.SetActive(true);
    }

    public static void HideMessage()
    {
         ScreenDim.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
