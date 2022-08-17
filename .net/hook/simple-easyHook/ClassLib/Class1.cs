using EasyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassLib
{
    [Serializable]
    public class HookParameter
    {
        public string Msg { get; set; }
        public int HostProcessId { get; set; }
    }

    public class Main : EasyHook.IEntryPoint
    {
        public LocalHook MessageBoxWHook = null;
        public LocalHook MessageBoxAHook = null;

        public Main(
            RemoteHooking.IContext context,
            String channelName
            , HookParameter parameter
            )
        {
            MessageBox.Show(parameter.Msg, "Hooked");
        }

        public void Run(
            RemoteHooking.IContext context,
            String channelName
            , HookParameter parameter
            )
        {
            try
            {
                MessageBoxWHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxW"),
                    new DMessageBoxW(MessageBoxW_Hooked),
                    this);
                MessageBoxWHook.ThreadACL.SetExclusiveACL(new Int32[1]);

                MessageBoxAHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxA"),
                    new DMessageBoxW(MessageBoxA_Hooked),
                    this);
                MessageBoxAHook.ThreadACL.SetExclusiveACL(new Int32[1]);

                /**复制文件钩子**/
                COMClassInfo copyItemscom = new COMClassInfo(typeof(CLSID_IFileOperation), typeof(IID_IFileOperation), "CopyItems");  //
                copyItemscom.Query();
                var CopyItemsHook = EasyHook.LocalHook.Create(copyItemscom.MethodPointers[0], new CopyItems_Delegate(CopyItemsHooked), this);
                // 激活钩子
                CopyItemsHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                while (true)
                {
                    Thread.Sleep(10);
                }
            }
            catch
            {

            }
        }

        #region MessageBoxW

        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        public static extern IntPtr MessageBoxW(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        delegate IntPtr DMessageBoxW(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxW_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxW(hWnd, "Hooked - " + text, "Hooked - " + caption, type);
        }

        #endregion

        #region MessageBoxA

        [DllImport("user32.dll", EntryPoint = "MessageBoxA", CharSet = CharSet.Ansi)]
        public static extern IntPtr MessageBoxA(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate IntPtr DMessageBoxA(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxA_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxA(hWnd, "Hooked - " + text, "Hooked - " + caption, type);
        }

        #endregion


        #region CopyItems_Delegate为文件复制的委托函数
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = false)]
        public delegate void CopyItems_Delegate(IID_IFileOperation self, IntPtr punkItems,
IntPtr psiDestinationFolder);  //这里第一个参数需传入接口实例，为的是可以在替换的复制hook函数中调用原始的复制函数，达到不影响原始操作的目的。

        public void CopyItemsHooked(IID_IFileOperation self, IntPtr punkItems, IntPtr psiDestinationFolder)
        {
            MessageBox.Show("复制了啊");
            self.CopyItems(punkItems, psiDestinationFolder); //执行原始操作，也可不调用这句来实现拦截文件操作的目的
        }
        #endregion
    }
}
