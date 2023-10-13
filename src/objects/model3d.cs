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
        public Material material;
        public static Shader textureshader;

        public Node3D Generate3DModel(Node3D parent)
        {
            // BoxCollider box = this.GetComponent<BoxCollider>();
            // if (box != null)
            // {
            //     DestroyImmediate(box);
            // }
            //MeshFilter meshF = parent.AddComponent<MeshFilter>();
            //MeshRenderer mr = parent.AddComponent<MeshRenderer>();
            int[] mats = new int[NoOfMeshes()];
            // Mesh mesh = new Mesh
            // {
            //     subMeshCount = NoOfMeshes(),
            //     vertices = ModelVertices()
            // };

            var a_mesh = new ArrayMesh(); //= Mesh as ArrayMesh;
            var verts = ModelVertices();
            Vector2[] uvs = ModelUVs(verts);

            
            for (int i = 0; i < NoOfMeshes(); i++)
            {                           
               
               // mesh.SetTriangles(ModelTriangles(i), i);
                mats[i] =  ModelColour(i);
                
                  // ModelMaterials(i);
                //mr.material.SetColor("_Color",ModelColour(0));
                //mats[i].SetColor("_Color", ModelColour(i));
            }
            // if (uvs.GetUpperBound(0) > 0)
            // {
            //     mesh.uv = uvs;
            // }
            //mr.materials = mats;


            // for (int i = 0; i < NoOfMeshes(); i++)
            // {
            //     mr.materials[i].SetColor("_Color", ModelColour(i));
            // }
            //meshF.mesh = mesh;
            //mesh.RecalculateNormals();

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            for (int i=0; i<NoOfMeshes();i++)
            {
                AddSurfaceToMesh(verts, uvs, mats, i, a_mesh, normals, ModelTriangles(i));
            }

            return CreateMeshInstance(parent, "test",  a_mesh);

            //mesh.RecalculateBounds();
            // if (isSolidModel())
            // {
            //     MeshCollider mc = parent.AddComponent<MeshCollider>();
            //     mc.convex = true;
            //     mc.sharedMesh = null;
            //     mc.sharedMesh = mesh;
            // }
            // else
            // {
            //     Rigidbody rgd = this.GetComponent<Rigidbody>();
            //     if (rgd != null)
            //     {
            //         DestroyImmediate(rgd);
            //     }
            // }
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

        public virtual int ModelColour(int meshNo)
        {
            return 0;
            //return Color.Color8(0, 0, 0, 0);  //.white;
        }

        public virtual bool isSolidModel()
        {
            return true;
        }

        public virtual Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] customUVs = new Vector2[verts.Length];
            for (int i = 0; i < customUVs.Length; i++)
            {
                customUVs[i] = new Vector2(verts[i].X, verts[i].Z);
                customUVs[i] = customUVs[i] * TextureScaling();
            }
            return customUVs;
        }

        public virtual Material ModelMaterials(int meshNo)
        {
            return material;
        }


        public virtual Texture2D ModelTexture(int meshNo)
        {
            return null;
        }

        public virtual float TextureScaling()
        {
            return 1f;
        }


        private Node3D CreateMeshInstance(Node3D parent,string ModelName, ArrayMesh a_mesh, bool EnableCollision = false)
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
        private void AddSurfaceToMesh(Vector3[] verts, Vector2[] uvs, int[] MatsToUse, int FaceCounter, ArrayMesh a_mesh, List<Vector3> normals, int[] indices, int faceCounterAdj = 0)
        {
            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            //Add the new surface to the mesh
            a_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

            //bool useCustomShader=true;
           // if (useCustomShader)
           // {
            a_mesh.SurfaceSetMaterial(FaceCounter + faceCounterAdj, GetMaterial(MatsToUse[FaceCounter])); //  surfacematerial.Get(MatsToUse[FaceCounter]));
           // }
            // else
            // { //standard material shader. this works but does not cycle the textures.
            //     var material = new StandardMaterial3D(); // or shader 
            //     material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            //     material.AlbedoTexture = MatsToUse[FaceCounter];  //textureForMesh; // shader parameter, etc.
            //     material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;            
            //     a_mesh.SurfaceSetMaterial(FaceCounter + faceCounterAdj, material);
            // }
        }

        public virtual ShaderMaterial GetMaterial(int textureno)
        {
            if (textureshader==null)
            {
                textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwshader.gdshader");
            }
          //  if (materials[textureno] == null)
            //{
                //materials[textureno] = new surfacematerial(textureno);
                //create this material and add it to the list
                var newmaterial = new ShaderMaterial();
                newmaterial.Shader = textureshader;
                newmaterial.SetShaderParameter("texture_albedo", (Texture)Palette.IndexToImage((byte)textureno));
                newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
                newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("UseAlpha", false);
                return newmaterial;
                //materials[textureno] = newmaterial;

           // }
           // return materials[textureno];    
        }


    }//end class
}//end namespace