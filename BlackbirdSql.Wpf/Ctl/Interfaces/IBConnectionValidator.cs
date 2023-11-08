// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IConnectionValidator

using System.Data;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Wpf.Ctl.Interfaces;

public interface IBConnectionValidator : IBExportable
{
	void CheckConnection(IDbConnection conn);
}
