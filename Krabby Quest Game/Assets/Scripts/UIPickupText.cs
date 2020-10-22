using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPickupText : MonoBehaviour
{
    public string WatchingPickup;
    Text textComponent;
    // Start is called before the first frame update
    void Start()
    {
        textComponent = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Pickup.MajorPickups.TryGetValue(WatchingPickup, out var amount))        
            textComponent.text = WatchingPickup + ": " + amount.amountCollected + " out of " + amount.amountTotal;        
    }
}
