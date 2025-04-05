using System;
using System.Linq;
using UnityEngine;

namespace Project.Scripts.Saving.Utils
{
    public static class SafeActionExtensions
    {
        public static void SafeInvoke(this Action action)
        {
            if (action == null)
                return;
            foreach (Action action1 in action.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action1();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred in SafeAction<T>: {(object)ex}");
                }
            }
        }

        public static void SafeInvoke<T>(this Action<T> action, T arg)
        {
            if (action == null)
                return;
            foreach (Action<T> action1 in action.GetInvocationList().Cast<Action<T>>())
            {
                try
                {
                    action1(arg);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred in SafeAction<T>: {(object)ex}");
                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null)
                return;
            foreach (Action<T1, T2> action1 in action.GetInvocationList().Cast<Action<T1, T2>>())
            {
                try
                {
                    action1(arg1, arg2);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred in Action<T1, T2>: {(object)ex}");
                }
            }
        }
    }
}