// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ParserException

using System.Data.Common;



namespace BlackbirdSql.Core.Model.Exceptions;


public class SqlException(string message) : DbException(message)
{
}
