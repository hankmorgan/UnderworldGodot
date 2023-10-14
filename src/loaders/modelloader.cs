using System.Collections.Specialized;
using System.Collections.Generic;
using Godot;

namespace Underworld
{
    // For file research
    public class modelloader : Loader
    {

        enum nodecmd
        {
            M3_UW_ENDNODE = 0x0000,
            M3_UW_SORT_PLANE = 0x0006,
            M3_UW_SORT_PLANE_ZY = 0x000C,
            M3_UW_SORT_PLANE_XY = 0x000E,
            M3_UW_SORT_PLANE_XZ = 0x0010,
            M3_UW_COLOR_DEF = 0x0014, //???
            M3_UW_FACE_UNK16 = 0x0016, //???
            M3_UW_FACE_UNK40 = 0x0040,
            M3_UW_FACE_PLANE = 0x0058,
            M3_UW_FACE_PLANE_ZY = 0x005E,
            M3_UW_FACE_PLANE_XY = 0x0060,
            M3_UW_FACE_PLANE_XZ = 0x0062,
            M3_UW_FACE_PLANE_X = 0x0064,
            M3_UW_FACE_PLANE_Z = 0x0066,
            M3_UW_FACE_PLANE_Y = 0x0068,
            M3_UW_ORIGIN = 0x0078,
            M3_UW_VERTEX = 0x007A,
            M3_UW_FACE_VERTICES = 0x007E,
            M3_UW_VERTICES = 0x0082,
            M3_UW_VERTEX_X = 0x0086,
            M3_UW_VERTEX_Z = 0x0088,
            M3_UW_VERTEX_Y = 0x008A,
            M3_UW_VERTEX_CEIL = 0x008C,
            M3_UW_VERTEX_XZ = 0x0090,
            M3_UW_VERTEX_XY = 0x0092,
            M3_UW_VERTEX_YZ = 0x0094,
            M3_UW_FACE_SHORT = 0x00A0,
            M3_UW_TEXTURE_FACE = 0x00A8,
            M3_UW_TMAP_VERTICES = 0x00B4,
            M3_UW_FACE_SHADE = 0x00BC,
            M3_UW_FACE_TWOSHADES = 0x00BE,
            M3_UW_VERTEX_DARK = 0x00D4,
            M3_UW_FACE_GOURAUD = 0x00D6,
        };

        struct UWModel
        {
            public List<Vector3> verts;//= new List<Vector3>();
            public List<int> tris; //=;// new List<int>();
            public List<float> u;
            public List<float> v;
            public Vector3 origin;
            public string modelname;
            public int NoOfVerts;
        };

        struct ua_model_offset_table
        {
            public long table_offset;
            public long value;          /* 4 bytes at table_offset */
            public long base_offset;
        };

        static string[] ua_model_name =
        {
            "-",//0
            "door frame",//1
            "bridge",//2
            "bench",//3
            "Lotus Turbo Esprit (no, really!)",//4
            "small boulder",//5
            "medium boulder",//6
            "large boulder",//7
            "arrow",//8
            "beam",//9
            "pillar",//10
            "shrine",//11
            "?",//12
            "painting [uw2]",//13
            "?",//14
            "?",//15
            "texture map (8-way lever)",//16
            "texture map (8-way switch)",//17
            "texture map (writing)",//18
            "gravestone",//19
            "texture map (0x016e)",//20
            "-",//21
            "?texture map (0x016f)",//22
            "moongate",//23
            "table",//24
            "chest",//25
            "nightstand",//26
            "barrel",//27
            "chair",//28
            "bed [uw2]",//29
            "blackrock gem [uw2]",//30
            "shelf [uw2]"//31
            };


