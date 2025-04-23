namespace Underworld
{
    public class an_arrow_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            
            motion.RangedAmmoItemID = (int)trapObj.owner | ((int)trapObj.quality<<5);
            motion.RangedAmmoType = 0x14;
            motion.MissileHeading = 2;
            motion.MissilePitch = 2;
            motion.projectileXHome = triggerX;
            motion.projectileYHome = triggerY;

            motion.MissileLauncherHeadingBase = 0;

            var arrow = motion.PrepareProjectileObject(trapObj);
            if (arrow!=null)
            {
                ObjectCreator.RenderObject(arrow,UWTileMap.current_tilemap);
                //make a sound                
            }


            // ObjectCreator.spawnObjectInTile(
            //     itemid: itemid, 
            //     tileX: triggerX, 
            //     tileY: triggerY, 
            //     xpos: (short)trapObj.tileX, 
            //     ypos: (short)trapObj.tileY, 
            //     zpos: trapObj.zpos, 
            //     WhichList: ObjectFreeLists.ObjectListType.StaticList);
            // Debug.Print("TODO: spawn this arrow trap object as a projectile");
        }
    }//end class
        
}//end namespace
        //owner | (quality<<5)