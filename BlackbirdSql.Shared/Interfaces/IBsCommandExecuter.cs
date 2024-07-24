// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ICommandExecuter


using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Shared.Interfaces;

public interface IBsCommandExecuter
{
	Task<EnParserAction> BatchStatementCallbackAsync(IBsNativeDbStatementWrapper statement, int numberOfTimes, CancellationToken cancelToken);

	void OnBatchDataLoaded(object sender, QueryDataEventArgs eventArgs);
	void OnBatchScriptParsed(object sender, QueryDataEventArgs eventArgs);

}
