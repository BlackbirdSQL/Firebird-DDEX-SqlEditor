// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.ResultControlEventArgs

using System;
using BlackbirdSql.Common.Controls.ResultsPanels;


namespace BlackbirdSql.Common.Controls.Events;

public class ResultControlEventArgs : EventArgs
{
	public AbstractResultsPanel ResultsControl { get; set; }

	public ResultControlEventArgs(AbstractResultsPanel resultsControl)
	{
		ResultsControl = resultsControl;
	}
}
