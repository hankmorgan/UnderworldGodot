using Godot;

namespace Underworld
{
    public class npc : objectInstance
    {

        /// <summary>
        /// global shader for npcs.
        /// </summary>
        public static Shader textureshader;
       
       /// <summary>
       /// The material for rendering this unique npc
       /// </summary>
        public ShaderMaterial material;


        public npc(uwObject _uwobject)
        {
            uwobject =_uwobject;
            SetAnimSprite(0,0);
        }

        static npc()
        {
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwsprite.gdshader");
        }

        public void SetAnimSprite(int animationNo, int frameNo)
        {
            var crit = CritLoader.GetCritter(this.uwobject.item_id & 0x3F);
            if (material == null)
            {//create the initial material
                var newmaterial = new ShaderMaterial();
                newmaterial.Shader = textureshader;
                //newmaterial.SetShaderParameter("texture_albedo", (Texture)LoadImageAt(textureno,true));
                newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
                newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("UseAlpha", true);
                material = newmaterial;
            }
            //assign the params to the shader
            //critAnim.animSprites[critAnim.animIndices[AnimationIndex, AnimationPos++]]
            var texture = crit.critterinfo.AnimInfo.animSprites[crit.critterinfo.AnimInfo.animIndices[animationNo, frameNo]];
            FrameSize= new Vector2(
                ArtLoader.SpriteScale * texture.GetWidth(), 
                ArtLoader.SpriteScale * texture.GetHeight()
                );
            material.SetShaderParameter("texture_albedo", (Texture)texture);
        }

    }//end class

}//end namespace