        static byte[] buffer;
        static ua_model_offset_table[] modeltable;
        static modelloader()
        {
            modeltable = new ua_model_offset_table[6];
            modeltable[0].table_offset = 0x0004e910; modeltable[0].value = 0x40064ab6; modeltable[0].base_offset = 0x0004e99e;
            modeltable[1].table_offset = 0x0004ccd0; modeltable[1].value = 0x40064ab6; modeltable[1].base_offset = 0x0004cd5e;
            modeltable[2].table_offset = 0x0004e370; modeltable[2].value = 0x40064ab6; modeltable[2].base_offset = 0x0004e3fe;
            modeltable[3].table_offset = 0x0004ec70; modeltable[3].value = 0x40064ab6; modeltable[3].base_offset = 0x0004ecfe;
            modeltable[4].table_offset = 0x00054cf0; modeltable[4].value = 0x59aa64d4; modeltable[4].base_offset = 0x00054d8a;
            modeltable[5].table_offset = 0x000550e0; modeltable[5].value = 0x59aa64d4; modeltable[5].base_offset = 0x0005517a;
        }
        public static string DecodeModel(int modelToLoad)
        {
            if (buffer == null)
            {
                var exename = "UW.EXE";
                if (_RES == GAME_UW2)
                {
                    exename = "UW2.EXE";
                }
                var path = System.IO.Path.Combine(BasePath, exename);
                if (!ReadStreamFile(path, out buffer))
                {
                    return "File not loaded";
                }
            } //end load file

            //now decode

            long baseOffset = 0; // models list base address
            long addressptr = 0;

            // search all offsets for model table begin
            for (int i = 0; i <= modeltable.GetUpperBound(0); i++)
            {
                long file_value = getValAtAddress(buffer, modeltable[i].table_offset, 32);
                if (file_value == modeltable[i].value)
                {
                    // found position
                    baseOffset = modeltable[i].base_offset;
                    addressptr = modeltable[i].table_offset;
                    break;
                }
            }
            if (baseOffset == 0)
            {
                //Debug.Log("didn't find models in file\n");
                return "Did not find model list"; // didn't find list
            }

            // read in offsets
            long[] offsets = new long[32];

            for (int j = 0; j <= offsets.GetUpperBound(0); j++)
            {
                offsets[j] = getValAtAddress(buffer, addressptr, 16);//fread16(fd);
                addressptr += 2;
            }

            UWModel[] models = new UWModel[32];

            // parse all models
            for (int n = 0; n <= offsets.GetUpperBound(0); n++)
            {
                addressptr = baseOffset + offsets[n];
                // read header
                long unk1 = getValAtAddress(buffer, addressptr, 32);
                addressptr += 4;
                // extents
                float ex = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;
                float ey = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;
                float ez = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;

                //Vector3 extents = new Vector3((float)ex, (float)ez, (float)ey);
                //ua_vector3d extents(ex,ez,ey);

                // ua_mdl_trace("dumping builtin model %u (%s)\noffset=0x%08x [unk1=0x%04x, extents=(%f,%f,%f) ]\n",
                //  n,ua_model_name[n],base + offsets[n],unk1,ex,ey,ez);

                //ua_model3d_builtin* model = new ua_model3d_builtin;

                models[n].tris = new List<int>();
                models[n].verts = new List<Vector3>();
                models[n].modelname = ua_model_name[n];
                // Debug.Log(models[n].modelname);
                for (int i = 0; i <= 128; i++)
                {
                    models[n].verts.Add(Vector3.Zero);
                }

                // temporary variables
                // ua_vector3d origin;

                //std::vector<ua_vector3d> vertex_list;

                // parse root node
                //ua_model_parse_node(fd, origin, vertex_list, model->triangles, dump);
                if (n == modelToLoad)
                {
                    //writer.WriteLine("Loading model " + ua_model_name[n] + " at " + addressptr);
                    return ua_model_parse_node(buffer, addressptr, ref models[n], true);
                }


            }
            return "no model loaded!";
        }//end decode


        /// <summary>
        /// Converts value at position in buffer into a float
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="addressPtr"></param>
        /// <returns></returns>
        static float ua_mdl_read_fixed(byte[] buffer, long addressPtr)
        {
            short val = (short)getValAtAddress(buffer, addressPtr, 16);
            return ((float)val) / 256f;
        }

        static int ua_mdl_read_vertno(byte[] buffer, long addressPtr)
        {
            int val = (int)DataLoader.getValAtAddress(buffer, addressPtr, 16);
            return val / 8;
        }

