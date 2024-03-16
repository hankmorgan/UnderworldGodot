namespace Underworld
{
    /// <summary>
    /// Class to find objects in tiles etc
    /// </summary>
    public class objectsearch:UWClass
    {

        /// <summary>
        /// Searches for the first matching object type in the specified object list chain
        /// </summary>
        /// <param name="ListHeadIndex">Item Index to test for matching.</param>
        /// <param name="majorclass"></param>
        /// <param name="minorclass">use -1 to return any of the minor class</param>
        /// <param name="classindex">use -1 to return any of the class index </param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static uwObject FindMatchInObjectChain(int ListHeadIndex, int majorclass, int minorclass, int classindex, uwObject[] objList, bool SkipNext = false) 
        {
            if (ListHeadIndex !=0 )
            {
                var testObj = objList[ListHeadIndex];
                if (testObj!=null)
                {
                    if (testObj.majorclass == majorclass)
                    { //matching major class
                        if ((testObj.minorclass == minorclass)|| (minorclass==-1))
                        {//Either minor class matches or if minorclass =-1 (find all)
                            if ((testObj.classindex == classindex) || (classindex==-1))
                            {//obj match found.
                                return testObj;
                            }
                        }
                    }

                    if (testObj.is_quant==0)
                    {
                        if (testObj.link!=0)
                        {
                            var testlinked = FindMatchInObjectChain(
                                ListHeadIndex: testObj.link, 
                                majorclass: majorclass, 
                                minorclass: minorclass, 
                                classindex: classindex, 
                                objList: objList);                                
                            if (testlinked!=null)
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
                            objList: objList);
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
        public static uwObject FindMatchInObjectList(int majorclass, int minorclass, int classindex, uwObject[] objList) 
        {
            for (int i = 1; i<=objList.GetUpperBound(0);i++)
            {
                var testObj = objList[i];
                if (testObj!=null)
                {
                    if (testObj.majorclass == majorclass)
                    { //matching major class
                        if ((testObj.minorclass == minorclass)|| (minorclass==-1))
                        {//Either minor class matches or if minorclass =-1 (find all)
                            if ((testObj.classindex == classindex) || (classindex==-1))
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
            if (ListHead==ToFind)
            {
                //same object
                return -1;
            }
            if (ListHead<=0){return -1;}
            var ListHeadObject = objList[ListHead];
            if (ListHeadObject != null)
            {
                if ((ListHeadObject.link !=0) && (ListHeadObject.is_quant==0))
                { //List has objects
                    if (ListHeadObject.link == ToFind)
                    { //The first object is the one we want to find.
                        return ListHeadObject.index;
                    }
                    else
                    {//Go throught the chain. Check each next and if the next has an object link search up that chain
                        var NextObjectIndex = ListHeadObject.link; //get the first object
                        while (NextObjectIndex!=0)
                        {
                            var NextObject = objList[NextObjectIndex];
                            if (NextObject.index == ToFind)
                            {//This object is the one I want to find. Return the list head I started from.
                                return ListHeadObject.index;
                            }
                            else
                            {
                                if (NextObject.is_quant==0)
                                {
                                    if (NextObject.link!=0)
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

    }//end class
}//end namespace