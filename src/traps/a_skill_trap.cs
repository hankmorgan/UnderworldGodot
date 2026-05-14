using System.Diagnostics;

namespace Underworld
{
    public class a_skill_trap : trap
    {
        public static short Activate(uwObject trapObj)
        {
            Debug.Print("SKILL TRAP.Does as skill check and controls execution of other traps");
            int rngFactor;
            switch (trapObj.quality)
            {
                case < 3:
                    rngFactor = playerdat.GetAttributeValue(trapObj.quality);//STR,DEX or INT, vanilla behaviour is to get this data from critter.dat for the adventurer.
                    break;
                case 3:
                    rngFactor = 0xF;
                    break;
                case > 3:
                    rngFactor = playerdat.pdat[0x1E + trapObj.quality];//get skill value
                    break;
            }

            var Target_var34 = trapObj.owner * 3;
            int var35;
            if (trapObj.heading == 1)
            {
                //ovr166_9DF
                var skillCheck = playerdat.SkillCheck(skillValue: rngFactor, targetValue: trapObj.owner, debug: true);
                if (skillCheck > 0)
                {
                    //pass
                    var35 = 0;
                }
                else
                {
                    //fail
                    var35 = 1;
                }
            }
            else
            {
                //ovr166_9FA
                if (rngFactor >= Target_var34)
                {
                    var35 = 0;
                }
                else
                {
                    var35 = 1;
                }
            }

            if (var35 == 0)
            {                
                //will execute the next object on it's link
                //ovr166_A18
                var linked = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
                return linked.next;
            }
            else
            {
                //will stop or run default link.
                return trapObj.link;
            }
        }
    }
}