using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpikeBehavior : MonoBehaviour
{
    const float SPIKE_TIME_UP = 1f;

    float[] SpikeTimes =
    {
        2f,
        2f,
        2f,
        7f
    };
    int sproutType = 0;
    float currentTime;
    public bool Up
    {
        get; private set;
    } = true;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        var block = GetComponentInParent<DataBlockComponent>().DataBlock;
        var number = block.Name.Last().ToString();
        sproutType = int.Parse(number);
    }

    // Update is called once per frame
    void Update()
    {
        if (Up)
        {
            if (currentTime > SPIKE_TIME_UP)
            {
                animator.Play("SpikeDown");
                Up = false;
                currentTime = 0;
            }
            else currentTime += Time.deltaTime;
        }
        else
        {
            if (currentTime > SpikeTimes[sproutType - 1]) 
            {
                animator.Play("SpikeUp");
                //GetComponent<SoundLoader>().Play(0);
                Up = true;
                currentTime = 0;
            }
            else currentTime += Time.deltaTime;
        }
    }
}
