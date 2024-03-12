// Microsoft.SqlServer.Management.SqlParser, Version=16.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlParser.Binder.IBinder
using System.Collections.Generic;
using BlackbirdSql.Common.Ctl.Parser.Enums;
using BlackbirdSql.Common.Model.Parsers.Interfaces;

namespace BlackbirdSql.Common.Ctl.Parser.Interfaces;

public interface IBBinder
{
	IBServer Bind(IEnumerable<ParseResult> parseResults, string contextDatabaseName, EnBindMode bindMode);
}
