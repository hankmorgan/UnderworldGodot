
using System;
using System.Collections;
using Godot;
using Peaky.Coroutines;

namespace Underworld
{
    /// <summary>
    /// Specific code for the ethereal void (UW1)
    /// </summary>
    public class Etherealvoid : UWClass
    {

        /// <summary>
        /// Codes runs on a random chance every no of frames
        /// Causes light flashes and drains player health during the finale of UW1.
        /// </summary>
        public static void EtherealVoidEndGameSpecialEffects()
        {

            uimanager.FlashColour(0xB5, uimanager.Cuts3DWin, 0.05f);

            if (playerdat.play_hp > 0x64)
            {
                playerdat.play_hp -= (byte)Rng.r.Next(6);
            }
            else
            {
                if (playerdat.play_hp > 0x32)
                {
                    playerdat.play_hp -= (byte)Rng.r.Next(4);
                }
                else
                {
                    if (
                        (playerdat.play_hp > 0x14)
                        &&
                        (Rng.r.Next(3) < 1)
                        )
                    {
                        playerdat.play_hp -= (byte)Rng.r.Next(3);
                    }
                    else
                    {
                        if (playerdat.play_hp > 1)
                        {
                            if ((Rng.r.Next(0x7FFF) & 7) == 0)
                            {
                                playerdat.play_hp -= 1;
                            }
                        }
                    }
                }
            }

            //shake screen
            motion.SetScreenShake(TypeOfShake: 0x40, duration: (byte)(0xF + Rng.r.Next(0x1E)));

            //randomise compass
            uimanager.PointCompassInDirection(Rng.r.Next(0x7FFF) & 0xF);
        }

        /// <summary>
        /// As part of the UW1 end game spawn a moongate and suck the avatar into the ethereal void.
        /// </summary>
        public static void LaunchPlayerIntoTheVoid()
        {
            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x116));// A Rending Sound fills the air.

            //Spawn moongate
            var newmoongate = ObjectCreator.PrepareNewObject(0x15A, ObjectFreeLists.ObjectListType.StaticList);
            var obj = UWTileMap.current_tilemap.LevelObjects[newmoongate];
            obj.is_quant = 1;
            var moontile = UWTileMap.current_tilemap.Tiles[0x20, 0x20];
            obj.tileX = 0x20; obj.tileY = 0x20;
            //insert object to tile.
            obj.link = 0x2C0;
            obj.next = moontile.indexObjectList;
            moontile.indexObjectList = obj.index;
            ObjectCreator.RenderObject(obj, UWTileMap.current_tilemap);


            //Start the camera changes
            _ = Peaky.Coroutines.Coroutine.Run(
                        MoveCameraToVoid(0x20,0x20),
                        main.instance);
        }


        /// <summary>
        /// Janky effect to move the player to the camera.
        /// </summary>
        /// <param name="moongate"></param>
        /// <returns></returns>
        public static IEnumerator MoveCameraToVoid(byte targetX = 0x20, byte targetY = 0x20)
        {
            playerdat.StepsTakenToMoongate = 0;
            playerdat.MoongateSuckX = targetX;
            playerdat.MoongateSuckY = targetY;
            playerdat.MoongateSucking = true;

            var xdist = (playerdat.MoongateSuckX<<8) - motion.playerMotionParams.x_0;
            var ydist = (playerdat.MoongateSuckY<<8) - motion.playerMotionParams.y_2;
            playerdat.MoongateDist = (short)(((short)Math.Sqrt((xdist * xdist) + (ydist * ydist)))>>6);

            var distvalue = (playerdat.MoongateDist & 0xFFFF) << 6;
            
            var di_x = (xdist << 0xF) / distvalue;
            var si_Y = (ydist << 0xF) / distvalue;

            playerdat.MoonGateCameraYaw = (ushort)motion.MaybeGetTangent_seg021_22FD_EFB((short)si_Y, (short)di_x);

            while (playerdat.StepsTakenToMoongate < 0x40)
            {
                playerdat.PositionPlayerCamera();
                playerdat.StepsTakenToMoongate++;
                playerdat.MoonGateCameraYaw += 0xCCB;
                yield return new WaitForSeconds(0.1f);
                // traveltime+=speed;
                // main.cameraPitchGimbal.Position = new Godot.Vector3(Mathf.Lerp(initial.X, moongate.instance.uwnode.Position.X, traveltime/5f), initial.Y, Mathf.Lerp(initial.Z, moongate.instance.uwnode.Position.Z, traveltime/5f));  
            }
            //Turn off camera effect.
            playerdat.MoongateSucking = false;

            //Do teleports
            Teleportation.Teleport(character: 1, tileX: 0x1B, tileY: 0x17, newLevel: 9, HeadingHeightFlag: 0);

        }

    }//end class    
}//end namespace
