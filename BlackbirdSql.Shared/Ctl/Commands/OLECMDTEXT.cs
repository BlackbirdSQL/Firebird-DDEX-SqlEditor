#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;



namespace BlackbirdSql.Shared.Ctl.Commands;


[StructLayout(LayoutKind.Sequential)]
[ComVisible(false)]
public sealed class OLECMDTEXT
{
	public static string GetText(IntPtr pCmdTextInt)
	{
		Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT oLECMDTEXT = (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));
		IntPtr intPtr = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "rgwz");
		if (oLECMDTEXT.cwActual == 0)
		{
			return "";
		}

		char[] array = new char[oLECMDTEXT.cwActual - 1];
		Marshal.Copy((IntPtr)((long)pCmdTextInt + (long)intPtr), array, 0, array.Length);
		StringBuilder stringBuilder = new StringBuilder(array.Length);
		stringBuilder.Append(array);
		return stringBuilder.ToString();
	}

	public static void SetText(IntPtr pCmdTextInt, string text)
	{
		Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT obj = (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));
		char[] array = text.ToCharArray();
		int num = (int)Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "rgwz");
		int num2 = (int)Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "cwActual");
		int num3 = (int)Math.Min(obj.cwBuf - 1, array.Length);
		Marshal.Copy(array, 0, (IntPtr)((long)pCmdTextInt + num), num3);
		Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + num + num3 * 2), 0);
		Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + num2), num3 + 1);
	}
}
