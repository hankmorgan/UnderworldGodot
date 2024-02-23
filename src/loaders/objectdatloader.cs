using System.IO;
namespace Underworld
{
    /// <summary>
    /// Base class for loading objects.dat
    /// </summary>
    public class objectDat:Loader
    {
        protected static byte[] buffer;
        static objectDat() 
        {
             ReadStreamFile(Path.Combine(BasePath,"DATA","OBJECTS.DAT"), out buffer);
        }
    } //end class

}//end namespace