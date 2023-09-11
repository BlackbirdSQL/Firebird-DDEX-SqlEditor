// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.Parser
using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Threading;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Model;

public class ManagedBatchParser : IDisposable
{
	EnParseMode _ParseMode = EnParseMode.RecognizeAll;

	private ICommandExecuter _Executor = null;
	private IVariableResolver _Resolver = null;
	private IBatchSource _BatchSource = null;

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

	public void  SetBatchSource(IBatchSource pIBatchSource)
	{
		_BatchSource = pIBatchSource;
	}

	public void SetCommandExecuter(ICommandExecuter pICommandExecuter)
	{
		_Executor = pICommandExecuter;
	}

	public void SetVariableResolver(IVariableResolver pIVariableResolver)
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
