using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBehavior : MonoBehaviour
{
    const float SPIKE_TIME_UP = 1f, SPIKE_TIME_DOWN = 2f;
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
            if (currentTime > SPIKE_TIME_DOWN)
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
