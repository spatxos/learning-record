using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClassLib
{
    #region IFileOperation
    [ComVisible(true)]
    [Guid("3ad05575-8857-4850-9277-11b85bdb8e09")]
    public class CLSID_IFileOperation
    {

        //接口所属类的内容可为空，仅方便后续通过EasyHook API确定接口位置
    }
    [ComImport]
    [Guid("947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IID_IFileOperation
    {
        uint Advise(IntPtr pfops, IntPtr pdwCookie);
        void Unadvise(uint dwCookie);
        void SetOperationFlags(IntPtr dwOperationFlags);
        void SetProgressMessage(
         [MarshalAs(UnmanagedType.LPWStr)] string pszMessage);
        void SetProgressDialog(
         [MarshalAs(UnmanagedType.Interface)] object popd);
        void SetProperties(
         [MarshalAs(UnmanagedType.Interface)] object pproparray);
        void SetOwnerWindow(uint hwndParent);
        void ApplyPropertiesToItem(IntPtr psiItem);
        void ApplyPropertiesToItems(
         [MarshalAs(UnmanagedType.Interface)] object punkItems);
        void RenameItem(IntPtr psiItem,
         [MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
         IntPtr pfopsItem);
        void RenameItems(
        IntPtr pUnkItems,
         [MarshalAs(UnmanagedType.LPWStr)] string pszNewName);
        void MoveItem(
         IntPtr psiItem,
         IntPtr psiDestinationFolder,
         [MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
         IntPtr pfopsItem);
        void MoveItems(
         IntPtr punkItems,
         IntPtr psiDestinationFolder);
        void CopyItem(
         IntPtr psiItem,
         IntPtr psiDestinationFolder,
         [MarshalAs(UnmanagedType.LPWStr)] string pszCopyName,
         IntPtr pfopsItem);
        void CopyItems(
         IntPtr punkItems,
         IntPtr psiDestinationFolder);
        void DeleteItem(
         IntPtr psiItem,
         IntPtr pfopsItem);
        void DeleteItems(
        IntPtr punkItems);
        uint NewItem(
         IntPtr psiDestinationFolder,
         IntPtr dwFileAttributes,
         [MarshalAs(UnmanagedType.LPWStr)] string pszName,
         [MarshalAs(UnmanagedType.LPWStr)] string pszTemplateName,
         IntPtr pfopsItem);
        long PerformOperations();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetAnyOperationsAborted();
    }
    #endregion
}
