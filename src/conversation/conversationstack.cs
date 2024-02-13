using System.Diagnostics;
namespace Underworld
{

    /// <summary>
    /// Implementation of the conversation memory stack
    /// </summary>
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// The stack values.
        /// </summary>
        public static short[] StackValues;

        /// <summary>
        /// The stack pointer
        /// </summary>
        public static int stackptr = 0;

        /// <summary>
        /// The top value in the stack.
        /// </summary>
       // public int TopValue = 0;

        /// <summary>
        /// The result register for imported functions.
        /// </summary>
        public static int result_register;

        /// <summary>
        /// The call level of the VM.
        /// </summary>
        public static int call_level = 1;


        /// <summary>
        /// Pop a value from the stack
        /// </summary>
        public static short Pop()
        {  
            var popvalue = StackValues[stackptr];
            Debug.Print($"Pop {popvalue} from {stackptr}");
            stackptr--;            
            return popvalue;
        }


        /// <summary>
        /// Push a value to the stack
        /// </summary>
        /// <param name="newValue">New value.</param>
        public static void Push(short newValue)
        {
            stackptr++;
            StackValues[stackptr] = newValue;
            Debug.Print($"Push {newValue} to {stackptr}");
        }

        public static void Push(int newValue)
        {
            Push ((short)newValue);
        }

        /// <summary>
        /// Set the specified value on the stack
        /// </summary>
        /// <param name="index">Index to change</param>
        /// <param name="val">value to set</param>
        public static void Set(int index, short val)
        {
            Debug.Print($"Set {val} at {index}");
            StackValues[index] = val;//I hope!
        }

        public static void Set(int index, int val)
        {
            Set(index,(short)val);
        }

        // public static int get_stackp()
        // {
        //     return stackptr;
        // }

        public static void set_stackp(int ptr)
        {
            Debug.Print($"Set StackPtr to {ptr}");
            stackptr = ptr;
        }

        public static short at(int index)
        {
            if (index > StackValues.GetUpperBound(0))
            {
                Debug.Print($"Stack out of bounds- At ({index})");
                return 0;
            }
            if (index < 0)
            {
                Debug.Print($"Stack out of bounds (neg)- At ({index})");
                return 0;
            }
            return StackValues[index];
        }

        public static int Upperbound()
        {
            return StackValues.GetUpperBound(0);
        }

        /// <summary>
        /// Returns the value pointed to by the specified ptr value
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static int GetConvoStackValueAtPtr(int ptr)
        {
            return StackValues[at(ptr)];
        }
    }

}//end namespace