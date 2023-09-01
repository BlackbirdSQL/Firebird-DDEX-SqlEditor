// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ParserException

using System;
using BlackbirdSql.Common.Model;

namespace BlackbirdSql.Common.Exceptions;

public class ParserException : Exception
{
	private readonly ParserState _ParserStateValue;

	public ParserState ParserStateValue => _ParserStateValue;

	public unsafe ParserException(ParserState ps)
		: base("")
	{
		_ParserStateValue = new ParserState(ps);
	}
}
