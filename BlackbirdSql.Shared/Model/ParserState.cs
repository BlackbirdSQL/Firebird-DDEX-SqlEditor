// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ParserState



namespace BlackbirdSql.Shared.Model;

public class ParserState
{
	public enum EnStatus
	{
		Success,
		Abort,
		Error
	}

	public enum EnErrorType
	{
		NoError,
		Unknown,
		VariableNotFound,
		SyntaxError,
		CommandAborted
	}

	private readonly EnStatus _StatusValue;

	private readonly EnErrorType _ErrorTypeValue;

	private readonly int _Line;

	private readonly string _Info;

	public string Info => _Info;

	public EnErrorType ErrorTypeValue => _ErrorTypeValue;

	public int Line => _Line;

	public EnStatus StatusValue => _StatusValue;

	private EnStatus TranslateStatus(ParserState ps)
	{
		return ps.StatusValue;
	}

	private unsafe EnErrorType TranslateErrorType(ParserState ps)
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
