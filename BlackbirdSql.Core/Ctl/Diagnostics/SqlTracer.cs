// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.SqlTracer
#define TRACE
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Core.Ctl.Diagnostics;


internal static class SqlTracer
{
	private const string C_TraceSourceName = "BlackbirdSql.Core.Diagnostics.Tracer";

	private const int C_MessageChunkLength = 1754;

	private static TraceSource _traceSource;

	private static TraceSource TraceSource
	{
		get
		{
			if (_traceSource != null)
			{
				return _traceSource;
			}
			TraceSource value = new TraceSource(C_TraceSourceName);
			Interlocked.CompareExchange(ref _traceSource, value, null);
			return _traceSource;
		}
	}

	public static IBSqlTraceTelemetryProvider SqlTraceTelemetryProvider { get; set; }

	private static EnWinEventTracingLevel TraceEventTypeToLevel(TraceEventType eventType)
	{
		switch (eventType)
		{
			case TraceEventType.Critical:
				return EnWinEventTracingLevel.Critical;
			case TraceEventType.Error:
				return EnWinEventTracingLevel.Error;
			case TraceEventType.Information:
			case TraceEventType.Start:
			case TraceEventType.Stop:
			case TraceEventType.Suspend:
			case TraceEventType.Resume:
			case TraceEventType.Transfer:
				return EnWinEventTracingLevel.Informational;
			case TraceEventType.Verbose:
				return EnWinEventTracingLevel.Verbose;
			case TraceEventType.Warning:
				return EnWinEventTracingLevel.Warning;
			default:
				return EnWinEventTracingLevel.Always;
		}
	}

	private static bool WriteEtwEvent(TraceEventType eventType, EnSqlTraceId traceId, string message)
	{
		return TraceEventTypeToLevel(eventType) switch
		{
			EnWinEventTracingLevel.Error => SqlEtwProvider.EventWriteLogError((uint)traceId, message),
			EnWinEventTracingLevel.Warning => SqlEtwProvider.EventWriteLogWarning((uint)traceId, message),
			EnWinEventTracingLevel.Informational => SqlEtwProvider.EventWriteLogInformational((uint)traceId, message),
			EnWinEventTracingLevel.Verbose => SqlEtwProvider.EventWriteLogVerbose((uint)traceId, message),
			_ => SqlEtwProvider.EventWriteLogCritical((uint)traceId, message),
		};
	}

	internal static bool ShouldTrace(TraceEventType eventType)
	{
		if (!TraceSource.Switch.ShouldTrace(eventType))
		{
			return SqlEtwProvider.IsLoggingEnabled(TraceEventTypeToLevel(eventType));
		}
		return true;
	}

	internal static bool TraceEvent(TraceEventType eventType, EnSqlTraceId traceId)
	{
		return TraceEvent(eventType, traceId, string.Empty);
	}

	internal static bool TraceEvent(TraceEventType eventType, EnSqlTraceId traceId, string message)
	{
		bool flag = true;
		TraceSource.TraceEvent(eventType, (int)traceId, message);
		TraceSource.Flush();
		if (message == null || message.Length <= C_MessageChunkLength)
		{
			flag = WriteEtwEvent(eventType, traceId, message);
		}
		else
		{
			int num = message.Length / C_MessageChunkLength;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				flag &= WriteEtwEvent(eventType, traceId, message.Substring(num2, C_MessageChunkLength));
				num2 += C_MessageChunkLength;
			}
			if (num2 < message.Length)
			{
				flag &= WriteEtwEvent(eventType, traceId, message[num2..]);
			}
		}
		return flag;
	}

	internal static bool TraceEvent(TraceEventType eventType, EnSqlTraceId traceId, string format, params object[] args)
	{
		return TraceEvent(eventType, traceId, string.Format(CultureInfo.CurrentCulture, format, args));
	}

	internal static bool TraceException(EnSqlTraceId traceId, Exception exception, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return TraceException(TraceEventType.Error, traceId, exception, string.Empty, lineNumber, fileName, memberName);
	}

	internal static bool TraceException(EnSqlTraceId traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return TraceException(TraceEventType.Error, traceId, exception, message, lineNumber, fileName, memberName);
	}

	internal static bool TraceException(TraceEventType eventType, EnSqlTraceId traceId, Exception exception, string message = null, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		string messageForException = GetMessageForException(exception);
		SqlTraceTelemetryProvider?.PostEvent(eventType, traceId, exception, lineNumber, fileName, memberName);
		if (message == null)
			return TraceEvent(eventType, traceId, messageForException);
		else
			return TraceEvent(eventType, traceId, string.Format(CultureInfo.CurrentCulture, "{0} {1}", message, messageForException));

	}

	private static string GetMessageForException(Exception exception)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (exception is FbException)
		{
			FbException obj = exception as FbException;
			stringBuilder.AppendLine("FbException:  Message = " + exception.Message);
			if (!string.IsNullOrWhiteSpace(exception.StackTrace))
			{
				stringBuilder.AppendLine("               StackTrace = " + exception.StackTrace);
			}
			stringBuilder.AppendLine("Errors:");
			foreach (FbError error in obj.Errors)
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "Number = {0}, State = {1}, Server = {2}, Message = {3}",
					error.Number, obj.GetState(), obj.GetServer(), error.Message);
			}
		}
		else
		{
			stringBuilder.AppendLine("Exception:  Message = " + exception.Message);
			if (!string.IsNullOrWhiteSpace(exception.StackTrace))
			{
				stringBuilder.AppendLine("               StackTrace = " + exception.StackTrace);
			}
		}
		return stringBuilder.ToString();
	}

	internal static bool TraceHResult(EnSqlTraceId traceId, int hr)
	{
		return TraceHResult(traceId, hr, string.Empty);
	}

	internal static bool TraceHResult(EnSqlTraceId traceId, int hr, string message)
	{
		Exception exceptionForHR = Marshal.GetExceptionForHR(hr);
		if (exceptionForHR != null)
		{
			return TraceException(traceId, exceptionForHR, "HRESULT failure: " + message, 323, "SqlTracer.cs", "TraceHResult");
		}
		return false;
	}

	internal static bool DebugTraceEvent(TraceEventType eventType, EnSqlTraceId traceId, string message)
	{
		return TraceEvent(eventType, traceId, message);
	}

	internal static bool AssertTraceEvent(bool condition, TraceEventType eventType, EnSqlTraceId traceId, string message)
	{
		if (!condition)
		{
			return DebugTraceEvent(eventType, traceId, message);
		}
		return true;
	}

	internal static bool DebugTraceException(TraceEventType eventType, EnSqlTraceId traceId, Exception exception, string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string memberName = "")
	{
		return TraceException(eventType, traceId, exception, message, lineNumber, fileName, memberName);
	}

	internal static bool AssertTraceException(bool condition, TraceEventType eventType, EnSqlTraceId traceId, Exception exception, string message)
	{
		if (!condition)
		{
			return DebugTraceException(eventType, traceId, exception, message, 397, "D:\\a\\_work\\1\\s\\src\\VSSQL\\Microsoft.Data.Tools.Components\\Diagnostics\\SqlTracer.cs", "AssertTraceException");
		}
		return true;
	}
}
