// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IBatchSource

using BlackbirdSql.Common.Model.Enums;

namespace BlackbirdSql.Common.Model.Interfaces;

public interface IBBatchSource
{
	EnParserAction GetMoreData(ref string str);
}
