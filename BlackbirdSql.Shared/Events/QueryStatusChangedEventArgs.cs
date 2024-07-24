// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusChangedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusChangedEventArgs

using System;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void QueryStatusChangedEventHandler(object sender, QueryStatusChangedEventArgs args);


public class QueryStatusChangedEventArgs : EventArgs
{
	public QueryStatusChangedEventArgs(EnQueryStatusFlags statusFlag, bool enabled, bool newConnection) : base()
	{
		StatusFlag = statusFlag;
		Enabled = enabled;
		NewConnection = newConnection;
	}

	public EnQueryStatusFlags StatusFlag { get; private set; }
	public bool Enabled { get; private set; }
	public bool NewConnection { get; private set; }
}