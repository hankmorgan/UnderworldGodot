namespace Underworld
{
    public partial class combat : UWClass
    {
        private static void CastOnWeaponHitSpells()
        {
            if (DefendingCharacter == null)
            {
                return;
            }

            int currWeaponType = 0;
            if (currentweapon != null)
            {
                currWeaponType = isWeapon(currentweapon);
            }
            //post apply spell effect if applicable
            switch (OnHitSpell)
            {
                case 1:
                    {
                        //Debug.Print("Lifestealer");
                        if (currWeaponType > 0)
                        {
                            playerdat.HPRegenerationChange(-AttackDamage);
                        }
                        break;
                    }
                case 2:
                    {//Debug.Print("Undeadbane"); 
                        if (currWeaponType > 0)
                        {
                            SpellCasting.SmiteUndead(DefendingCharacter.index, UWTileMap.current_tilemap.LevelObjects, playerdat.playerObject);
                        }
                        break;
                    }
                case 3:
                    {
                        //Debug.Print("Firedoom"); 
                        //explosion
                        var tile = UWTileMap.current_tilemap.Tiles[DefendingCharacter.tileX, DefendingCharacter.tileY];
                        var height = tile.floorHeight << 3;
                        if (height < 0x80)
                        {
                            height = height + Rng.r.Next(0x80 - height);
                        }
                        animo.SpawnAnimoInTile(subclassindex: 2, xpos: 3, ypos: 3, zpos: (short)height, tileX: DefendingCharacter.tileX, tileY: DefendingCharacter.tileY);
                        //Do damage in area of tile.
                        damage.DamageObjectsInTile(DefendingCharacter.tileX, DefendingCharacter.tileY, 0, 1);
                        break;
                    }
                case 4:
                    {
                        //Debug.Print("stonestrike"); 
                        SpellCasting.Paralyse(DefendingCharacter.index, UWTileMap.current_tilemap.LevelObjects, playerdat.playerObject);
                        break;
                    }
                case 5:
                case 6:
                    {
                        if (DefendingCharacter.OneF0Class == 0x14)
                        {
                            //Debug.Print("Entry");
                            //is door
                            SpellCasting.Unlock(DefendingCharacter.index, UWTileMap.current_tilemap.LevelObjects);
                        }
                        break;
                    }
                    //case 7: Debug.Print("unknownspecial 7"); break;
                    //case 8: Debug.Print("unknownspecial 8"); break;
            }
        }
    }//end class
}//end namespace