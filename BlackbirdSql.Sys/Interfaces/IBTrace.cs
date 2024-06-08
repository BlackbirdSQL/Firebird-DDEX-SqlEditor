// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ITrace

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;



namespace BlackbirdSql.Sys.Interfaces;


public interface IBTrace // : IBExportable
{
	bool TraceEvent(TraceEventType eventType, int traceId, string message, params object[] args);

	bool TraceException(TraceEventType eventType, int traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "");
}
