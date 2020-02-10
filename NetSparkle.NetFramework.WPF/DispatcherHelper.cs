using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace NetSparkle.UI.NetFramework.WPF
{
    public static class DispatcherHelper
    {
        [DebuggerStepThrough]
        public static void BeginInvoke(DispatcherPriority dispatcherPriority, Action invocableAction, bool ignoreExceptions = true)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
            {
                Execute(invocableAction, ignoreExceptions);
            }
            else
            {
                Application.Current?.Dispatcher.BeginInvoke(dispatcherPriority, (Action)(() => Execute(invocableAction, ignoreExceptions)));
            }
        }

        [DebuggerStepThrough]
        public static void Invoke(DispatcherPriority dispatcherPriority, Action invocableAction, bool ignoreExceptions = true)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
            {
                Execute(invocableAction, ignoreExceptions);
            }
            else
            {
                Application.Current?.Dispatcher.Invoke(dispatcherPriority, (Action)(() => Execute(invocableAction, ignoreExceptions)));
            }
        }

        [DebuggerStepThrough]
        private static void Execute(Action invocableAction, bool ignoreExceptions)
        {
            if (ignoreExceptions)
            {
                try
                {
                    invocableAction();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                invocableAction();
            }
        }
    }
}
