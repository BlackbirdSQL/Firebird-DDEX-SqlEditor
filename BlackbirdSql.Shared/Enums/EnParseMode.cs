// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ParseMode

namespace BlackbirdSql.Shared.Enums;

internal enum EnParseMode
{
	RecognizeAll,
	RecognizeOnlyVariables,
	RecognizeOnlyBatchDelimiter
}
