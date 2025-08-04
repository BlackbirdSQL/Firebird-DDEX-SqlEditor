#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
#endregion

using System;
using System.Windows.Forms;



namespace BlackbirdSql.Shared.Events;


internal delegate void SplitterMovedEventHandler(object sender, SplitterMovedEventArgs e);


internal class SplitterMovedEventArgs : EventArgs
{
	private readonly int _splitPos;

	private readonly Control _boundControl;

	internal int SplitPosition => _splitPos;

	internal Control BoundControl => _boundControl;

	public SplitterMovedEventArgs(int splitPos, Control boundControl)
	{
		_splitPos = splitPos;
		_boundControl = boundControl;
	}
}
