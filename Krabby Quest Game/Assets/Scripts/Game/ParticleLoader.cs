using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLoader : MonoBehaviour
{
    public bool AutoLoad = true, LookIntoParent = false;
    public ParticleSystem Target;
    // Start is called before the first frame update
    void Start()
    {
        var BlockComponent = GetComponent<DataBlockComponent>();
        if (LookIntoParent)
            BlockComponent = transform.parent.gameObject.GetComponent<DataBlockComponent>();
        if (BlockComponent == null) return;
        var particleObject = gameObject;
        Target = particleObject.GetComponent<ParticleSystem>();
        var tBurst = Target.emission.GetBurst(0);
        if (BlockComponent.DataBlock == null) return;
        if (BlockComponent.DataBlock.GetParameterByName("ParticleMaterial", out var param))
        {
            var material = particleObject.GetComponent<ParticleSystemRenderer>().material;
            if (!material.name.StartsWith(param.Value))
            {
                var nMaterial = (Material)Resources.Load("Particles/" + param.Value);
                if (nMaterial != default)
                    particleObject.GetComponent<ParticleSystemRenderer>().material = nMaterial;
                else
                    Debug.LogWarning($"The GameObject: {gameObject.name} is asking for ParticleMaterial: {param.Value} which does not exist. Using default material...");
            }
        }
        if (BlockComponent.DataBlock.GetParameterByName<int>("Particle_BurstCount", out var tParam))
        {
            var nBurst = new ParticleSystem.Burst(tBurst.time, tParam.Value);
            Target.emission.SetBurst(0, nBurst);
        }
    }

    public static void SetBurst(ParticleSystem Target, int Count)
    {
        var tBurst = Target.emission.GetBurst(0);
        var nBurst = new ParticleSystem.Burst(tBurst.time, Count);
        Target.emission.SetBurst(0, nBurst);
    }

    public void Play()
    {
        if (Target != default)
            Target.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
