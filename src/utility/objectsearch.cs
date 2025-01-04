namespace Underworld
{
    /// <summary>
    /// Class to find objects in tiles etc
    /// </summary>
    public class objectsearch : UWClass
    {

        /// <summary>
        /// Searches the specified tile for a matching object
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="classindex"></param>
        /// <returns></returns>
        public static uwObject FindMatchInTile(int tileX, int tileY, int majorclass, int minorclass, int classindex)
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX,tileY];
            return FindMatchInObjectListChain(tile.indexObjectList, majorclass, minorclass,classindex, UWTileMap.current_tilemap.LevelObjects);
        }


        /// <summary>
        /// Finds the matching object in the chain starting at ListHeadIndex. Does not go into linked items
        /// </summary>
        /// <param name="ListHeadIndex"></param>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="classindex"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static uwObject FindMatchInObjectListChain(int ListHeadIndex, int majorclass, int minorclass, int classindex, uwObject[] objList)
        {                
            var next = ListHeadIndex;
            while (next != 0)
            {
                var testObj = objList[next];
                if (testObj.majorclass == majorclass)
                { //matching major class
                    if ((testObj.minorclass == minorclass) || (minorclass == -1))
                    {//Either minor class matches or if minorclass =-1 (find all)
                        if ((testObj.classindex == classindex) || (classindex == -1))
                        {//obj match found.
                            return testObj;
                        }
                    }
                }
                next = testObj.next;
            }                        
            return null; //nothing found. 
        }

        // public static uwObject FindMatchInObjectChainTopLevel(int ListHeadIndex, int majorclass, int minorclass, int classindex, uwObject[] objList)
        // {
        //     if (ListHeadIndex != 0)
        //     {
        //         var TopObject = objList[ListHeadIndex];
        //         if (TopObject != null)
        //         {
        //             //check the top object first.
        //             if (TopObject.majorclass == majorclass)
        //             { //matching major class
        //                 if ((TopObject.minorclass == minorclass) || (minorclass == -1))
        //                 {//Either minor class matches or if minorclass =-1 (find all)
        //                     if ((TopObject.classindex == classindex) || (classindex == -1))
        //                     {//obj match found.
        //                         return TopObject;
        //                     }
        //                 }
        //             }

        //             if (TopObject.is_quant == 0)
        //             {
        //                 if (TopObject.link != 0)
        //                 {//check the contents of the top object
        //                     var next = TopObject.link;
        //                     while (next != 0)
        //                     {
        //                         var testObj = objList[next];
        //                         if (testObj.majorclass == majorclass)
        //                         { //matching major class
        //                             if ((testObj.minorclass == minorclass) || (minorclass == -1))
        //                             {//Either minor class matches or if minorclass =-1 (find all)
        //                                 if ((testObj.classindex == classindex) || (classindex == -1))
        //                                 {//obj match found.
        //                                     return testObj;
        //                                 }
        //                             }
        //                         }
        //                         next = testObj.next;
        //                     }
        //                 }
        //             }


        //         }
        //     }
        //     return null; //nothing found. 
        // }


        /// <summary>
        /// Searches for the first matching object type in the specified object list chain
        /// </summary>
        /// <param name="ListHeadIndex">Item Index to test for matching.</param>
        /// <param name="majorclass"></param>
        /// <param name="minorclass">use -1 to return any of the minor class</param>
        /// <param name="classindex">use -1 to return any of the class index </param>
        /// <param name="objList">The list to search within</param>
        /// <param name="SkipNext">Search up in the next object</param>
        /// <param name="SkipLinks">Search up in the links</param>
        /// <returns></returns>
        public static uwObject FindMatchInObjectChain(int ListHeadIndex, int majorclass, int minorclass, int classindex, uwObject[] objList, bool SkipNext = false, bool SkipLinks = false)
        {
            if (ListHeadIndex != 0)
            {
                var testObj = objList[ListHeadIndex];
                if (testObj != null)
                {
                    if (testObj.majorclass == majorclass)
                    { //matching major class
                        if ((testObj.minorclass == minorclass) || (minorclass == -1))
                        {//Either minor class matches or if minorclass =-1 (find all)
                            if ((testObj.classindex == classindex) || (classindex == -1))
                            {//obj match found.
                                return testObj;
                            }
                        }
                    }

                    if (testObj.is_quant == 0)
                    {
                        if (testObj.link != 0)
                        {
                            var testlinked = FindMatchInObjectChain(
                                ListHeadIndex: testObj.link,
                                majorclass: majorclass,
                                minorclass: minorclass,
                                classindex: classindex,
                                objList: objList,
                                SkipNext: SkipNext,
                                SkipLinks: SkipLinks);
                            if (testlinked != null)
                            {
                                return testlinked;
                            }
                        }
                    }
                    if (!SkipNext)
                    {
                        //no matches. Try next value. Returns null if nothing found.
                        return FindMatchInObjectChain(
                            ListHeadIndex: testObj.next,
                            majorclass: majorclass,
                            minorclass: minorclass,
                            classindex: classindex,
                            objList: objList,
                            SkipNext: SkipNext,
                            SkipLinks: SkipLinks);
                    }
                }
            }
            return null; //nothing found. 
        }

        /// <summary>
        /// Finds a matching object in a full list of objects
        /// </summary>
        /// <param name="majorclass"></param>
        /// <param name="minorclass"></param>
        /// <param name="classindex"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static uwObject FindMatchInFullObjectList(int majorclass, int minorclass, int classindex, uwObject[] objList)
        {
            for (int i = 1; i <= objList.GetUpperBound(0); i++)
            {
                var testObj = objList[i];
                if (testObj != null)
                {
                    if (testObj.majorclass == majorclass)
                    { //matching major class
                        if ((testObj.minorclass == minorclass) || (minorclass == -1))
                        {//Either minor class matches or if minorclass =-1 (find all)
                            if ((testObj.classindex == classindex) || (classindex == -1))
                            {//obj match found.
                                return testObj;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the object index that contains the ToFind object
        /// </summary>
        /// <param name="ListHead"></param>
        /// <param name="ToFind"></param>
        /// <returns></returns>
        public static int GetContainingObject(int ListHead, int ToFind, uwObject[] objList)
        {
            if (ListHead == ToFind)
            {
                //same object
                return -1;
            }
            if (ListHead <= 0) { return -1; }
            var ListHeadObject = objList[ListHead];
            if (ListHeadObject != null)
            {
                if ((ListHeadObject.link != 0) && (ListHeadObject.is_quant == 0))
                { //List has objects
                    if (ListHeadObject.link == ToFind)
                    { //The first object is the one we want to find.
                        return ListHeadObject.index;
                    }
                    else
                    {//Go throught the chain. Check each next and if the next has an object link search up that chain
                        var NextObjectIndex = ListHeadObject.link; //get the first object
                        while (NextObjectIndex != 0)
                        {
                            var NextObject = objList[NextObjectIndex];
                            if (NextObject.index == ToFind)
                            {//This object is the one I want to find. Return the list head I started from.
                                return ListHeadObject.index;
                            }
                            else
                            {
                                if (NextObject.is_quant == 0)
                                {
                                    if (NextObject.link != 0)
                                    { //search up that object chain
                                        var result = GetContainingObject(NextObject.index, ToFind, objList);
                                        if (result != -1)
                                        {
                                            return result;
                                        }
                                    }
                                }
                                //No matches. Try the next object.
                                NextObjectIndex = NextObject.next;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of an object that either links to indexToFind or is the object previous to it in the linked list
        /// </summary>
        /// <param name="listhead"></param>
        /// <param name="indexToFind"></param>
        /// <param name="objList"></param>
        /// <returns>0 if nothing found, other the object that links to or is previous to</returns>
        public static int GetObjectParent(int listhead, int indexToFind, uwObject[] objList)
        {
            var next = listhead;
            while (next != 0)
            {
                var nextObj = objList[next];
                if (nextObj.next == indexToFind)
                {
                    return next;
                }
                if (nextObj.is_quant==0 && nextObj.link>0)
                {
                    var result = GetObjectParent(nextObj.link, indexToFind, objList);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                next = nextObj.next;
            }
            return 0;
        }
    }//end class
}//end namespace