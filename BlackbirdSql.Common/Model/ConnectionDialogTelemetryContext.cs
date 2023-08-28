// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ConnectionDialogTelemetryContext

using System.Collections.Generic;




namespace BlackbirdSql.Common.Model;

public abstract class ConnectionDialogTelemetryContext : AbstractTelemetryPropertyAgent
{


	public ConnectionDialogTelemetryContext()
	{
	}

	public ConnectionDialogTelemetryContext(Dictionary<string, object> properties)
	{
	}



	public abstract void EnsureContextProperties();
}
