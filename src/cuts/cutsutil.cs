using System.Collections.Generic;

namespace Underworld
{

	public partial class cutsplayer : UWClass
	{
        /// <summary>
        /// Calculates the correct file name to use with cutsno
        /// </summary>
        /// <param name="cutsNo"></param>
        /// <param name="extensionNo"></param>
        /// <returns></returns>
        static string GetsCutsceneFileName(int cutsNo, int extensionNo)
        {
            string[] chars = new string[5];
            chars[0] = char.ToString((char)(48 + ((cutsNo>>6) & 7)));
            chars[1] = char.ToString((char)(48 + ((cutsNo>>3) & 7)));
            chars[2] = char.ToString((char)(48 + ((cutsNo) & 7)));
            chars[3] = char.ToString((char)(48 + ((extensionNo>>3) & 7)));
            chars[4] = char.ToString((char)(48 + ((extensionNo) & 7)));
            return $"CS{chars[0]}{chars[1]}{chars[2]}.N{chars[3]}{chars[4]}";
        }

        /// <summary>
        /// Loads cutscene commands from a block of data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<CutSceneCommand> LoadCutsceneData(byte[] data)
        {
            var output = new List<CutSceneCommand>();
            int addr_ptr = 0;

            while (addr_ptr < data.Length)
            {
                if(addr_ptr+4>data.Length)
                {
                    return output;
                }
                
                var cmd = new CutSceneCommand();
                cmd.offset = addr_ptr;
                cmd.frame = (int)Loader.getAt(data, addr_ptr, 16);
                cmd.functionNo = (int)Loader.getAt(data, addr_ptr + 2, 16);
                //int argCount =  // CutSceneCommand.GetArgumentCount(cmd.functionNo);
                cmd.functionParams = new List<int>();
                addr_ptr += 4;
                for (int i = 0; i < cmd.NoOfParams; i++)
                {
                    if(addr_ptr<data.Length)
                    {
                        cmd.functionParams.Add((int)Loader.getAt(data, addr_ptr, 16));
                    }                    
                    addr_ptr += 2;
                }                   
                output.Add(cmd);
            }
            return output;
        }
    }
}
