using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("SpellIcons")]
        [Export] public TextureRect[] ActiveSpellIcons = new TextureRect[3];

        static void InitSpellIcons()
        {
            if (UWClass._RES==UWClass.GAME_UW2)
            {
                var offset = new Vector2(-140,28);
                for (int i=0;i<=instance.ActiveSpellIcons.GetUpperBound(0);i++)
                {
                    instance.ActiveSpellIcons[i].Position+=offset;
                    EnableDisable(instance.ActiveSpellIcons[i],false); 
                }
            }
            
        }

        public static void SetSpellIcon(int index,int major, int minor)
        {
            var spellno = GetSpellNoArt(major,minor);
            instance.ActiveSpellIcons[index].Texture = grSpells.LoadImageAt(spellno);
            instance.ActiveSpellIcons[index].Material = grSpells.GetMaterial(spellno);   
            EnableDisable(instance.ActiveSpellIcons[index],true);         
        }

        public static void ClearSpellIcon(int index)
        {
            instance.ActiveSpellIcons[index].Texture = null;
            instance.ActiveSpellIcons[index].Material = null;    
            EnableDisable(instance.ActiveSpellIcons[index],false);      
        }


        /// <summary>
        /// Not obvious logic to how spell icons are matched. Hardcode for now
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <returns></returns>
        static int GetSpellNoArt(int major,int minor)
        {
            switch(major)
            {
                case 0: //lights
                    {
                        switch (minor)
                        {
                            case 3: return 17;//torch icon
                        }
                    }
                    break;
            }

            return 0;//placeholder
        }

    }//end clss
}//end namespace