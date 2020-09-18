using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonColorAssignment : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string matName = "ButtonGreen";
        var blockData = GetComponent<DataBlockComponent>();
        if (!blockData.DataBlock.GetParameterByName("Button Color", out var parameter))
            return;
        var color = Color.green;
        switch (parameter.Value)
        {
            case "GREEN":
                matName = "ButtonGreen";
                break;
            case "BLUE":
                matName = "ButtonBlue";
                color = Color.blue;
                break;
            case "AQUA":
                matName = "ButtonCyan";
                color = Color.cyan;
                break;
            case "YELLOW":
                matName = "ButtonYellow";
                color = Color.yellow;
                break;
            case "RED":
                matName = "ButtonRed";
                color = Color.red;
                break;
            case "PURPLE":
                matName = "ButtonPurple";
                color = Color.magenta;
                break;
            case "WHITE":
                matName = "ButtonWhite";
                color = Color.white;
                break;
        }
        Material material = (Material)Resources.Load("Materials/Button Materials/" + matName);
        transform.GetChild(1).GetComponent<Renderer>().material = material; //assign color to button Pushable   
        GetComponentInChildren<Light>().color = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
