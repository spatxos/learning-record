using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EasyHook;

namespace ClassLib
{
    public class TestClass : EasyHook.IEntryPoint
    {
        FileMonInterface Interface = null;
        LocalHook Hook;
        Stack<String> Queue = new Stack<String>();

        public TestClass(
            RemoteHooking.IContext InContext,
            String InChannelName)
        {
            // connect to host...  
            Interface = RemoteHooking.IpcConnectClient<FileMonInterface>(InChannelName);
            Interface.Ping();
        }

        public void Run(
           RemoteHooking.IContext InContext,
           String InChannelName)
        {
            // install hook...
            Hook = LocalHook.Create(
                LocalHook.GetProcAddress("user32.dll", "SetWindowTextW"),
                new DSetWindowText(SetWindowText_Hooked),
                this);

            Hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Interface.ReportException(e);
            }

            Hook.Dispose();
            LocalHook.Release();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        delegate bool DSetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        static bool SetWindowText_Hooked(IntPtr hWnd, string text)
        {
            //            Interface.ReportMessage(text);   // 可用于调试
            int num = 0;
            bool flag = int.TryParse(text, out num);
            if (flag)
            {
                text = (num + 1).ToString();//修改要显示的数据
            }
            return SetWindowText(hWnd, text);//调用API
        }
    }
}
