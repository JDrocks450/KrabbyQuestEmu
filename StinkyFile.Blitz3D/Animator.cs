using StinkyFile.Blitz3D.Prim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D
{
    public class Animator
    {
        /// <summary>
        /// The amount of frames per <see cref="Anim"/>
        /// </summary>
        public int Frames { get; private set; }
        /// <summary>
        /// Each <see cref="BlitzObject"/> included in this Animator.
        /// </summary>
        public List<BlitzObject> Objects { get; private set; } = new List<BlitzObject>();
        /// <summary>
        /// The <see cref="Anim"/> instances in this animator, corresponding to each <see cref="BlitzObject"/> in <see cref="Objects"/>
        /// </summary>
        public IEnumerable<Anim> Animations => _anims; 
        /// <summary>
        /// Creates a dictionary of objects and their corresponding Anim instance. 
        /// <para>This should be used as a helper function, as all of this data can be found using <see cref="Animations"/> and <see cref="Objects"/></para>
        /// <para>Note: Extra <see cref="Anim"/> instances without an object attached are ignored.</para>
        /// </summary>
        public Dictionary<BlitzObject, Anim> ObjectAnimDictionary
        {
            get
            {
                var dic = new Dictionary<BlitzObject, Anim>();
                int index = 0;
                foreach (var obj in Objects)
                {
                    dic.Add(obj, Animations.ElementAtOrDefault(index));
                    index++;
                }
                return dic;
            }
        }
        private Anim[] _anims = new Anim[0];
        public List<Seq> Sequences { get; set; } = new List<Seq>();


        public Animator(BlitzObject parent, int frames)
        {
            addObjs(parent);
            Array.Resize(ref _anims, Objects.Count);
            addSeq(frames);
        }

        public Animator(IEnumerable<BlitzObject> objs, int frames)
        {
            Objects = objs.ToList();
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
            Seq seq = new Seq(0,frames,Sequences.Count);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public Seq extractSeq(int first, int last, int fromSeq)
        {
            Seq sq = new Seq(first, last, Sequences.Count + 1);
            Sequences.Add(sq);

            for (int k = 0; k < Objects.Count; k++)
            {
                Animation keys = _anims[k].keys[fromSeq];
                _anims[k].keys.Add(new Animation(keys, first, last));
            }
            return sq;
        }

        public override string ToString()
        {
            return $"Animations: {_anims.Length}, Seqs: {Sequences.Count}, Frames: {Sequences.FirstOrDefault()?.frames ?? 0} Objects: {Objects.Count}";
        }
    }

    /// <summary>
    /// A snippet of the total animation Keyframe timeline. 
    /// <para>Blitz3D stores animations on a per-object basis, using sequences allows us to only play a part of the total animation stored for each object.</para>
    /// </summary>
    public class Seq
    {
        /// <summary>
        /// The identifier for this sequence. 
        /// </summary>
        public int ID
        {
            get; internal set;
        }
        /// <summary>
        /// The root seq is the default sequence created by the <see cref="Animator"/> parent.
        /// <para>It is the total animation timeline, start to finish.</para>
        /// </summary>
        public bool IsRootSeq => ID == 0;
        /// <summary>
        /// The length of this sequence
        /// </summary>
        public int FrameLength
        {
            get => frames;
            set => frames = value;
        }
        public int First { get; }
        public int Last { get; }

        internal int frames;
        public Seq(int first, int last, int id)
        {
            First = first;
            Last = last;
            FrameLength = Last - First;
            ID = id;
        }
    }

    /// <summary>
    /// An animation on an individual object from <see cref="Objects"/>
    /// </summary>
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
