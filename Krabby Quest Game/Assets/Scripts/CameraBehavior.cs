using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    bool shiftHolding = false, canUpdate = true, camTransitioning = false, levelintroOverride = true, movingQuickly = false;
    Vector3 mouseHoldPosition, transitionStart, currentVelocity, lastCamDesiredPos;
    float transitionPercentage = 0f, camFollowRange = .005f;

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
        var transform = this.transform;
        Transform player = null;
        if (Player.Current != null)
        {
            player = Player.Current.transform;
        }         
        var camDesiredPos = transform.position;
        if (player != null)
            camDesiredPos = new Vector3(player.position.x,
                                    player.position.y + 10,
                                    player.position.z + 7);
        if (!LevelObjectManager.IsDone)
        {
            transform.position = camDesiredPos;
            return;
        }
        if (Input.GetMouseButtonDown(1))
            mouseHoldPosition = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            camTransitioning = false;
            var change = ((mouseHoldPosition - Input.mousePosition) / 10) * Time.deltaTime;
            var camTransform = transform;
            camTransform.position =
                new Vector3(camTransform.position.x + change.x,
                            camTransform.position.y,
                            camTransform.position.z + change.y);
        }
        else if (!camTransitioning)
        {
            try
            {     
                if (player == null)
                    return;                           
                if (Mathf.Abs(transform.position.x - camDesiredPos.x) > 1 ||
                    Mathf.Abs(transform.position.z - camDesiredPos.z) > 1)
                {
                    camTransitioning = true;
                    transitionStart = transform.position;
                    transitionPercentage = 0f;
                }    
                else
                    transform.position = camDesiredPos;                        
            }
            catch
            {
                //canUpdate = false;
            }
        }
        if (camTransitioning)
        {
            if (lastCamDesiredPos != camDesiredPos && !levelintroOverride && !movingQuickly)
            {
                movingQuickly = true;
            }
            else levelintroOverride = false;            
            transform.position = Vector3.SmoothDamp(transform.position, camDesiredPos, ref currentVelocity, movingQuickly ? .1f : .55f);
            if (Mathf.Abs(transform.position.x - camDesiredPos.x) <= camFollowRange ||
                Mathf.Abs(transform.position.z - camDesiredPos.z) <= camFollowRange)
            {
                camTransitioning = false;
                movingQuickly = false;
            }
        }
        lastCamDesiredPos = camDesiredPos;
    }
}
