#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
// namespace Microsoft.SqlServer.Data.Tools.ExceptionMessageBox
#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace BlackbirdSql.Sys.Controls;

[Guid(LibraryData.C_MessageBoxParentGuid)]


public sealed class MessageBoxParent(IntPtr HWnd) : IWin32Window
{
	private readonly IntPtr _Handle = HWnd;

	public IntPtr Handle => _Handle;
}
