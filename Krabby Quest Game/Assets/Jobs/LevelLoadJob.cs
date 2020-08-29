using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Jobs
{
    public struct LevelLoadJob : IJob
    {
        public GameObject Obj;
        public void Execute()
        {
            var gObject = (GameObject)UnityEngine.Object.Instantiate(Obj);
        }
        public static JobHandle Begin(LevelLoadJob Job = default, JobHandle DependsOn = default) => Job.Schedule(DependsOn);
    }    
}
