#region Assembly Microsoft.Data.SqlClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.SqlClient.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.Serialization;

namespace BlackbirdSql.Common.Ctl.Exceptions;


[Serializable]
public sealed class SqlError
{
	private readonly string _source = "Framework Microsoft SqlClient Data Provider";

	private readonly int _number;

	private readonly byte _State;

	private readonly byte _errorClass;

	[OptionalField(VersionAdded = 2)]
	private readonly string _server;

	private readonly string _message;

	private readonly string _procedure;

	private readonly int _lineNumber;

	[OptionalField(VersionAdded = 4)]
	private readonly int _win32ErrorCode;

	public string Source => _source;

	public int Number => _number;

	public byte State => _State;

	public byte Class => _errorClass;

	public string Server => _server;

	public string Message => _message;

	public string Procedure => _procedure;

	public int LineNumber => _lineNumber;

	public int Win32ErrorCode => _win32ErrorCode;

	public SqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber, uint win32ErrorCode)
		: this(infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber)
	{
		_win32ErrorCode = (int)win32ErrorCode;
	}

	public SqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber)
	{
		_number = infoNumber;
		_State = errorState;
		_errorClass = errorClass;
		_server = server;
		_message = errorMessage;
		_procedure = procedure;
		_lineNumber = lineNumber;
		if (errorClass != 0)
		{
			// SqlClientEventSource.Log.TryTraceEvent("<sc.SqlError.SqlError|ERR> infoNumber={0}, errorState={1}, errorClass={2}, errorMessage='{3}', procedure='{4}', lineNumber={5}", infoNumber, (int)errorState, (int)errorClass, errorMessage, procedure, lineNumber);
		}

		_win32ErrorCode = 0;
	}

	public override string ToString()
	{
		return typeof(SqlError).ToString() + ": " + _message;
	}
}
