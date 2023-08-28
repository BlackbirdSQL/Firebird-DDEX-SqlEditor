// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ICommandExecuter2
using BlackbirdSql.Common.Model.Enums;

namespace BlackbirdSql.Common.Model.Interfaces;

public interface ICommandExecuter2 : ICommandExecuter
{
	EnParserAction Connect(int timeout, string server, string user, string password, bool encyptConnection, bool trustServerCertificate);
}
