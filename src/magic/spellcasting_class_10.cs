namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void CastClass10_ManaBoost(int minorclass)
        {
            playerdat.ManaRegenChange(minorclass);
        }    
    }
}