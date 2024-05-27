// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.TraceableBase

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Sys;

namespace BlackbirdSql.Core.Ctl.Diagnostics;

public abstract class AbstractTraceableBase
{
	public abstract IBTrace Trace { get; set; }

	public bool TraceEvent(TraceEventType eventType, EnUiTraceId traceId, string format, params object[] args)
	{
		return TraceEvent(eventType, (int)traceId, format, args);
	}

	public bool TraceEvent(TraceEventType eventType, int traceId, string format, params object[] args)
	{
		return SafeTrace(eventType, traceId, format, args);
	}

	public bool AssertTraceEvent(bool condition, TraceEventType eventType, EnUiTraceId traceId, string message)
	{
		return AssertTraceEvent(condition, eventType, (int)traceId, message);
	}

	public bool AssertTraceEvent(bool condition, TraceEventType eventType, int traceId, string message)
	{
		if (!condition)
		{
			return DebugTraceEvent(eventType, traceId, message);
		}

		return false;
	}

	public bool AssertTraceException(bool condition, TraceEventType eventType, EnUiTraceId traceId, Exception exception, string message)
	{
		return AssertTraceException(condition, eventType, (int)traceId, exception, message);
	}

	public bool AssertTraceException2(bool condition, TraceEventType eventType, EnUiTraceId traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		if (!condition)
		{
			return DebugTraceException2(eventType, (int)traceId, exception, message, lineNumber, fileName, memberName);
		}

		return true;
	}

	public bool AssertTraceException(bool condition, TraceEventType eventType, int traceId, Exception exception, string message)
	{
		if (!condition)
		{
			return DebugTraceException(eventType, traceId, exception, message);
		}

		return true;
	}

	public bool DebugTraceException(TraceEventType eventType, int traceId, Exception exception, string message)
	{
		return TraceException(eventType, traceId, exception, message, 0, string.Empty, string.Empty);
	}

	public bool DebugTraceException2(TraceEventType eventType, int traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return TraceException(eventType, traceId, exception, message, lineNumber, fileName, memberName);
	}

	public bool TraceException(TraceEventType eventType, EnUiTraceId traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return TraceException(eventType, (int)traceId, exception, message, lineNumber, fileName, memberName);
	}

	public bool TraceException(TraceEventType eventType, int traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return SafeTraceException(eventType, traceId, exception, message, lineNumber, fileName, memberName);
	}

	public bool DebugTraceEvent(TraceEventType eventType, int traceId, string message)
	{
		return TraceEvent(eventType, traceId, message);
	}

	private bool SafeTrace(TraceEventType eventType, int traceId, string format, params object[] args)
	{
		if (Trace != null)
		{
			return Trace.TraceEvent(eventType, traceId, format, args);
		}

		return false;
	}

	private bool SafeTraceException(TraceEventType eventType, int traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		if (Trace != null)
		{
			return Trace.TraceException(eventType, traceId, exception, message, lineNumber, fileName, memberName);
		}

		return false;
	}
}
