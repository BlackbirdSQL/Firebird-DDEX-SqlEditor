// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ParserState



namespace BlackbirdSql.Common.Model;

public class ParserState
{
	public enum Status
	{
		Success,
		Abort,
		Error
	}

	public enum ErrorType
	{
		NoError,
		Unknown,
		VariableNotFound,
		SyntaxError,
		CommandAborted
	}

	private readonly Status _StatusValue;

	private readonly ErrorType _ErrorTypeValue;

	private readonly int _Line;

	private readonly string _Info;

	public string Info => _Info;

	public ErrorType ErrorTypeValue => _ErrorTypeValue;

	public int Line => _Line;

	public Status StatusValue => _StatusValue;

	private Status TranslateStatus(ParserState ps)
	{
		return ps.StatusValue;
	}

	private unsafe ErrorType TranslateErrorType(ParserState ps)
	{
		return ps.ErrorTypeValue;
	}

	public ParserState(ParserState parserState)
	{
		_StatusValue = TranslateStatus(parserState);
		_ErrorTypeValue = TranslateErrorType(parserState);
		_Line = parserState._Line;
		_Info = parserState._Info;
	}
}
