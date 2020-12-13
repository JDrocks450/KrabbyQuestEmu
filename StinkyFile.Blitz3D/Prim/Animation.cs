﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Prim
{
    public class Animation : BlitzObject
    {
        public class Rep
        {
            public Dictionary<int, Quat> scale_anim { get; }
            public Dictionary<int, Quat> pos_anim { get; } 
            public Dictionary<int, Quat> rot_anim { get; }

            public Rep()
            {
                scale_anim = new Dictionary<int, Quat>();
                pos_anim = new Dictionary<int, Quat>();
                rot_anim = new Dictionary<int, Quat>();
            }

            /// <summary>
            /// C++ signature of function <see cref="SetKey(int, Quat)"/>
            /// </summary>
            /// <param name="time"></param>
            /// <param name="value"></param>
            public void setKey(Dictionary<int, Quat> keys, int time, Quat value) => SetKey(keys, time, value);
            public void SetKey(Dictionary<int, Quat> keys, int time, Quat value)
            {
                if (!keys.ContainsKey(time))
                    keys.Add(time, value);
            }
        }

        public Rep AnimRep { get => rep; set => rep = value; }
        private Rep rep;

        public Animation()
        {
            rep = new Rep();
        }

        void write()
        {

        }

        public void setScaleKey(int time, Vector q) {
            write();
            rep.SetKey(rep.scale_anim, time, new Quat(0, q));
        }

        public void setPositionKey(int time, Vector q)
        {
            write();
            rep.setKey(rep.pos_anim, time, new Quat(0, q));
        }

        public void setRotationKey( int time, Quat q ){
	        write();
	        rep.setKey( rep.rot_anim,time,q );
        }

        public int numScaleKeys() {
	        return rep.scale_anim.Count;
        }

         public int numPositionKeys() {
	        return rep.pos_anim.Count;
        }

         public int numRotationKeys() {
	        return rep.rot_anim.Count;
        }

        public override string ToString()
        {
            return $"(Animation) Keys- P: {numPositionKeys()}, R: {numRotationKeys()}, S: {numScaleKeys()}";
        }
    }
}