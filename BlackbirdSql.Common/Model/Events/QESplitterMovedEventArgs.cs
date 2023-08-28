#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events
{
	public class QESplitterMovedEventArgs : EventArgs
	{
		private readonly int _splitPos;

		private readonly Control _boundControl;

		public int SplitPosition => _splitPos;

		public Control BoundControl => _boundControl;

		public QESplitterMovedEventArgs(int splitPos, Control boundControl)
		{
			_splitPos = splitPos;
			_boundControl = boundControl;
		}
	}
}
