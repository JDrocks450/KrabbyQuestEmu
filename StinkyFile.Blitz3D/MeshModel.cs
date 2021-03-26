using StinkyFile.Blitz3D.Prim;
using StinkyFile.Primitive;
using System.Collections.Generic;
using System.Linq;

namespace StinkyFile.Blitz3D
{
    public class MeshModel : BlitzObject
    {
        [EditorVisible()]
        public List<BlitzObject> Bones { get; internal set; } = new List<BlitzObject>();        

        public MeshModel()
        {

        }

        public Pivot GetBoneByVertex(int vertex) => Bones.OfType<Pivot>().FirstOrDefault(x => x.Vertex == vertex);
        public bool TryGetBoneByVertex(int vertex, out Pivot Bone)
        {
            Bone = GetBoneByVertex(vertex);
            return Bone != null;
        } 

        public void AddBone(Pivot bone, int index = -1)
        {
            if (index != -1)
            {
                Bones.Insert(index, bone);
                return;
            }
            Bones.Add(bone);
        }
    }
}
