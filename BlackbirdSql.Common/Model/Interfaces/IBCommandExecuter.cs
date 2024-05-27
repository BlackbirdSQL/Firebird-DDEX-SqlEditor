// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ICommandExecuter


using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.Sys;

namespace BlackbirdSql.Common.Model.Interfaces;

public interface IBCommandExecuter
{
	EnParserAction ProcessParsedBatchStatement(IBsNativeDbStatementWrapper statement, int numberOfTimes);

	EnParserAction Reset();

	EnParserAction Ed(string batch, ref IBsNativeDbBatchParser pIBatchSource);

	EnParserAction ExecuteShellCommand(string command);

	EnParserAction Quit();

	EnParserAction Exit(string batch, string exitBatch);

	EnParserAction IncludeFileName(string fileName, ref IBsNativeDbBatchParser ppIBatchSource);

	EnParserAction ServerList();

	EnParserAction List(string batch);

	EnParserAction ListVar(string varList);

	EnParserAction Error(EnOutputDestination od, string fileName);

	EnParserAction Out(EnOutputDestination od, string fileName);

	EnParserAction PerfTrace(EnOutputDestination od, string fileName);

	EnParserAction Connect(int timeout, string server, string user, string password);

	void OnBatchDataLoaded(object sender, QESQLQueryDataEventArgs eventArgs);

	EnParserAction OnError(EnErrorAction ea);

	EnParserAction Xml(EnXmlStatus xs);

	EnParserAction Help();
}
