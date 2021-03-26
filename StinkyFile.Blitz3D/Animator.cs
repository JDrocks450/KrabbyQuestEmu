using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StinkyFile.Blitz3D.Prim;
using System;
using System.Collections.Generic;
using System.IO;
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
            //seq.frames = frames;
            Frames = frames;
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
        /// Extracts a sequence from an existing <see cref="Animation"/> at the index, applies this sequence to all objects, then returns a <see cref="Seq"/> representing the change.
        /// </summary>
        /// <param name="first">The first keyframe in the span</param>
        /// <param name="last">The last keyframe in the span</param>
        /// <param name="fromSeq">Optionally, you can take from an existing <see cref="Animation"/> sequence, at the index.</param>
        /// <returns></returns>
        public Seq extractSeq(int first, int last, int fromSeq = 0)
        {
            Seq sq = new Seq(first, last, Sequences.Count);
            Sequences.Add(sq);

            for (int k = 0; k < Objects.Count; k++)
            {
                Animation keys = _anims[k].keys[fromSeq];
                _anims[k].keys.Add(new Animation(keys, first, last));
            }
            return sq;
        }
        
        /// <summary>
        /// Gets a filename for the serialized sequences that is within standard spec for KrabbyQuestEmu
        /// </summary>
        /// <param name="BlitzObjectName">The name of the object which the <see cref="Animator"/> is attached.</param>
        /// <param name="B3DFileName">The name of the Blitz3D file the object originated, without directory information (and without extention!)</param>
        /// <returns></returns>
        public static string GetFormattedSerializedFileName(string BlitzObjectName, string B3DFileName)
        {
            return B3DFileName + "_" + BlitzObjectName + ".animseq";
        }

        /// <summary>
        /// Loads the JSON into <see cref="Sequences"/>
        /// </summary>
        /// <param name="source"></param>
        public void Deserialize(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new Exception("The serialized JSON string is empty.");
            JArray array = (JArray)JsonConvert.DeserializeObject(source);
            foreach (var seq in array.Select(x => x.ToObject<Seq>()))            
                extractSeq(seq.First, seq.Last, seq.BasedOnSequence).Name = seq.Name;            
        }

        /// <summary>
        /// Loads from the workspace the sequence information, stored in a *.animseq file.
        /// </summary>
        /// <param name="AnimationWorkspace">The Workspace Directory where Animation definitions are stored.</param>
        /// <param name="ObjectName">The animated object in the Blitz3D file tree which the sequences are tied to</param>
        /// <param name="B3DFileName">The *.b3d fileName (without extention!) which is currently opened</param>
        public void LoadSequencesFromPath(string AnimationWorkspace, string ObjectName, string B3DFileName)
        {
            var path = System.IO.Path.Combine(AnimationWorkspace, Animator.GetFormattedSerializedFileName(ObjectName, B3DFileName));
            var content = File.ReadAllText(path);
            if (string.IsNullOrEmpty(content))
                throw new Exception($"The *.animseq file at the supplied path: {path} must exist, not be empty, and also be formatted correctly in order to be read.");
            Deserialize(content);
        }

        /// <summary>
        /// Gets a JSON string containing the information for each sequence definition in this animator
        /// </summary>
        /// <returns></returns>
        public string SerializeSequences(bool includeRootSeq = false)
        {
            using (var sw = new StringWriter())
            {
                SerializeSequences(sw);
                return sw.ToString();
            }
        }
        /// <summary>
        /// Writes a JSON string containing the information for each sequence definition in this animator using the TextWriter
        /// <para>See: <see cref="GetFormattedSerializedFileName(string, string)"/> for proper naming conventions when saving to file</para>
        /// </summary>
        public void SerializeSequences(TextWriter writer, bool includeRootSeq = false)
        {
            JsonSerializer serializer = new JsonSerializer();

            JArray array = new JArray();
            foreach (var seq in Sequences)
                if (seq.IsRootSeq && !includeRootSeq)
                    continue;
                else
                    array.Add(JToken.FromObject(seq));

            serializer.Serialize(writer, array);
        }

        public Seq GetSequenceDefinition(string Name) => Sequences.Where(x => x.Name == Name).FirstOrDefault();
        public Seq GetSequenceDefinition(int index) => Sequences[index];

        public override string ToString()
        {
            return $"Animations: {_anims.Length}, Seqs: {Sequences.Count}, Frames: {Sequences.FirstOrDefault()?.FrameLength ?? 0} Objects: {Objects.Count}";
        }
    }
    
    [JsonObject]
    /// <summary>
    /// A snippet of the total animation Keyframe timeline. 
    /// <para>Blitz3D stores animations on a per-object basis, using sequences allows us to only play a part of the total animation stored for each object.</para>
    /// </summary>
    public class Seq
    {
        [JsonProperty]
        /// <summary>
        /// Sets which sequence index this one derives from
        /// </summary>
        public int BasedOnSequence
        {
            get; set;
        } = 0;
        [JsonProperty]
        /// <summary>
        /// The identifier for this sequence. 
        /// </summary>
        public int ID
        {
            get; internal set;
        }
        [JsonProperty]
        /// <summary>
        /// Optionally, this can have a name attached to it for easier identification, though it is not in Blitz3D spec
        /// </summary>
        public string Name
        {
            get;set;
        }
        [JsonIgnore]
        /// <summary>
        /// The root seq is the default sequence created by the <see cref="Animator"/> parent.
        /// <para>It is the total animation timeline, start to finish.</para>
        /// </summary>
        public bool IsRootSeq => ID == 0;
        [JsonIgnore]
        /// <summary>
        /// The length of this sequence
        /// </summary>
        public int FrameLength => Last - First;
        [JsonProperty]
        /// <summary>
        /// The first keyframe in the span
        /// </summary>
        public int First { get; set; }
        [JsonProperty]
        /// <summary>
        /// The last keyframe in the span
        /// </summary>
        public int Last { get; set; }

        /// <summary>
        /// Default constuctor
        /// </summary>
        public Seq()
        {

        }

        /// <summary>
        /// Creates a sequence from the first and last keyframes, making a span.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="id"></param>
        public Seq(int first, int last, int id)
        {
            First = first;
            Last = last;
            ID = id;
            if (id == 0)
                Name = "Base Animation";
        }
    }

    /// <summary>
    /// Represents an individual object's animations
    /// </summary>
    public class Anim
    {
        //Parts of the total animation this object contains.
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
