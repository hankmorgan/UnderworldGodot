namespace Underworld
{

    public partial class combat : UWClass
    {
        public static int CombatHitTileX;
        public static int CombatHitTileY;
        public enum CombatStages
        {
            Ready = 0,
            Charging = 1,
            Release = 2,
            Swinging = 3,
            Striking = 4,
            Resetting = 5
        }
        public static uwObject currentweapon;  //if null then using fist
        public static int CurrentWeaponRadius = 0;

        public static int OnHitSpell = 0;

        public static int CurrentAttackSwingType = 0;

        public static int currentWeaponItemID
        {
            get
            {
                if (currentweapon == null)
                {
                    return 15; //a_fist
                }
                else
                {
                    return currentweapon.item_id;
                }
            }
        }

        public static CombatStages stage = 0;
        public static double combattimer = 0.0;

        /// <summary>
        /// tracks if a jewelled dagger is being used in order to ensure the listener in the sewers can be killed with it
        /// </summary>
        public static bool JeweledDagger = false;

        /// <summary>
        /// Item ID for Fist object
        /// </summary>
        const int fist = 15;

        public static int WeaponCharge = 0;
        public static int FinalAttackCharge = 0;

        public static int AttackScore = 0;
        public static int AttackDamage = 0;
        public static int AttackScoreFlankingBonus = 0;
        public static bool AttackWasACrit = false;

        public static int BodyPartHit;

        // public static int DefenderIndex;
        static uwObject AttackingCharacter;
        static uwObject DefendingCharacter;
        // public static int AttackerIndex;

        static int AttackHitZ_dseg_67d6_24CE;


        /// <summary>
        /// Table of how strong NPC swings are
        /// </summary>
        public static short[] NPCSwingCharges = new short[] { 0x32, 0x3C, 0x46, 0x50, 0x5A, 0x64, 0x6E, 0x78, 0x82, 0x8C, 0x9B, 0xAA, 0xB9, 0xCD, 0xE6, 0xFF };

        public static short[] BodyHitZ = new short[] { 5, 3, 1, 7, 0 };


    }//end class
}//end namespace