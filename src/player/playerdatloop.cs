using System.Diagnostics;

namespace Underworld
{
    //for handling loop updates for the player.
    public partial class playerdat : Loader
    {
        /// <summary>
        /// resets the player variables ahead of a status update
        /// </summary>
        static void ResetPlayer()
        {
            lightlevel = 0;
        }

        public static void PlayerStatusUpdate(bool CastOnEquip = false)
        {
            ResetPlayer();

            //Get armour protections, magic bonuses from equipment

            //Get brightest physcial light
            lightlevel = BrightestNonMagicalLight();

            //cast active spell effects
            for (int i = 0; i < 3; i++)
            {
                if (i < ActiveSpellEffectCount)
                {
                    var stability = GetEffectStability(i);
                    var effectclass = GetEffectClass(i);
                    var major = effectclass & 0xF;
                    var minor = effectclass >> 4;
                    Debug.Print($"Player has spell effect {major},{minor} of {stability} ");
                    SpellCasting.ApplyStatusEffectSpell(majorclass: major, minorclass: minor, TriggeredByInventoryEvent: false);
                    uimanager.SetSpellIcon(i, major, minor);
                }
                else
                {
                    uimanager.ClearSpellIcon(i);
                }
            }

            //apply spell effects from inventory objects
            for (int i = 0; i < 10; i++)
            {
                var objindex = uimanager.GetPaperDollObjAtSlot(i);
                if (objindex != -1)
                {
                    var obj = InventoryObjects[objindex];
                    if (obj != null)
                    {
                        bool isValid = true;

                        if (!uimanager.ValidObjectForSlot(i, obj))
                        {
                            isValid = false;
                        }
                        if (uimanager.DominantHandSlot == i)
                        {//Check for weapon in dominant hand.
                            if (
                                !
                            (
                                ((obj.majorclass == 0) && (obj.minorclass == 0))
                                ||
                                ((obj.majorclass == 0 && obj.minorclass == 1) && (obj.classindex >= 8 && obj.classindex <= 10))
                            )
                            )
                            {
                                isValid = false;
                            }
                        }

                        if (isValid)
                        {
                            var spell = MagicEnchantment.GetSpellEnchantment(obj: obj, InventoryObjects);
                            if (spell != null)
                            {
                                SpellCasting.CastSpell(
                                    majorclass: spell.SpellMajorClass,
                                    minorclass: spell.SpellMinorClass, caster: obj,
                                    target: null,
                                    tileX: tileX,
                                    tileY: tileY,
                                    CastOnEquip: CastOnEquip,
                                    PlayerCast: false);
                            }
                        }

                    }
                }
            }

            RefreshLighting();//either brightest physical light or brightest magical light
        }


        public static void RefreshLighting()
        {
            Godot.RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(playerdat.lightlevel));
            Godot.RenderingServer.GlobalShaderParameterSet("shades", shade.shadesdata[playerdat.lightlevel].ToImage());
        }

        /// <summary>
        /// Calculates the brightest non magical or ambient level light source that surrounds the player
        /// </summary>
        /// <returns></returns>
        public static int BrightestNonMagicalLight()
        {
            int lightlevel = 0; //darkness
            //5,6,7,8
            for (int i = 5; i <= 8; i++)
            {
                var obj = GetInventorySlotObject(i);
                if (obj != null)
                {
                    if ((obj.majorclass == 2) && (obj.minorclass == 1) && (obj.classindex >= 4) && (obj.classindex <= 7))
                    {   //object is a lit light
                        var level = lightsourceObjectDat.brightness(obj.item_id);
                        if (level > lightlevel)
                        {
                            lightlevel = level;
                        }
                    }
                }
            }
            //If uw2 check for dungeon light level
            if (_RES == GAME_UW2)
            {
                var dungeon_ambientlight = DlDat.GetAmbientLight(playerdat.dungeon_level - 1);
                var remainder = dungeon_ambientlight % 10;
                var dlFlag = 0;
                if (dungeon_ambientlight >= 10)
                {
                    dlFlag = 1;
                }
                int tileLightFlag = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY].lightFlag;
                if ((tileLightFlag ^ dlFlag) == 1)
                {
                    dungeon_ambientlight = remainder;
                }
                else
                {
                    dungeon_ambientlight = -1;
                }
                if (dungeon_ambientlight > lightlevel)
                {
                    lightlevel = dungeon_ambientlight;
                }
            }
            //TODO check for magic lights
            return lightlevel;
        }
    }//end class
}//end namespace