        static void ua_mdl_store_vertex(Vector3 vertex, int vertno, ref UWModel mod)
        {
            if (vertno >= mod.verts.Capacity)
            {
                mod.verts.Capacity = vertno + 1;
            }
            mod.verts[vertno] = vertex;
            if (mod.NoOfVerts < vertno)
            {
                mod.NoOfVerts = vertno;
            }
            //writer.WriteLine("\tStoring Vertex " + vertno + " at " + vertex);
        }

        //Ported from underworld adventures
        static string ua_model_parse_node(byte[] buffer, long addressptr, ref UWModel mod, bool dump)
        {
            string result = "";
            // parse node until end node
            bool loop = true;
            int instr = -1;
            while (loop)
            {
                // read next command
                int cmd = (int)(getValAtAddress(buffer, addressptr, 16)); addressptr += 2;
                instr++;
                switch ((nodecmd)cmd)
                {
                    // misc. nodes
                    case nodecmd.M3_UW_ENDNODE: // 0000 end node
                        {
                            result += $"\nInstr {instr}\tEnd Node";
                            loop = false;
                            break;
                        }
                    case nodecmd.M3_UW_ORIGIN: // 0078 define model center
                        {
                            //writer.WriteLine("\nInstr " + instr + " origin");
                            result += $"\nInstr {instr}\tDefine Model Centre";
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            mod.origin = mod.verts[vertno];

                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;
                            result += $"\tVertNo:{vertno} x:{vx} y:{vy} z:{vz} unk:{unk1}";
                            // ua_mdl_trace("[origin] vertno=%u unk1=%04x origin=(%f,%f,%f)",
                            //  vertno,unk1,vx,vy,vz);
                            break;
                        }


                    // vertex definition nodes
                    case nodecmd.M3_UW_VERTEX: // 007a define initial vertex
                        {
                            result += $"\nInstr {instr}\tDefine Initial Vertex";
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;//ua_mdl_read_vertno(fd);
                            Vector3 refvect = new Vector3((float)vx, (float)vy, (float)vz);

                            result += $"\tx:{vx} y:{vy} z:{vz} Vertno:{vertno}";
                            ua_mdl_store_vertex(refvect, vertno, ref mod);
                            //Debug.Log("\nInstr " + instr + "Vertex #" + vertno + "="  + refvect);
                            // ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f)",
                            // vertno,vx,vy,vz);
                            break;
                        }


                    case nodecmd.M3_UW_VERTICES: // 0082 define initial vertices
                        {
                            // writer.WriteLine("\nInstr " + instr + " M3_UW_VERTICES");
                            result += $"\nInstr {instr}\tDefine Initial Vertices";
                            int nvert = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            int vertno = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            for (int n = 0; n < nvert; n++)
                            {
                                float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                                float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                                float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);

                                Vector3 refvect = new Vector3((float)vx, (float)vy, (float)vz);
                                ua_mdl_store_vertex(refvect, vertno + n, ref mod);
                                result += $"\n\tx:{vx} y:{vy} z:{vz} StartingAt:{vertno} NoOfVerts {nvert}";
                                //Debug.Log("\nInstr " + instr + "Vertex #" + vertno + "="  + refvect);
                                //ua_mdl_trace("%s[vertex] vertno=%u vertex=(%f,%f,%f)",
                                // n==0 ? "" : "\n      ",vertno+n,vx,vy,vz);
                            }
                        }
                        break;

                    case nodecmd.M3_UW_VERTEX_X: // 0086 define vertex offset X
                        {
                            // writer.WriteLine("\nInstr " + instr + " offsetX");
                            result += $"\nInstr {instr}\tVertex Offset X";
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            result += $"\n\trefvert:{refvert} x:{vx} vertno:{vertno}";
                            Vector3 refvect = mod.verts[refvert];
                            Vector3 adj = new Vector3(vx, 0f, 0f);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            //// Debug.Log("Vertex offsetX #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            //ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) x from=%u",
                            //  vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }

                    case nodecmd.M3_UW_VERTEX_Z: // 0088 define vertex offset Z
                        {
                            //writer.WriteLine("\nInstr " + instr + " offsetZ");
                            result += $"\nInstr {instr}\tVertex Offset Z";
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            Vector3 adj = new Vector3(0f, 0f, vz);
                            result += $"\n\trefvert:{refvert} z:{vz} vertno:{vertno}";
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            // Debug.Log("Vertex offsetZ #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            //ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) z from=%u",
                            //  vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_Y: // 008a define vertex offset Y
                        {
                            //writer.WriteLine("\nInstr " + instr + " offsetY");
                            result += $"\nInstr {instr}\tVertex Offset Z";
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            //refvect.y += vy;
                            //refvect  = new Vector3(refvect.x,refvect.y+(float)vy,refvect.z);
                            Vector3 adj = new Vector3(0f, vy, 0f);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            result += $"\n\trefvert:{refvert} y:{vy} vertno:{vertno}";
                            // Debug.Log("Vertex offsetY #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            // ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) y from=%u",
                            //   vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_XZ: // 0090 define vertex offset X,Z
                        {
                            //writer.WriteLine("\nInstr " + instr + " offsetXZ");
                            result += $"\nInstr {instr}\tVertex Offset XZ";
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            //refvect.x += vx;
                            //refvect.z += vz;
                            // refvect  = new Vector3(refvect.x+(float)vx,refvect.y,refvect.z+(float)vz);
                            Vector3 adj = new Vector3(vx, 0f, vz);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            result += $"\n\trefvert:{refvert} x:{vx} z:{vz} vertno:{vertno}";
                            // Debug.Log("Vertex offsetXZ #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            // ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) xz from=%u",
                            //    vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_XY: // 0092 define vertex offset X,Y
                        {
                            result += $"\nInstr {instr}\tVertex Offset XY";
                            //writer.WriteLine("\nInstr " + instr + " offsetXY");
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            // refvect.x += vx;
                            //  refvect.y += vy;
                            //refvect  = new Vector3(refvect.x+(float)vx,refvect.y+(float)vy,refvect.z);
                            // ua_mdl_store_vertex(refvect,vertno,ref mod);
                            Vector3 adj = new Vector3(vx, vy, 0f);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            result += $"\n\trefvert:{refvert} x:{vx} y:{vy} vertno:{vertno}";
                            // Debug.Log("Vertex offsetXY #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            // ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) xy from=%u",
                            //   vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_YZ: // 0094 define vertex offset Y,Z
                        {
                            //writer.WriteLine("\nInstr " + instr + " offsetYZ");
                            result += $"\nInstr {instr}\tVertex Offset YZ";
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            //refvect.y += vy;
                            // refvect.z += vz;
                            //refvect  = new Vector3(refvect.x,refvect.y+(float)vy,refvect.z+(float)vz);
                            // ua_mdl_store_vertex(refvect,vertno,ref mod);
                            Vector3 adj = new Vector3(0f, vy, vz);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            result += $"\n\trefvert:{refvert} y:{vy} z:{vz} vertno:{vertno}";
                            // Debug.Log("Vertex offsetYZ #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);

                            // ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,%f) yz from=%u",
                            //    vertno,refvect.x,refvect.y,refvect.z,refvert);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_CEIL: // 008c define vertex variable height
                        {
                            // writer.WriteLine("\nInstr " + instr + " UW_VERTEX_CEIL");
                            result += $"\nInstr {instr}\tVertex Variable Height of Ceiling";
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                            Vector3 refvect = mod.verts[refvert];
                            // refvect.z = 32.0f; // todo: ceiling value
                            //refvect  = new Vector3(refvect.x,refvect.y,32f);
                            Vector3 adj = new Vector3(0f, 0f, 32f);
                            ua_mdl_store_vertex(refvect + adj, vertno, ref mod);
                            result += $"\n\trefvert:{refvert} unk1:{unk1} vertno:{vertno}";
                            //  writer.WriteLine("\tVertex Ceil #" +(vertno) + "="  + (refvect+adj) + " from " + refvect + "(" + refvert + ")" + " adj = " +adj);
                            //  ua_mdl_trace("[vertex] vertno=%u vertex=(%f,%f,ceil) ceil from=%u unk1=%04x",
                            //   vertno,refvect.x,refvect.y,refvert,unk1);
                            break;
                        }

                    // face plane checks
                    case nodecmd.M3_UW_FACE_PLANE: // 0058 define face plane, arbitrary heading
                        {
                            //writer.WriteLine("\nInstr " + instr + " UW_FACE_PLANE");
                            result += $"\nInstr {instr}\tFace Plane";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            float nx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float ny = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float nz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            //writer.Write("\t Normal (" + nx + "," + ny + "," + nz + ") dist (" + vx + "," + vy + "," + vz + ")\n");
                            result += $"\n\tunk1:{unk1} normalXYZ:{nx},{ny},{nz} vertexXYZ:{vx},{vy},{vz}";

                            //  ua_mdl_trace("[planecheck] skip=%04x normal=(%f,%f,%f) dist=(%f,%f,%f)",
                            //    unk1,nx,ny,nz,vx,vy,vz);
                            break;
                        }


                    case nodecmd.M3_UW_FACE_PLANE_X: // 0064 define face plane X
                        {
                            result += $"\nInstr {instr}\tFace Plane X";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            float n = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float v = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormal:{n} Vertex:{v} unk1:{unk1}";
                            break;
                        }
                    case nodecmd.M3_UW_FACE_PLANE_Z: // 0066 define face plane Z
                        {
                            result += $"\nInstr {instr}\tFace Plane Z";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            float n = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float v = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormal:{n} Vertex:{v} unk1:{unk1}";
                            break;
                        }
                    case nodecmd.M3_UW_FACE_PLANE_Y: // 0068 define face plane Y
                        {
                            result += $"\nInstr {instr}\tFace Plane Y";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            float n = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float v = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormal:{n} Vertex:{v} unk1:{unk1}";
                            break;
                        }


                    case nodecmd.M3_UW_FACE_PLANE_ZY: // 005e define face plane Z/Y
                        {
                            result += $"\nInstr {instr}\tFace Plane ZY";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            float nz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;// ua_mdl_read_fixed(fd);
                            float ny = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormalZY:{nz},{ny} VertexZY:{vz},{vy} unk1{unk1}";
                            break;
                        }
                    case nodecmd.M3_UW_FACE_PLANE_XY: // 0060 define face plane X/Y
                        {
                            result += $"\nInstr {instr}\tFace Plane ZY";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            float nx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;// ua_mdl_read_fixed(fd);
                            float ny = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormalZY:{nx},{ny} VertexZY:{vx},{vy} unk1{unk1}";
                            break;
                        }
                    case nodecmd.M3_UW_FACE_PLANE_XZ: // 0062 define face plane X/Z
                        {
                            result += $"\nInstr {instr}\tFace Plane XZ";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            float nx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;// ua_mdl_read_fixed(fd);
                            float nz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            result += $"\n\tNormalZY:{nx},{nz} VertexZY:{vx},{vz} unk1{unk1}";
                            break;
                        }

                    // face info nodes
                    case nodecmd.M3_UW_FACE_VERTICES: // 007e define face vertices
                        {
                            //writer.WriteLine("\nInstr " + instr + " UW_FACE_VERTICES");
                            result += $"\nInstr {instr}\tFace Vertices";
                            int nvert = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int[] faceverts = new int[nvert];
                            for (int i = 0; i < nvert; i++)
                            {
                                // Uint16 
                                int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;

                                // mod.tris.Add(vertno);//moved
                                faceverts[i] = vertno;
                                //Xua_vertex3d vert;
                                //output = output + vertno + ",";
                                result += $"\n\tVertNo{vertno}";
                            }
                            convertVertToTris(faceverts, ref mod);
                        }
                        break;

                    case nodecmd.M3_UW_TEXTURE_FACE: // 00a8 define texture-mapped face
                    case nodecmd.M3_UW_TMAP_VERTICES: // 00b4 define face vertices with u,v information
                        {
                            result += $"\nInstr {instr}\tUW_TEXTURE_FACE or UW_TMAP_VERTICES";
                            // ua_mdl_trace("[face] %s ",cmd==M3_UW_TEXTURE_FACE ? "tex" : "tmap");
                            //string output = "\tFace Verts are :";
                            // read texture number
                            if ((nodecmd)cmd == nodecmd.M3_UW_TEXTURE_FACE)
                            {
                                int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd); // texture number?
                                result += $"\n\tunk1{unk1}";
                            }

                            int nvert = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int[] faceverts = new int[nvert];
                            for (int i = 0; i < nvert; i++)
                            {
                                // Uint16 
                                int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                                float u0 = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                                float v0 = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                                faceverts[i] = vertno;
                                result += $"\n\tVertNo{vertno} UV:{u0},{v0}";
                            }
                            convertVertToTris(faceverts, ref mod);
                        }
                        break;

                    // sort nodes
                    case nodecmd.M3_UW_SORT_PLANE: // 0006 define sort node, arbitrary heading
                    case nodecmd.M3_UW_SORT_PLANE_ZY: // 000C define sort node, ZY plane
                    case nodecmd.M3_UW_SORT_PLANE_XY: // 000E define sort node, XY plane
                    case nodecmd.M3_UW_SORT_PLANE_XZ: // 0010 define sort node, XZ plane
                        {
                            //writer.WriteLine("\nInstr " + instr + " SORT PLANES");
                            result += $"\nInstr {instr}\tSORT PLANES";
                            if ((nodecmd)(cmd) == nodecmd.M3_UW_SORT_PLANE)
                            {
                                float nx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                                float vx = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            }

                            float ny = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vy = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float nz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);
                            float vz = ua_mdl_read_fixed(buffer, addressptr); addressptr += 2;//ua_mdl_read_fixed(fd);

                            long left = getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            left += addressptr;// ftell(fd);

                            long right = getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            right += addressptr;// ftell(fd);

                            long here = addressptr;//ftell(fd);

                            // parse left nodes
                            //fseek(fd,left,SEEK_SET);
                            addressptr = here;
                            ua_model_parse_node(buffer, addressptr, ref mod, dump);
                            //ua_model_parse_node(fd,origin,vertex_list,triangles,dump);

                            // ua_mdl_trace("      [sort] end left node/start right node\n");

                            // parse right nodes
                            //  fseek(fd,right,SEEK_SET);
                            addressptr = right;
                            ua_model_parse_node(buffer, addressptr, ref mod, dump);
                            //ua_model_parse_node(fd,origin,vertex_list,triangles,dump);

                            // return to "here"
                            //  fseek(fd,here,SEEK_SET);
                            addressptr = here;

                            // ua_mdl_trace("      [sort] end");
                        }
                        break;

                    // unknown nodes
                    case nodecmd.M3_UW_COLOR_DEF: // 0014 ??? colour definition
                        {
                            result += $"\nInstr {instr}\tUW_COLOUR_DEF";
                            int refvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            result += $"\n\trefvert:{refvert} unk1:{unk1} vertno:{vertno}";
                            break;
                        }


                    case nodecmd.M3_UW_FACE_SHADE: // 00BC define face shade
                        {
                            result += $"\nInstr {instr}\tUW_FACE_SHADE";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int vertno = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            result += $"\n\tunk1:{unk1} vertno:{vertno}";                                                                     //ua_mdl_trace("[shade] shade unk1=%02x unk2=%02x",unk1,vertno);
                            break;
                        }


                    case nodecmd.M3_UW_FACE_TWOSHADES: // 00BE ??? seems to define 2 shades
                        {
                            result += $"\nInstr {instr}\tFACE_TWOSHADE";
                            int vertno = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            result += $"\n\tunk1:{unk1} vertno:{vertno}";                                                                        //ua_mdl_trace("[shade] twoshade unk1=%02x unk2=%02x ",vertno,unk1);
                            break;
                        }


                    case nodecmd.M3_UW_VERTEX_DARK: // 00D4 define dark vertex face (?)
                        {
                            result += $"\nInstr {instr}\tVERTEX DARK";
                            int nvert = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;// fread16(fd);
                            result += $"\n\tunk1:{unk1} nvert{nvert}";
                            for (int n = 0; n < nvert; n++)
                            {
                                int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                                // unk1 = fgetc(fd);
                                addressptr++;
                                result += $"\n\tvertno:{vertno}";
                                // ua_mdl_trace("%u (%02x) ",vertno,unk1);
                            }
                            if ((nvert & 1) == 1)
                            {
                                // fgetc(fd); // read alignment padding
                                addressptr++;
                            }
                        }
                        break;

                    case nodecmd.M3_UW_FACE_GOURAUD: // 00D6 define gouraud shading
                                                     // ua_mdl_trace("[shade] gouraud");
                        result += $"\nInstr {instr}\tFACE_GOURAUD";
                        break;

                    case nodecmd.M3_UW_FACE_UNK40: // 0040 ???
                        result += $"\nInstr {instr}\tFACE_UNK40";
                        // ua_mdl_trace("[shade] unknown");
                        break;

                    case nodecmd.M3_UW_FACE_SHORT: // 00A0 ??? shorthand face definition
                        {
                            result += $"\nInstr {instr}\tFACE_SHORT";
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            result += $"\n\tinitial_vertno:{vertno}";
                            //ua_mdl_trace("[face] shorthand unk1=%u vertlist=",vertno);
                            int[] faceverts = new int[4];
                            for (int i = 0; i < 4; i++)
                            {
                                vertno = (int)getValAtAddress(buffer, addressptr, 8); addressptr++; //fgetc(fd);
                                result += $"\n\tvertno:{vertno}";
                                faceverts[i] = vertno;
                            }
                            convertVertToTris(faceverts, ref mod);
                        }
                        break;

                    case (nodecmd)0x00d2: // 00D2 ??? shorthand face definition
                        {
                            result += $"\nInstr {instr}\tSHORTHAND_FACE_DEFINITION";
                            int vertno = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;
                            result += $"\n\tinitialvertno:{vertno}";
                            //ua_mdl_trace("[face] vertno=%u vertlist=",vertno);
                            int[] faceverts = new int[4];
                            for (int i = 0; i < 4; i++)
                            {
                                vertno = (int)getValAtAddress(buffer, addressptr, 8); addressptr++; //fgetc(fd);
                                result += $"\n\tvertno:{vertno}";
                                faceverts[i] = vertno;
                            }
                            convertVertToTris(faceverts, ref mod);
                        }
                        break;

                    case nodecmd.M3_UW_FACE_UNK16: // 0016 ???
                        {
                            //writer.WriteLine("\nInstr " + instr + " UW_FACE_UNK16");
                            result += $"\nInstr {instr}\tFace_UNK16";
                            long pos = addressptr;//(int)getValAtAddress(buffer,addressptr,16);addressptr++;//ftell(fd);

                            int nvert = ua_mdl_read_vertno(buffer, addressptr); addressptr += 2;//ua_mdl_read_vertno(fd);
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            result += $"\n\tnvert:{nvert} unk1:{unk1}";
                            for (int n = 0; n < nvert; n++)
                            {
                                // unk1 = fgetc(fd);
                                var unk2 = (int)getValAtAddress(buffer, addressptr, 8); addressptr++;
                                result += $"\n\tunk2:{unk2}";
                            }

                            if ((nvert & 1) == 1)
                            {    //fgetc(fd); // read alignment padding
                                addressptr++;
                            }
                        }
                        break;

                    case (nodecmd)0x0012:
                        {
                            // writer.WriteLine("\nInstr " + instr + " UNK 12");
                            result += $"\nInstr {instr}\tUNK12";
                            int unk1 = (int)getValAtAddress(buffer, addressptr, 16); addressptr += 2;//fread16(fd);
                            result += $"\n\tunk1:{unk1}";
                            break;
                        }

                    default:
                        //ua_mdl_trace("unknown command at offset 0x%08x\n",ftell(fd)-2);
                        result += $"\nInstr {instr}\tUNKNOWN CMD returning";
                        return result;
                }
                // ua_mdl_trace("\n");
            }
            return result;
        }//end parse node


        static void convertVertToTris(int[] vertices, ref UWModel mod)
        {//This is sort of wrong.

        }
    }//end class

}//end namespace