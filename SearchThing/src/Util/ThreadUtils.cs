using MelonLoader;
using Microsoft.VisualBasic;

namespace SearchThing.Util;

public static class ThreadUtils
{
    public static void RunOnMainThread(this Action action)
    {
        MelonCoroutines.Start(InvokeOnMainThread(action));   
    }
    
    public static void RunOnMainThread<T>(this Action<T> action, T arg)
    {
        MelonCoroutines.Start(InvokeOnMainThread(() => action(arg)));   
    }
    
    private static System.Collections.IEnumerator InvokeOnMainThread(Action action)
    {
        yield return null; // Wait one frame to ensure we're on main thread
        action();
    }
}