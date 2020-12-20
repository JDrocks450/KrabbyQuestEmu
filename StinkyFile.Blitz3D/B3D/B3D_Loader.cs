using StinkyFile.Blitz3D.Prim;
using StinkyFile.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.B3D
{
	/// <summary>
	/// A translation of loader_b3d.h of Blitz3D open source that reads info used to recreate the model elsewhere. 
	/// </summary>
	public class B3D_Loader
	{
		public FileRoot RootObject
		{
			get; set;
		}

		FileStream streamIn;
		List<long> chunk_stack = new List<long>();

		public List<BlitzObject> LoadedObjects = new List<BlitzObject>();
		public MeshLoader MeshLoader = new MeshLoader();

		//static List<Texture> textures;
		//static List<Brush> brushes;		

		bool collapse;
		bool animonly;

		void fclose()
		{
			streamIn.Dispose();
		}

		static long swap_endian(uint n) {
			return ((n & 0xff) << 24) | ((n & 0xff00) << 8) | ((n & 0xff0000) >> 8) | ((n & 0xff000000) >> 24);
		}

		void clear() {
			//brushes.Clear();
			//textures.Clear();
			chunk_stack.Clear();
		}

		/// <summary>
		/// Attempts to read the header
		/// </summary>
		/// <returns></returns>
		string readChunk() {
			int len = 0;
			string head = "";
			try
			{
				head = FileTools.ReadString(streamIn, 4);
				len = FileTools.ReadInt(streamIn);
			}
			catch
			{
				return null;
			}
			chunk_stack.Add(streamIn.Position + len);
			return head;
		}

		/// <summary>
		/// Leaves the current chunk
		/// </summary>
		void exitChunk() {
			streamIn.Seek(chunk_stack.Last(), SeekOrigin.Begin);
			chunk_stack.RemoveAt(chunk_stack.Count - 1); // vector::pop_back
		}

		static int ftell(FileStream fs) => (int)fs.Position;

		int chunkSize() {
			return (int)chunk_stack.Last() - ftell(streamIn);
		}

		void skip(int n) => streamIn.Seek(n, SeekOrigin.Current);

		int readInt() => FileTools.ReadInt(streamIn);

		int[] readIntArray(int n) {
			int[] buffer = new int[n];
			for (int i = 0; i < n; i++)
				buffer[i] = readInt();
			return buffer;
		}

		float readFloat() => FileTools.ReadFloat(streamIn);

		float[] readFloatArray(int n) {
			float[] buffer = new float[n];
			for (int i = 0; i < n; i++)
				buffer[i] = readFloat();
			return buffer;
		}

		uint readColor() {
			float r = readFloat(); if (r < 0) r = 0; else if (r > 1) r = 1;
			float g = readFloat(); if (g < 0) g = 0; else if (g > 1) g = 1;
			float b = readFloat(); if (b < 0) b = 0; else if (b > 1) b = 1;
			float a = readFloat(); if (a < 0) a = 0; else if (a > 1) a = 1;
			return (uint)(((int)(a * 255) << 24) | ((int)(r * 255) << 16) | ((int)(g * 255) << 8) | (int)(b * 255));
		}

		string readString() {
			string t = "";
			for (; ; ) {
				char c;
				byte[] buffer = new byte[1];
				try
				{
					streamIn.Read(buffer, 0, 1);					
					c = (char)buffer[0];
					if (c == '\0') return t;				
				}
				catch
				{
					return t;
				}
				t += c;
			}
		}

		void readKeys(ref Animation anim) {
			if (anim == null) anim = new Animation();
			int flags = readInt();
			while (chunkSize() > 0) {
				int frame = readInt();
				if ((flags & 1) != 0) {
					float[] pos = null;
					pos = readFloatArray(3);
					anim.setPositionKey(frame, new Vector(pos[0], pos[1], pos[2]));
				}
				if ((flags & 2) != 0) {
					float[] scl = null;
					scl = readFloatArray(3);
					anim.setScaleKey(frame, new Vector(scl[0], scl[1], scl[2]));
				}
				if ((flags & 4) != 0) {
					float[] rot;
					rot = readFloatArray(4);
					anim.setRotationKey(frame, new Quat(rot[0], new Vector(rot[1], rot[2], rot[3])));
				}
			}
		}

		private Pivot readBone()
		{
			Pivot bone = new Pivot();

			MeshLoader.AddBone(bone);

			while (chunkSize() > 0)
			{
				int vert = readInt();
				float weight = readFloat();
				bone.Vertex = vert;
				bone.Weight = weight;
			}
			return bone;
		}

		BlitzObject readObject(BlitzObject parent)
		{
			BlitzObject obj = null;

			string name = readString();
			float[] pos, scl, rot;
			pos = readFloatArray(3);
			scl = readFloatArray(3);
			rot = readFloatArray(4);

			Animation keys = new Animation();
			int anim_len = 0;
			MeshModel mesh = null;
			int mesh_flags, mesh_brush;

			while (chunkSize() > 0) {
				switch (readChunk()) {
					case "MESH":
						obj = mesh = MeshLoader.CreateMesh();
						mesh_brush = readInt();
						//mesh_flags=readMesh();
						break;
					case "BONE":
						obj = readBone();
						break;
					case "KEYS":
						readKeys(ref keys);
						break;
					case "ANIM":
						readInt();
						anim_len = readInt();
						readFloat();
						break;
					case "NODE":
						if (obj == null) obj = new MeshModel();
						readObject(obj);
						break;
				}
				exitChunk();
			}

			if (obj == null) obj = new MeshModel();

			obj.Name = name;
			obj.LocalPosition = (new Vector(pos[0], pos[1], pos[2]));
			obj.LocalScale = (new Vector(scl[0], scl[1], scl[2]));
			obj.LocalRotation = (new Quat(rot[0], new Vector(rot[1], rot[2], rot[3])));
			obj.setAnimation(keys);

			if (mesh != null) {
				//MeshLoader::endMesh(mesh);
				//if (!(mesh_flags & 1)) mesh->updateNormals();
				//if (mesh_brush != -1) mesh->setBrush(brushes[mesh_brush]);
			}

			if (mesh != null && mesh.Bones.Any()) {
				mesh.Bones.Insert(0, mesh);
				mesh.setAnimator(new Animator(mesh.Bones, anim_len));
			} else if (anim_len > 0) {
				obj.setAnimator(new Animator(obj, anim_len));
			}
			if (parent != null) obj.Parent = (parent);
			LoadedObjects.Add(obj);
			return obj;
		}		

		public static B3D_Loader Load(string filePath)
		{
			var b3d = new B3D_Loader();
			b3d.RootObject = b3d.LoadB3D(filePath);
			return b3d;
		}

		public FileRoot LoadB3D(string filePath)//, Transform conv,int hint )
		{
			//collapse = !!(hint & MeshLoader::HINT_COLLAPSE);
			//animonly = !!(hint & MeshLoader::HINT_ANIMONLY);

			streamIn = File.OpenRead(filePath);
			clear();
			RootObject = new FileRoot()
			{
				Name = "FILE_ROOT",
			};

			string tag = readChunk();
			if (tag != "BB3D") { //BB3D
				fclose();
				return null;
			}

			int version = readInt();
			if (version > 1) {
				fclose();
				return null;
			}

			BlitzObject obj = null;
			while (chunkSize() > 0) {
				string chunkHead = readChunk();
				switch (chunkHead) {
					case "TEXS":
						//readTextures();
						break;
					case "BRUS":
						//readBrushes();
						break;
					case "NODE":
						obj = readObject(RootObject);
						break;
				}
				exitChunk();
			}

			fclose();
			clear();

			LoadedObjects.Insert(0, RootObject);

			return RootObject;//?.getModel().getMeshModel();
		}
	}
}
