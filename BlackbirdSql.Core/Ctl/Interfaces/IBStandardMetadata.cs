// Microsoft.SqlServer.Tools.Extensibility, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Tools.Extensibility.IStandardMetadata

namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBStandardMetadata
{

	string Id { get; }

	string DatasetKey { get; }
}
