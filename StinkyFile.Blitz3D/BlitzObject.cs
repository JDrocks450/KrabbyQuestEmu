using StinkyFile.Blitz3D.Prim;
using StinkyFile.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D
{
    /// <summary>
    /// The base class for Blitz3D components
    /// <para><c>Blitz3D Object.h</c></para>
    /// </summary>
    public class BlitzObject
    {
        private BlitzObject _parent;

        [EditorVisible()]
        public string Name { get; set; }
        [EditorVisible()]
        public Vector LocalPosition { get; set; }
        [EditorVisible()]
        public Vector LocalScale { get; set; }
        [EditorVisible()]
        public Quat LocalRotation { get; set; }
        [EditorVisible()]
        public BlitzObject Parent { 
            get => _parent; 
            set
            {
                _parent = value;
                value.Children.Add(this);
            }
        }
        [EditorVisible()]
        public List<BlitzObject> Children { get; private set; } = new List<BlitzObject>();
        //[EditorVisible()]
        /// <summary>
        /// This is transferred to the object's animator during loading, if you want this object's animations, see: <see cref="Animator"/>
        /// </summary>
        public Animation Animation { get; private set; }

        public bool HasAnimator => Animator != null;

        [EditorVisible()]
        /// <summary>
        /// The object's animator, handles animations for this object.
        /// </summary>
        public Animator Animator
        {
            get; set;
        }        

        public BlitzObject()
        {

        }

        public BlitzObject(BlitzObject Parent) : this()
        {
            this.Parent = Parent;
        }

        public void setAnimation(Animation keys) => Animation = keys;

        public void setAnimator(Animator animator) => Animator = animator;

        public override string ToString()
        {
            return Name;
        }
    }
}
