using System.Collections.Generic;

namespace Underworld
{
    public class CutSceneCommand : UWClass
    {
        public int offset;
        public int frame;
        public int functionNo;
        public List<int> functionParams;
        public int NoOfParams
        {
            get
            {
                return GetArgumentCount(functionNo);
            }
        }

        // Command names from reverse engineering (disassembly ovr108, DPaint LPF format docs)
        static string[] FunctionNames = {
            "show-text",        // 0: display subtitle text with palette color
            "set-flag",         // 1: clears animation flag
            "no-op",            // 2: no operation
            "pause",            // 3: pause for arg/2 seconds
            "to-frame",         // 4: play N animation frames inline
            "frame-set",        // 5: segment boundary, frame field = frame count
            "end-cutsc",        // 6: end cutscene
            "rep-seg",          // 7: repeat segment N times
            "open-file",        // 8: load new LPF animation file
            "fade-out",         // 9: fade to black (rate: higher=faster, 0=instant)
            "fade-in",          // 10: fade from black
            "frame-trigger",    // 11: conditional frame advance (compares arg-1 with frame)
            "bit-control",      // 12: sets flag bit 4 to arg & 1
            "text-play",        // 13: show text + play VOC audio (999=no audio)
            "wait-secs",        // 14: wait N seconds
            "klang",            // 15: play klang sound (UW1 only)
            "pal-copy",         // 16: palette data copy between buffers
            "timer-cb",         // 17: register timer callback for synchronized events
            "no-op2",           // 18: no operation
            "pal-interp",       // 19: interpolate palette toward PALS.DAT[N] target
            "viewport-setup",   // 20: set panorama canvas dimensions + display offset
            "set-start",        // 21: set scroll start position
            "map-file",         // 22: load LBACK*.BYT background bitmap for panorama
            "start-scroll",     // 23: begin scrolling (direction table lookup, not raw speed)
            "audio-setup",      // 24: set audio file index (999=none, else decrement)
            "music",            // 25: play XMI music theme
            "no-op3",           // 26: no operation
            "audio-wait"        // 27: wait for audio playback completion or timeout
            };

        // Descriptions from disassembly reverse engineering (ovr108)
        static string[] FunctionDescriptions =
        {
            "displays text arg[1] with palette color arg[0]",
            "clears internal animation flag at +39h",
            "no-op, arguments are ignored",
            "pauses for arg[0] / 2 seconds",
            "plays arg[0] animation frames inline",
            "segment boundary marker; frame field = number of frames in segment",
            "ends cutscene, clears animation and end flags",
            "repeat segment arg[0] times; sets up replay parameters",
            "opens a new file csXXX.nYY, with XXX from arg[0] and YY from arg[1]; octal encoding",
            "fades out at rate arg[0] (higher is faster, 0=instant)",
            "fades in at rate arg[0] (higher is faster, 0=instant)",
            "conditional frame advance: compares arg[0]-1 with current frame, sets trigger if different",
            "sets flag bit 4 to (arg[0] & 1)",
            "displays text arg[1] with color arg[0] and plays VOC audio arg[2] (999=no audio)",
            "wait arg[0] seconds (arg[1] used based on flags)",
            "plays 'klang' sound (UW1 only)",
            "copies palette data between buffers",
            "registers timer callback for synchronized events",
            "does nothing (no-op)",
            "interpolates LPF palette toward PALS.DAT[arg[0]] at speed arg[1] for arg[2] frames",
            "sets panorama canvas: arg[0]=width, arg[1]=height, arg[2]=display offset",
            "sets scroll start position: arg[0]=X, arg[1]=Y",
            "loads LBACK background bitmap: arg[0]=position, arg[1]=extent, arg[2]=LBACK file index",
            "begins scrolling: arg[0]=direction table index (0=Down,1=Right,2=Up,3=Left), arg[1]=delta multiplier",
            "sets audio file index: 999->none, otherwise decrements by 1",
            "plays XMI music theme arg[0]",
            "does nothing (no-op)",
            "waits for audio playback completion or timeout arg[0]"
        };
        public string FunctionName
        {
            get
            {
                if (functionNo > FunctionNames.GetUpperBound(0)) { return "OUTOFBOUNDS No " + functionNo + " "; }
                return FunctionNames[functionNo];
            }
        }
        public string FunctionDesc
        {
            get
            {
                if (functionNo > FunctionDescriptions.GetUpperBound(0)) { return "OUTOFBOUNDS"; }
                return FunctionDescriptions[functionNo];
            }
        }

        /// <summary>
        /// Returns the number of arguments for the specicied cutscene command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        static int GetArgumentCount(int cmd)
        {
            switch (cmd)
            {
                case 0: return 2;//displays text arg[1] with color arg[0]
                case 1: return 0;//sets some flag to 0
                case 2: return 2;//no-op, arguments are ignored
                case 3: return 1;//pauses for arg[0] / 2 seconds
                case 4: return 2;//plays up to frame arg[0]
                case 5: //unknown, set frame for static cutscene? (0 params in UW2, 1 in UW1)
                    if (_RES == GAME_UW2)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                case 6: return 0;//ends cutscene
                case 7: return 1;//repeat segment arg[0] times
                case 8: return 2;//opens a new file csXXX.nYY, with XXX from arg[0] and YY from arg[1]; XXX and YY are formatted octal values
                case 9: return 1;//fades out at rate arg[0] (higher is faster)
                case 10: return 1;//fades in at rate arg[0] (higher is faster)
                case 11: return 1;//unknown
                case 12: return 1;//unknown
                case 13: return 3;//displays text arg[1] with color arg[0] and plays audio arg[2]
                case 14: return 2;//wait arg[0] or arg[1] seconds
                case 15: return 0;//plays 'klang' sound (UW1 only)
                //case 16: return *;//unknown, not used in uw2 but looks like some sort of copy function.
                //case 17: return *;//unknown, not used in uw2
                case 18: return 4;//does nothing, not used in uw2
                case 19: return 3;//unknown
                case 20: return 3;//unknown
                case 21: return 2;//unknown
                case 22: return 3;//unknown
                case 23: return 3;//unknown
                case 24: return 1;//unknown, usually at the start of cutscene
                case 25: return 1;//music
                case 26: return 1;//does nothing, not used in uw2
                case 27: return 1;//unknown
            }
            return 0;
        }
    }//end namespace
}// end class