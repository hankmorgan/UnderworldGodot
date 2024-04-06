namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        public static void place_object(uwObject talker)
        {
            var index = at(at(stackptr - 3));
            var tilex = at(at(stackptr - 2));
            var tiley = at(at(stackptr - 1));
            var tile = UWTileMap.current_tilemap.Tiles[tilex, tiley];

            //remove from the npcs inventory
            var previous = 0; var next = talker.link;
            while (next != 0)
            {
                var objToPlace = UWTileMap.current_tilemap.LevelObjects[next];
                if (objToPlace.index == index)
                {
                    //unlink from npc inventory
                    if (previous == 0)
                    {//head of npc chain
                        talker.link = objToPlace.next;
                    }
                    else
                    {//inside the chain
                        var PrevObject = UWTileMap.current_tilemap.LevelObjects[previous];
                        PrevObject.next = objToPlace.next;
                    }
                    objToPlace.next = 0;
                    result_register = moveobject(objToPlace, tile, tilex, tiley);
                    return;
                }
                previous = objToPlace.index;
                next = objToPlace.next;
            }
            result_register = 0;
        }


        static int moveobject(uwObject objToPlace, TileInfo tile, int tilex, int tiley)
        {
            UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
            objToPlace.tileX = tilex; objToPlace.tileY = tiley;
            objToPlace.xpos = (short)newxpos; objToPlace.ypos = (short)newypos; objToPlace.zpos = (short)newzpos;

            //link to tile chain.
            objToPlace.next = tile.indexObjectList;
            tile.indexObjectList = objToPlace.index;

            if (objToPlace.instance != null)
            {
                objToPlace.instance.uwnode.QueueFree();
            }
            ObjectCreator.RenderObject(objToPlace, UWTileMap.current_tilemap);

            return 1;
        }
    }//end class
}//end namespace