using Godot;

namespace Underworld
{
    public class runetrap : uwObject
    {

        public static bool CreateRuneTrap(uwObject obj, Node3D parent)
        {
            var h = ((float)commonObjDat.height(obj.item_id) / 128f) * 0.15f;
            h = h * 10; //increase size a bit. not sure what height it should be, but a bigger box makes it easier to test.
            var r = ((float)commonObjDat.radius(obj.item_id) / 8f) * 1.2f;
            var ar = new a_runetrapcollision();
            ar.uwObjectIndex = obj.index;
            var col = new CollisionShape3D();
            var box = new BoxShape3D();
            box.Set("extents", new Vector3(r, h, r));
            col.Shape = box;
            ar.AddChild(col);
            parent.AddChild(ar);
            ar.BodyEntered += ar.runetrap_entered;
            return true;
        }

        /// <summary>
        /// Triggers the effects of rune traps
        /// </summary>
        /// <param name="indexCharacter"></param>
        /// <param name="indexTrap"></param>
        /// <returns></returns>
        public static bool Use(int indexCharacter, int indexTrap)
        {
            var objRuneTrap = UWTileMap.current_tilemap.LevelObjects[indexTrap];
        
            switch(objRuneTrap.item_id)
            {
                case 0x19E://flam rune
                    var damageToApply = 4 + Rng.DiceRoll(3,4);
                    if (indexCharacter == 0)
                    {
                        //damage player
                        damage.DamagePlayer(damageToApply, 8, 0);
                    }
                    else
                    {//damage other objects
                        var objToDamage = UWTileMap.current_tilemap.LevelObjects[indexCharacter];
                        damage.DamageObject(objToDamage: objToDamage, basedamage: damageToApply, damagetype: 8, objList: UWTileMap.current_tilemap.LevelObjects, WorldObject: true, damagesource: 0);                    
                    }
                    //explosion
                    animo.SpawnAnimoInTile(2, objRuneTrap.xpos,objRuneTrap.ypos, objRuneTrap.zpos, objRuneTrap.tileX, objRuneTrap.tileY);
                    ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(objRuneTrap.tileX, objRuneTrap.tileY, objRuneTrap.index); 
                    return true;
                case 0x19F://tym rune
                    if (indexCharacter==0)
                    {
                        var duration = 4 + Rng.r.Next(0xF);
                        //set paralyse timer
                        playerdat.ParalyseTimer = duration;                        
                        main.gamecam.Set("MOVE", false);
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,0x163));//your limbs stiffen
                    }
                    else
                    {
                        //paralyse
                        var objToDamage = UWTileMap.current_tilemap.LevelObjects[indexCharacter];
                        var duration = 64 + (Rng.r.Next(63)>>2);
                        SpellCasting.ApplyAIChangingSpell(
                            targetObject:  objToDamage,
                            newgoal: (byte)npc.npc_goals.npc_goal_petrified,
                            newgtarg: (byte)duration, 
                            newattitude: 1);
                    }
                    ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(objRuneTrap.tileX, objRuneTrap.tileY, objRuneTrap.index); 
                    return true;
                default:
                    return false;
            }
           }
    }//end class

    public partial class a_runetrapcollision : Area3D
    {
        public int uwObjectIndex;

        [Signal]
        public delegate void MoveTriggerEnteredEventHandler();

        public void runetrap_entered(Node3D body)
        {
            if (body.Name == "Gronk")
            {
                //Debug.Print($"{body.Name} collides with {uwObjectIndex}");
                runetrap.Use(indexCharacter: 0, indexTrap: uwObjectIndex);
            }
        }
    }

}//end namespace