#region Assembly Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Runtime.InteropServices;
using BlackbirdSql.Core;




namespace BlackbirdSql.Shared.Interfaces;


[ComImport]
[Guid(LibraryData.EditorPaneGuid)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IBVsFindTarget3
{
	int IsNewUISupported
	{
		[PreserveSig]
		get;
	}

	[PreserveSig]
	int NotifyShowingNewUI();
}
