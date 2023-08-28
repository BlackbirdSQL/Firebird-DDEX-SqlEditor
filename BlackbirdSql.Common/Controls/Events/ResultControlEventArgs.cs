#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.ResultsPane;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor
namespace BlackbirdSql.Common.Controls.Events;


public class ResultControlEventArgs : EventArgs
{
	public AbstractResultsPanel ResultsControl { get; set; }

	public ResultControlEventArgs(AbstractResultsPanel resultsControl)
	{
		ResultsControl = resultsControl;
	}
}
