using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBehavior : MonoBehaviour
{
    const float LONG_THROW_TIME = 5.0f;
    bool animating = false;
    
    DataBlockComponent BlockComponent;
    Object GooberPrefab;
    AnimationLoader Animations;

    float timeSinceLastThrow = 0f, throwTime = LONG_THROW_TIME;

    // Start is called before the first frame update
    void Start()
    {
        BlockComponent = GetComponent<DataBlockComponent>();
        GooberPrefab = Resources.Load("Objects/Goober");
        Animations = GetComponent<AnimationLoader>();
        if(Animations == null)
            Animations = GetComponentInChildren<AnimationLoader>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!LevelObjectManager.IsDone) return;
#if KQT_ANIMS
        if (!animating)
        {
            Animations.PlayAnimationSequence("CannonIdle");
            animating = true;
        }
        if (timeSinceLastThrow > throwTime-.9f)
            Animations.PlayAnimationSequence("CannonThrow"); // throwing animation is offset by about a second
#endif
        if (timeSinceLastThrow > throwTime)
        { // throw goober
            Throw();
        }
        else timeSinceLastThrow += Time.deltaTime;
    }

    public void Throw()
    {
        var position = new Vector2Int(BlockComponent.WorldTileX, BlockComponent.WorldTileY);
        var goober = (GameObject)Instantiate(GooberPrefab);
        goober.GetComponent<GooberBehavior>().StartProjectile(position, BlockComponent.DataBlock.Rotation);        
        //Animations.EnqueueSequence("CannonIdle");
        timeSinceLastThrow = 0f;
    }
}
