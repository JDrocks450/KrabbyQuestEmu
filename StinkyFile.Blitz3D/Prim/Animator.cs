using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Prim
{
    public class Animator
    {
        public int Frames { get; private set; }
        public List<BlitzObject> Objects { get; private set; } = new List<BlitzObject>();
        public IEnumerable<Anim> Animations => _anims; 
        private Anim[] _anims = new Anim[0];
        internal List<Seq> Sequences = new List<Seq>();


        public Animator(BlitzObject parent, int frames)
        {
            addObjs(parent);
            Array.Resize(ref _anims, Objects.Count);
            addSeq(frames);
        }

        public Animator(List<BlitzObject> objs, int frames)
        {
            Objects = objs;
            Array.Resize(ref _anims, Objects.Count);
            addSeq(frames);
        }

        void addObjs(BlitzObject Object)
        {
            Objects.Add(Object);
            foreach(var obj in Object.Children)
            {
                Objects.Add(obj); // ugly casts! wait thats not in this one...
            }
        }

        void addSeq(int frames)
        {
            Seq seq = new Seq();
            seq.frames = frames;
            Sequences.Add(seq);
            for (int k = 0; k < Objects.Count; ++k)
            {
                BlitzObject obj = Objects[k];
                _anims[k] = new Anim();
                _anims[k].keys.Add(obj.Animation);
                obj.setAnimation(null);
            }
        }

        public override string ToString()
        {
            return $"Animations: {_anims.Length}, Seqs: {Sequences.Count}, Objects: {Objects.Count}";
        }
    }

    /// <summary>
    /// A class to hold one variable. It's here because it wants to be ok?
    /// </summary>
    internal class Seq
    {
        internal int frames;
    }

    public class Anim
    {
        //Animation instances that are keys for each animation this object contains
		public List<Animation> keys = new List<Animation>();
		//Transitions
		bool pos,scl,rot;
		Vector src_pos,dest_pos;
		Vector src_scl,dest_scl;
		Quat src_rot,dest_rot;

        public Anim()
        {
            pos = false;
            scl = false;
            rot = false;
        }
    }
}
