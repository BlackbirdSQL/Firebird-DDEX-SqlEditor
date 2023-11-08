// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.Parser
using System;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Interfaces;


namespace BlackbirdSql.Common.Model.Parser;

public class ManagedBatchParser : IDisposable
{
	EnParseMode _ParseMode = EnParseMode.RecognizeAll;

	private IBCommandExecuter _Executor = null;
	private IBVariableResolver _Resolver = null;
	private IBBatchSource _BatchSource = null;

	private bool _SubstitutionEnabled = true;
	private bool _RecognizeOnlyVariables = false;
	private string _Delimiter = null;

	/*
	private bool IsDisposed()
	{
		return true;
	}
	*/

	public ManagedBatchParser()
	{
	}

	~ManagedBatchParser()
	{
		ParserDispose();
	}


	public void ParserDispose()
	{
	}

	public void Cleanup()
	{
	}

	public void SetParseMode(EnParseMode pm)
	{
		_ParseMode = pm;
		_ = _ParseMode;
	}

	public void SetRecognizeOnlyVariables(bool bRecognizeOnlyVariables)
	{
		_RecognizeOnlyVariables = bRecognizeOnlyVariables;
		_ = _RecognizeOnlyVariables;
	}

	public void SetBatchDelimiter(string strBatchDelimiter)
	{
		_Delimiter = strBatchDelimiter;
		_ = _Delimiter;
	}

	public void DisableVariableSubstitution()
	{
		_SubstitutionEnabled = false;
		_ = _SubstitutionEnabled;
	}

	public void SetBatchSource(IBBatchSource pIBatchSource)
	{
		_BatchSource = pIBatchSource;
	}

	public void SetCommandExecuter(IBCommandExecuter pICommandExecuter)
	{
		_Executor = pICommandExecuter;
	}

	public void SetVariableResolver(IBVariableResolver pIVariableResolver)
	{
		_Resolver = pIVariableResolver;
		_ = _Resolver;
	}

	public bool Parse()
	{
		string script = "";

		_BatchSource.GetMoreData(ref script);

		_Executor.ProcessBatch(script, 1);

		return true;
	}

	public int GetLastCommandLineNumber()
	{
		// 1 for now
		return 0;
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (isDisposing)
			ParserDispose();
	}

	public virtual void Dispose()
	{
		Dispose(true);
		// GC.SuppressFinalize(this);
	}
}
