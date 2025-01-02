using System.Diagnostics;


namespace Underworld
{
    /// <summary>
    /// For handling the bizarre dream logic of sleeping
    /// </summary>
    public class sleep : UWClass
    {

        /// <summary>
        /// Handles the player going to sleep
        /// </summary>
        /// <param name="sleepmethod">0=no bedroll, 1=with bedroll, 2=In a bed, -2 Drunk with alcohol.</param>
        public static void Sleep(int sleepmethod)
        {
            var PlayerWillDie = false;
            var DidDream = true;

            if (sleepmethod >= 0)
            {
                if (((playerdat.TileState & 0x1B) == 0) && (playerdat.ZVelocity == 0))
                {
                    if ((_RES == GAME_UW2) && (playerdat.CurrentWorld == 8) && (playerdat.DreamingInVoid))
                    {//player has tried to sleep while sleeping in a void vision. end the void vision.
                        AwakenFromTheVoid();
                        return;
                    }
                    else
                    {
                        //test for nearby hostiles.
                        if (TestForNearByEnemies() || playerdat.IsFightingInPit)
                        {//Enemies nearby
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0xE));
                            return;
                        }
                        else
                        {//no enemies.
                            if (sleepmethod == 0 || sleepmethod > 2)
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0xF));//you make camp
                            }
                            else
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, sleepmethod + 0x15));//various strings relating to how you go to sleep
                            }
                        }
                    }
                }
                else
                {//tilestate <> 0
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x14));
                    return;
                }
            }

            //ovr154_BCA
            //fade to black
            //pick music

            if (sleepmethod >= 0)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x10));//you go to sleep
            }

            FindAndCloseDoors(0);

            //Cull objects.

            var si_hourstosleep = 2 + Rng.r.Next(5);

            playerdat.ActiveSpellEffectCount = 0;
            playerdat.shrooms = 0;

            playerdat.AdvanceTime(si_hourstosleep * 0xE10);

            if (_RES == GAME_UW2)
            {
                if ((playerdat.GetQuest(50) == 1) && (playerdat.GetQuest(54) != 0))//keep is crashing and has not yet crashed.
                {
                    KilornIsCrashing(0);
                    damage.DamagePlayer(
                        basedamage: 0xFF,
                        damagetype: 0,
                        damagesource: 0);//kill the player for being dumb enough to fall asleep in a crashing keep.
                }
            }


            playerdat.UpdateLightStability(si_hourstosleep * 0x180);

            if (playerdat.play_poison > 0)
            {
                damage.DamagePlayer(
                    basedamage: ((playerdat.play_poison + 1) * playerdat.play_poison) >> 1,
                    damagetype: 0x10,
                    damagesource: 0);
                playerdat.play_poison = 0;
            }

            if (sleepmethod < 0)
            {
                //drunk-ass Avatar.
                SleepOnDamagingSurface();
                if (_RES == GAME_UW2 && playerdat.IsFightingInPit)
                {//Avatar fell asleep drunk during a duel!
                    damage.DamagePlayer(
                        basedamage: 0xFF,
                        damagetype: 0,
                        damagesource: 0);
                }
            }

            if (playerdat.play_hp == 0)
            {
                return;//player has died. Handle their death on the next update()
            }
            else
            {
                if (UpdateNPCMovements())
                {//sleep has been interrupted
                    SleepInterupted();
                }
                else
                {
                    Debug.Print("Add calm function here");
                    a_create_object_trap.FindAndRunCreateObjectTraps();

                    si_hourstosleep = 7 + Rng.r.Next(4) - si_hourstosleep;
                    if (playerdat.play_hp < 0xA)
                    {
                        si_hourstosleep = si_hourstosleep + 1 + Rng.r.Next(2);
                    }
                    playerdat.AdvanceTime(si_hourstosleep * 0xE10);
                    playerdat.UpdateLightStability(si_hourstosleep * 0xB4);

                    int foodregenfactor_var2;
                    if ((playerdat.play_hunger < 0x40) || (sleepmethod <= 0))
                    {
                        foodregenfactor_var2 = 0;
                    }
                    else
                    {
                        foodregenfactor_var2 = 1;
                    }

                    var fatigueregenfactor_var4 = 2 + (playerdat.play_fatigue / 2);
                    if (fatigueregenfactor_var4 > 5)
                    {
                        fatigueregenfactor_var4 = 5;
                    }
                    playerdat.play_fatigue = 0;
                    if (playerdat.play_hunger == 0)
                    {//you are starving
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x11));
                        damage.DamagePlayer(
                            basedamage: 2,
                            damagetype: 0,
                            damagesource: 0);
                    }
                    else
                    {
                        var regen = fatigueregenfactor_var4 * foodregenfactor_var2;
                        playerdat.HPRegenerationChange(regen);
                        playerdat.ManaRegenChange(-6);
                        playerdat.ManaRegenChange(((fatigueregenfactor_var4 + 1) * foodregenfactor_var2) + fatigueregenfactor_var4 - 1);
                    }

                    playerdat.ChangeHunger(-2 - Rng.r.Next(0x1F));
                    UpdateIntoxication();

                    if (playerdat.play_hp == 0 && sleepmethod >= 0)
                    {
                        playerdat.play_hp = 1;//keep player alive
                        PlayerWillDie = true;
                    }

                    if (sleepmethod >= 0)
                    {
                        DidDream = !Dreams(sleepfactor: foodregenfactor_var2);
                    }
                    else
                    {
                        uimanager.FlashColour(1, uimanager.Cuts3DWin, 0.5f);
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x13 - foodregenfactor_var2));
                    }
                    if (PlayerWillDie)
                    {
                        PlayerWillDie = false;
                        playerdat.play_hp = 0;
                    }
                }

                
                playerdat.PlayerStatusUpdate();
                //Cancel all motion.

            }

        }//end sleep

        private static void SleepInterupted()
        {
            if (playerdat.play_fatigue <= 0x18)
            {
                playerdat.play_fatigue = 0;
            }
            else
            {
                playerdat.play_fatigue -= 0x18;
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x15));//your sleep is interrupted
            playerdat.ChangeHunger(-12 - Rng.r.Next(16));
            UpdateIntoxication();
        }

        private static void UpdateIntoxication()
        {
            if (playerdat.intoxication >= 0x10)
            {
                playerdat.intoxication -= 0x10;
            }
            else
            {
                playerdat.intoxication = 0;
            }
        }

        static void AwakenFromTheVoid()
        {
            Debug.Print("TODO: awaken from a dream in the void.");
        }

        static bool TestForNearByEnemies()
        {
            return false;
        }

        static void FindAndCloseDoors(int arg1)
        {
            Debug.Print("Find and close doors. move me to somewhere better once I start implementing SCD.ARK!");
        }

        static void KilornIsCrashing(int arg1)
        {
            Debug.Print("Killorn is crashing. move me to somewhere better once I start implementing Kilorn");
        }

        static void SleepOnDamagingSurface()
        {
            Debug.Print("Test to see if Dumbass Avatar fell asleep drunk on some lava and if so Apply damage");
        }

        static bool UpdateNPCMovements()
        {
            Debug.Print("Update/Simulate movement of NPCs while asleep and test if any of them have detected the player to initiate an ambush");
            return false;
        }

        static bool Dreams(int sleepfactor)
        {

            if (_RES == GAME_UW2)
            {
                return DreamsUW2(sleepfactor);
            }
            else
            {
                uimanager.FlashColour(1, uimanager.Cuts3DWin, 2f);
                Debug.Print("unimplemented dreams uw1");
                return false;
            }
        }

        static bool DreamsUW2(int sleepfactor)
        {
            int[] DreamXClocks = new int[] { 4, 6, 0xA, 0xE };
            if (playerdat.DreamPlantEaten)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x18));// your dreams are vivid
                playerdat.SetQuest(48, 1);
                Debug.Print("Do a void Dream");
                return false;
            }
            else
            {
                var si_dreamflags = playerdat.GetQuest(145);
                var xclock = playerdat.GetXClock(1);
                var FoundStageVar2 = -1;
                for (int counter_var4 = 0; counter_var4 < 4; counter_var4++)
                {
                    if (xclock>=DreamXClocks[counter_var4])
                    {
                        if ((si_dreamflags & (1<<counter_var4)) == (1<<counter_var4))
                        {
                            //found a dreamstage that has not yet happened
                            FoundStageVar2 = counter_var4;
                        }
                    }
                }
                if (FoundStageVar2<0)
                {
                    if (playerdat.DreamPlantEaten == false)
                    {//should always be the case.
                        FoundStageVar2 = 4 + Rng.r.Next(3); //try and pic a random dream.
                        if (((1<<FoundStageVar2) & si_dreamflags) !=0)
                        {
                            FoundStageVar2 = -1;
                        }
                    }
                }

                if (FoundStageVar2<0)
                {
                    //wait in sleep
                    uimanager.FlashColour(1, uimanager.Cuts3DWin, 2f);
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x13-sleepfactor));//rested or uneasy message
                    return false;
                }
                else
                {
                    Debug.Print($"play cutscene {0x18+FoundStageVar2}");
                    playerdat.SetQuest(145, playerdat.GetQuest(145) ^ (1<<FoundStageVar2));  //XOR the new value in.

                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x13-sleepfactor));//rested or uneasy message
                    return true;
                }
            }
        }

    }//end class
}//end namespace