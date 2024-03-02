using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("SpellIcons")]
        [Export] public TextureRect[] ActiveSpellIcons = new TextureRect[3];


        //Based on a table of values in the .exe files used to match icons and strings
        static int[] SpellIconOffsetsUW1 = new int[] { 0x0E, -1, 0x0D, 0x04, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x0B, 0x80, 0x80, 0x80, 0x80 };
        static int[] SpellIconOffsetsUW2 = new int[] { 0x14, -1, 0x13, 0x05, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x11, 0x80, 0x80, 0x80, 0x80 };
        static void InitSpellIcons()
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                var offset = new Vector2(-140, 28);
                for (int i = 0; i <= instance.ActiveSpellIcons.GetUpperBound(0); i++)
                {
                    instance.ActiveSpellIcons[i].Position += offset;
                    EnableDisable(instance.ActiveSpellIcons[i], false);
                }
            }

        }


        /// <summary>
        /// Sets the icon that controls active spells
        /// </summary>
        /// <param name="index"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        public static void SetSpellIcon(int index, int major, int minor)
        {
            var spellno = GetSpellNoIndex(major, minor);
            if (spellno>= 0x80)
            {//out of range value/not applicable
                ClearSpellIcon(index);
            }
            else
            {
                instance.ActiveSpellIcons[index].Texture = grSpells.LoadImageAt(spellno);
                instance.ActiveSpellIcons[index].Material = grSpells.GetMaterial(spellno);
                EnableDisable(instance.ActiveSpellIcons[index], true);
            }
        }


        //Removes the active spell icon
        public static void ClearSpellIcon(int index)
        {
            instance.ActiveSpellIcons[index].Texture = null;
            instance.ActiveSpellIcons[index].Material = null;
            EnableDisable(instance.ActiveSpellIcons[index], false);
        }


        /// <summary>
        /// Uses a lookup table taken from the exe to major and minor spell class to get correct art no and offset into strings
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <returns></returns>
        static int GetSpellNoIndex(int major, int minor)
        {
            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    return SpellIconOffsetsUW2[major] + minor;
                default:
                    return SpellIconOffsetsUW1[major] + minor;
            }
        }

        private void _on_spell_icon_gui_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {
                var i = (int)extra_arg_0;
                if (eventMouseButton.ButtonIndex == MouseButton.Left)
                {
                    //left click. cancel spell
                    playerdat.CancelEffect(i);
                    playerdat.PlayerStatusUpdate();
                }
                else
                {//get spell description                    
                    var stability = playerdat.GetEffectStability(i);
                    var effectclass = playerdat.GetEffectClass(i);
                    var major = effectclass & 0xF;
                    var minor = effectclass >> 4;
                    var index = GetSpellNoIndex(major, minor);
                    var spellname = GameStrings.GetString(6, 384 + index);
                    int stabilityresult = 137;
                    if (UWClass._RES == UWClass.GAME_UW2)
                    {
                        stabilityresult = 151;
                    }
                    if (stability < 2)
                    {
                        stabilityresult += 0;
                    }
                    else
                    {
                        if (stability <= 0xA)
                        {
                            stabilityresult += 1;
                        }
                        else
                        {
                            stabilityresult += 2;
                        }
                    }
                    var stabilitymsg = GameStrings.GetString(1, stabilityresult);
                    uimanager.AddToMessageScroll($"{spellname}{stabilitymsg}");
                }
            }
        }
    }//end class
}//end namespace