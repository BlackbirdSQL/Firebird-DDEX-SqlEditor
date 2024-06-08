// SqlWorkbench.Interfaces, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.IObjectWithSite
using System;


namespace BlackbirdSql.Shared.Interfaces;

public interface IBObjectWithSite
{
	void SetSite(IServiceProvider sp);
}
