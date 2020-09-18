using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    bool shiftHolding = false, canUpdate = true;
    Vector3 mouseHoldPosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!canUpdate)
        {
            Destroy(this);
            return;
        }
        if (Input.GetMouseButtonDown(1))
            mouseHoldPosition = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            var change = ((mouseHoldPosition - Input.mousePosition) / 10) * Time.deltaTime;
            var camTransform = transform;
            camTransform.position =
                new Vector3(camTransform.position.x + change.x,
                            camTransform.position.y,
                            camTransform.position.z + change.y);
        }
        else
        {
            try
            {
                var player = Player.Current?.transform ?? null;
                if (player == null)
                    return;
                transform.position =
                        new Vector3(player.position.x,
                                    player.position.y + 8,
                                    player.position.z + 6);
            }
            catch
            {
                //canUpdate = false;
            }
        }
    }
}
