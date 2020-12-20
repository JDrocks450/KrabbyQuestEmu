using StinkyFile.Blitz3D.Prim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D
{
    /// <summary>
    /// MeshLoader.cpp
    /// </summary>
    public class MeshLoader
    {
        public IEnumerable<MeshModel> Meshes => _meshes;
        private List<MeshModel> _meshes = new List<MeshModel>();
        public MeshModel CurrentMesh
        {
            get;set;
        }

        public MeshLoader()
        {

        }

        public MeshModel CreateMesh() => CurrentMesh = AddMesh(new MeshModel());

        public MeshModel AddMesh(MeshModel model)
        {
            _meshes.Add(model);
            return model;
        }

        public void AddBone(Pivot bone) => CurrentMesh.AddBone(bone);
    }
}
