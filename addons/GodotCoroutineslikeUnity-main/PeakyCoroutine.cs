using Godot;
using System.Collections;
using System.Threading.Tasks;

namespace Peaky.Coroutines
{
    public static class Coroutine
    {
        public static async Task<int> Run(IEnumerator coroutine, Node runner)
        {
            while (coroutine.MoveNext())
            {
                if (coroutine.Current is IEnumerator casted)
                {
                    await Run(casted, runner);
                }
                else if (coroutine.Current is Waiter castWaiter)
                {
                    await castWaiter.Wait(runner);
                }
            }
            return 0;
        }
    }

    public abstract class Waiter
    {
        public virtual async Task<int> Wait(Node runner)
        {
            return 0;
        }
    }

    public class WaitOneFrame : Waiter
    {
        public override async Task<int> Wait(Node runner)
        {
#if GODOT4_OR_GREATER
            await runner.ToSignal(runner.GetTree(), "process_frame"); 
#else            
            await runner.ToSignal(runner.GetTree(), "idle_frame"); 
#endif
            return 0;
        }
    }

    public class WaitForSeconds : Waiter
    {
        public float TimeToWait;
        public WaitForSeconds(float seconds)
        {
            TimeToWait = seconds;
        }

        public override async Task<int> Wait(Node runner)
        {
            await Task.Delay((int)(TimeToWait * 1000f));
            return 0;
        }
    }
}
