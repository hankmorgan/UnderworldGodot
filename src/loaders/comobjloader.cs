using System.IO;
namespace Underworld
{
    /// <summary>
    /// Class to load and access values in comboj.dat (common object data)
    /// </summary>
    public  class commonObjDat:Loader
    {
        static byte[] buffer;

        /// <summary>
        /// Height of the object. Used for collision calculations.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int height(int item_id)
        { // value at + 0
            return buffer[2 + (item_id * 11) + 0];
        }

        /// <summary>
        /// Radius of the object. Used for collision calculations.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int radius(int item_id)
        {   //bits 0..2 at + 1
            var x = getAt(buffer, 2 + (item_id*11) + 1, 16);
            return (int)(x & 7);
        }


        /// <summary>
        /// The mass of the object in 0.1 stones
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int mass(int item_id)
        {   //bits 4-15 at + 1
            var x = getAt(buffer, 2 + (item_id*11) + 1, 16);
            return (int)(x>>4 & 0xFFF);
        }


        /// <summary>
        /// If true the object can be picked up
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool CanBePickedUp(int item_id)
        {//bit 5 at + 3
            return ((buffer[2 + item_id * 11 + 3] >>5) & 0x1)==1;
        }

        /// <summary>
        /// Returns 0,1 or 3 usually
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int stackable(int item_id)
        {
            return ((buffer[2 + item_id * 11 + 3]>>6) & 0x3);
        }

        /// <summary>
        /// The base trade value of a single qty of this object.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int monetaryvalue(int item_id)
        {   //int16 at + 4
            return (int)getAt(buffer, 2 + item_id*11 + 4, 16);
        }



        /// <summary>
        /// This object can activate switches when it collides with an object.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int ActivatedByCollision(int item_id)
        {//bits 2,3 at + 6
            return (buffer[2 + item_id * 11 + 6]>>1 ) & 0x1;
        }


        /// <summary>
        /// "Quality class bits 2,3
        /// Cannot be removed in projectile motion when =3
        /// Also door strength, 3=invulnerable, other values shift the damage value left"
        /// quality class (this value*6+quality gives index into string block 5 (TO CONFIRM THIS MATH)
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int qualityclass(int item_id)
        {//bits 2,3 at + 6
            return (buffer[2 + item_id * 11 + 6]>>2 ) & 0x3;
        }

        public static bool canhaveowner(int item_id)
        { //bit 7 at +7
            return (buffer[2 + item_id * 11 + 7] >> 7 & 0x1)  == 1;
        }

        /// <summary>
        /// bits 1-4  (checked when object hits the ground, 
        /// only set when object is a damaging projectile type so this is probability that
        /// the missile is removed from the world on impact when Rng (0-7) is less than the value. 
        /// Any value >=8 means the projectile is destroyed on impact, 
        /// 0 means object is not removed on impact.
        /// If the mobile object has bits 4,5,6 at offset 0xA set to 1 
        ///  then the projectile will be removed regardless of the below value and a splash spawned. (landing in water)
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int projectileflag(int item_id)
        { //bits 1-4 at + 7
            return buffer[2 + item_id * 11 + 7] >> 1  & 0xf;
        }

        /// <summary>
        /// Probably used for scaling damage and vulnerabilities
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int scaleresistances(int item_id)
        {
            return buffer[2 + item_id * 11 + 8];
        }

        public static int rendertype(int item_id)
        {
            return buffer[2 + item_id * 11 + 0x9] & 0x3;
        }

        /// <summary>
        /// Each possible value of "quality type" maps onto a group of 6 strings in
        /// block 4 (???? should this be 5????) from lowest to highest quality. 
        /// Items which have a quality are always described as "a/an <quality> <item>", 
        ///  with the exception of group D which is for armour items which are grammatically plural 
        ///  even if there is only one of the object, e.g. "leather leggings".
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int qualitytype(int item_id)
        {
            return buffer[2 + item_id * 11 + 0xA] & 0xF;
        }

        public static bool PrintableLook(int item_id)
        {
            return ((buffer[2 + item_id * 11 + 0xA] >> 4) & 1) == 1;
        }

        static commonObjDat()
        {
            ReadStreamFile(Path.Combine(BasePath,"DATA","COMOBJ.DAT"), out buffer);
        }
    } //end class

}//end namespace