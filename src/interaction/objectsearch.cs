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
        public static uwObject FindMatchInObjectList(int ListHeadIndex, int majorclass, int minorclass, int classindex, uwObject[] objList) 
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
                    //no matches. Try next value. Returns null if nothing found.
                    return FindMatchInObjectList(
                        ListHeadIndex: testObj.next, 
                        majorclass: majorclass, 
                        minorclass: minorclass, 
                        classindex: classindex, 
                        objList: objList);
                }
            }           
           return null; //nothing found. 
        }
    }//end class
}//end namespace