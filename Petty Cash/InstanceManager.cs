using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

public static class InstanceManager
{
    public const int HWND_BROADCAST = 0xffff;
    private static readonly string GUID;
    private static readonly int WM_SHOWME;
    private static Mutex mutex;

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32")]
    public static extern int RegisterWindowMessage(string message);

    public static event Action BroadcastReceived;

    static InstanceManager()
    {
        GUID = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
        WM_SHOWME = RegisterWindowMessage(GUID);
    }

    public static bool IsInstanceAlreadyRunning()
    {
        //check multiple instances
        mutex = new Mutex(false, "Global\\" + GUID);
        if (!mutex.WaitOne(0, false)) //instance already running
        {
            PostMessage((IntPtr)HWND_BROADCAST, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            return true;
        }
        GC.Collect();
        return false;
    }

    public static void AddHookWndProc(Visual visual)
    {
        HwndSource source = PresentationSource.FromVisual(visual) as HwndSource;
        source.AddHook(WndProc);
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_SHOWME)
            BroadcastReceived?.Invoke();
        return IntPtr.Zero;
    }
}
