// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ISmoDatabaseObject

using Microsoft.SqlServer.Management.Smo;


namespace BlackbirdSql.LanguageExtension.Interfaces;

public interface IBsSmoDatabaseObject
{
	SqlSmoObject SmoObject { get; }
}
