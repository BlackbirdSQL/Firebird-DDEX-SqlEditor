#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;



namespace BlackbirdSql.Shared.Events;


[ComVisible(false)]
public delegate void SpecialEditorCommandEventHandler(object sender, SpecialEditorCommandEventArgs e);


[ComVisible(false)]
public sealed class SpecialEditorCommandEventArgs : EventArgs
{
	private readonly int _cmdID;

	private readonly object[] _vIn;

	private readonly IntPtr _vOut;

	public int CommandID => _cmdID;

	public object[] VariantIn => _vIn;

	public IntPtr VariantOut => _vOut;

	public SpecialEditorCommandEventArgs(int cmdID, object[] vIn, IntPtr vOut)
	{
		_cmdID = cmdID;
		_vIn = vIn;
		_vOut = vOut;
	}
}
