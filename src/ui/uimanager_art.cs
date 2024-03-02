using Godot;
using System.Collections.Generic;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        public static GRLoader grCursors;
		public static GRLoader grButtons;
		public static GRLoader grObjects;
		public static GRLoader grLfti;
		public static GRLoader grOptBtns;
		public static GRLoader grConverse;
        public static GRLoader grBody;
		public static GRLoader grArmour_F;
		public static GRLoader grArmour_M;
		public static GRLoader grFlasks;
        public static GRLoader grOptbtn; //main menu buttons
        public static GRLoader grSpells; //spell icons

        public static BytLoader bitmaps;

        public static Dictionary<string, CutsLoader> csCuts;

		public static void InitArt()
		{
            grCursors = new GRLoader(GRLoader.CURSORS_GR, GRLoader.GRShaderMode.UIShader);
            grObjects = new GRLoader(GRLoader.OBJECTS_GR, GRLoader.GRShaderMode.UIShader);
            grObjects.UseRedChannel = true;
            grButtons = new GRLoader(GRLoader.BUTTONS_GR, GRLoader.GRShaderMode.UIShader);
            grLfti = new GRLoader(GRLoader.LFTI_GR, GRLoader.GRShaderMode.UIShader);
            grArmour_F = new GRLoader(GRLoader.ARMOR_F_GR, GRLoader.GRShaderMode.UIShader);
            grArmour_M = new GRLoader(GRLoader.ARMOR_M_GR, GRLoader.GRShaderMode.UIShader);
			grConverse = new GRLoader(GRLoader.CONVERSE_GR, GRLoader.GRShaderMode.UIShader);
            grOptbtn = new  GRLoader(GRLoader.OPBTN_GR, GRLoader.GRShaderMode.UIShader);            
            grOptbtn.PaletteNo = 6;
            grSpells = new  GRLoader(GRLoader.SPELLS_GR, GRLoader.GRShaderMode.UIShader);
            grSpells.UseRedChannel = true;
			bitmaps = new BytLoader();

            csCuts = new Dictionary<string, CutsLoader>();
		}

    }
}