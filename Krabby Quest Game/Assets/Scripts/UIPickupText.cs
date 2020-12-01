using Assets.Components.World;
using System;
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
        World current = World.Current;
        if (current == null) return;
        if (WatchingPickup == "TIME")
        {
            if (current.TryGetPickupInfo("TIME", out var timeLeft))            
                textComponent.text = $"TIME REMAINING: {TimeSpan.FromSeconds(timeLeft.AmountCollected).ToString()}";            
            return;
        }
        if (current.TryGetPickupInfo(WatchingPickup, out var amount))        
            textComponent.text = WatchingPickup + ": " + amount.AmountCollected + " out of " + amount.AmountTotal;        
    }
}
