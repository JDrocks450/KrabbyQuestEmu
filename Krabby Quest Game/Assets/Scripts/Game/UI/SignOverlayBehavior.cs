using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SignOverlayBehavior : MonoBehaviour
{
    TextMesh messageText;
    Animator Controller;
    bool elementsHidden = true;
    object signOpenSender = null;

    // Start is called before the first frame update
    void Start()
    {
        messageText = transform.GetChild(0).gameObject.GetComponent<TextMesh>();
        transform.GetChild(0).gameObject.SetActive(false); // hide the visible portion of the sign overlay
        transform.GetChild(1).gameObject.SetActive(false); // hide the visible portion of the sign overlay
        Controller = GetComponent<Animator>();
    }

    public static void ShowSignMessage(object sender, string Message)
    {
        var signObject = GameObject.FindGameObjectsWithTag("SignOverlay").FirstOrDefault();
        signObject.GetComponent<SignOverlayBehavior>().ShowMessage(sender, Message);
    }
    public static void CloseSignMessage(object sender)
    {
        var signObject = GameObject.FindGameObjectsWithTag("SignOverlay").FirstOrDefault();
        signObject.GetComponent<SignOverlayBehavior>().CloseMessage(sender);
    }

    private void CloseMessage(object sender)
    {
        if (sender != signOpenSender)
            return;
        Controller.Play("CloseOverlay");
    }

    private void ShowMessage(object sender, string message)
    {
        messageText.text = message;
        if (elementsHidden)
        {
            transform.GetChild(0).gameObject.SetActive(true); // hide the visible portion of the sign overlay
            transform.GetChild(1).gameObject.SetActive(true); // show the visible portion of the sign overlay
            elementsHidden = false;
        }
        Controller.Play("OpenOverlay");
        signOpenSender = sender;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
