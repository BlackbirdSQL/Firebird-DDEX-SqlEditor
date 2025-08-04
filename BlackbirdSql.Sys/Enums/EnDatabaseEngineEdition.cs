// Microsoft.SqlServer.ConnectionInfo, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Common.DatabaseEngineEdition

namespace BlackbirdSql.Sys.Enums;

public enum EnDatabaseEngineEdition
{
	Unknown,
	Personal,
	Standard,
	Enterprise,
	Express,
	SqlDatabase,
	SqlDataWarehouse,
	SqlStretchDatabase,
	SqlManagedInstance,
	SqlDatabaseEdge,
	SqlAzureArcManagedInstance,
	SqlOnDemand
}
