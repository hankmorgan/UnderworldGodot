using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Underworld
{

    /// <summary>
    /// Class for rendering 3d Model objects
    /// </summary>
    public class model3D : objectInstance
    {
        protected const int CEILING_HEIGHT = 32;
        public Material material;
        public static Shader textureshader;
        
        /// <summary>
        /// Generates the defined 3d model and adds as a child to the parent node.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Node3D Generate3DModel(Node3D parent)
        {
            int[] mats = new int[NoOfMeshes()];
            var a_mesh = new ArrayMesh(); //= Mesh as ArrayMesh;
            var verts = ModelVertices();
            Vector2[] uvs = ModelUVs(verts);
            int MeshCount = NoOfMeshes();
            
            for (int i = 0; i < MeshCount; i++)
            {  
                mats[i] = ModelColour(i); //index into the appropiate palette(default) or material list
            }

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            for (int i=0; i<MeshCount;i++)
            {
                AddSurfaceToMesh(this, verts, uvs, mats, i, a_mesh, normals, ModelTriangles(i));
            }

            return CreateMeshInstance(parent, $"modelinstance_{uwobject.index}",  a_mesh);
        }

        public virtual int[] ModelTriangles(int meshNo)
        {
            return new int[] { 0, 0, 0 };
        }

        public virtual Vector3[] ModelVertices()
        {
            return new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
        }

        public virtual int NoOfMeshes()
        {
            return 1;
        }

        /// <summary>
        /// This is the indices of the texture or colour palette to render with.
        /// </summary>
        /// <param name="meshNo"></param>
        /// <returns></returns>
        public virtual int ModelColour(int meshNo)
        {
            return 127; // this colour will standout.
            //return Color.Color8(0, 0, 0, 0);  //.white;
        }

        public virtual bool isSolidModel()
        {
            return true;
        }

        public virtual Vector2[] ModelUVs(Vector3[] verts)
        {//This probably gives bad mappings
            Vector2[] customUVs = new Vector2[verts.Length];
            for (int i = 0; i < customUVs.Length; i++)
            {
                customUVs[i] = new Vector2(verts[i].X, verts[i].Y);
                customUVs[i] = customUVs[i] * TextureScaling();
            }
            return customUVs;
        }

        // public virtual Material ModelMaterials(int meshNo)
        // {
        //     return material;
        // }


        // public virtual Texture2D ModelTexture(int meshNo)
        // {
        //     return null;
        // }

        public virtual float TextureScaling()
        {
            return 1f;
        }

        protected static Node3D CreateMeshInstance(Node3D parent,string ModelName, ArrayMesh a_mesh, bool EnableCollision = false)
        {
            var final_mesh = new MeshInstance3D();
            parent.AddChild(final_mesh);
            final_mesh.Position = Vector3.Zero; // new Vector3(x * -1.2f, 0.0f, y * 1.2f);
            final_mesh.Name = ModelName;
            final_mesh.Mesh = a_mesh;
            if (EnableCollision)
            {
                final_mesh.CreateConvexCollision();
            }
            return final_mesh;
        }


        /// <summary>
        /// Adds a surface built from the various uv, vertices and materials arrays to a mesh
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="uvs"></param>
        /// <param name="MatsToUse"></param>
        /// <param name="FaceCounter"></param>
        /// <param name="a_mesh"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        protected static void AddSurfaceToMesh(model3D instance, Vector3[] verts, Vector2[] uvs, int[] MatsToUse, int FaceCounter, ArrayMesh a_mesh, List<Vector3> normals, int[] indices, int faceCounterAdj = 0)
        {
            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);
            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();
            //Add the new surface to the mesh
            a_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            a_mesh.SurfaceSetMaterial(FaceCounter + faceCounterAdj, instance.GetMaterial(MatsToUse[FaceCounter], FaceCounter)); //  surfacematerial.Get(MatsToUse[FaceCounter]));

        }


        /// <summary>
        /// Get the material to display this 3d object with. Defaults to a flat colour from the palette.
        /// Override this to use textures from tmobj instead and replace the texture_albedo as needed.
        /// </summary>
        /// <param name="textureno"></param>
        /// <returns></returns>
        public  virtual ShaderMaterial GetMaterial(int textureno, int surface)
        {
            if (textureshader==null)
            {
                textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwshader.gdshader");
            }
            var newmaterial = new ShaderMaterial();
            newmaterial.Shader = textureshader;
            newmaterial.SetShaderParameter("texture_albedo", (Texture)Palette.IndexToImage((byte)textureno));
            newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
            newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
            newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
            newmaterial.SetShaderParameter("UseAlpha", false);
            return newmaterial;
        }



        


    }//end class
}//end namespace