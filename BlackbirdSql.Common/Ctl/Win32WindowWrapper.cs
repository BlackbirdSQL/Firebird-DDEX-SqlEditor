// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.Win32WindowWrapper
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace BlackbirdSql.Common.Ctl;


[ComVisible(false)]
public class Win32WindowWrapper : IWin32Window
{
	private readonly IntPtr handle;

	public IntPtr Handle => handle;

	public Win32WindowWrapper(IntPtr handle)
	{
		this.handle = handle;
	}
}
