// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ConnectionDialogTelemetryContext

using System.Collections.Generic;

using BlackbirdSql.Common.Model;




namespace BlackbirdSql.Wpf.Model;

public abstract class AbstractTelemetryContext : AbstractTelemetryPropertyAgent
{


    public AbstractTelemetryContext()
    {
    }

    public AbstractTelemetryContext(Dictionary<string, object> properties)
    {
    }



    public abstract void EnsureContextProperties();
}
