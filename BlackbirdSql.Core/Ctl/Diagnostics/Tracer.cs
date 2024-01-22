// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Impl.Tracer
// combined with
// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.TraceUtils
#define TRACE
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl.Diagnostics;

// [Exportable(typeof(IBTrace), "BlackbirdSql.Core.Diagnostics.Tracer", 0, null)]
internal class Tracer : IBTrace // , IBExportable
{
	public enum EnLevel : uint
	{
		Warning = 2u,
		Error = 1u,
		Information = 3u,
		Verbose = 4u
	}

	private static readonly TraceSource Source = new TraceSource("BlackbirdSql.Core.Diagnostics.Tracer");

	// public IBExportableMetadata Metadata { get; set; }

	// public IBDependencyManager DependencyManager { get; set; }

	/*
	public ExportableStatus Status
	{
		get
		{
			if (DependencyManager == null || Metadata == null)
			{
				return new ExportableStatus
				{
					LoadingFailed = true
				};
			}
			return new ExportableStatus();
		}
	}
	*/


	private static string GetMessageForException(Exception exception)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (exception is SqlException)
		{
			SqlException obj = exception as SqlException;
			stringBuilder.AppendLine("SqlException:  Message = " + exception.Message);
			if (!string.IsNullOrWhiteSpace(exception.StackTrace))
			{
				stringBuilder.AppendLine("               StackTrace = " + exception.StackTrace);
			}
			stringBuilder.AppendLine("Errors:");
			foreach (SqlError error in obj.Errors)
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "Number = {0}, State = {1}, Server = {2}, Message = {3}", error.Number, error.State, error.Server, error.Message);
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



	private static TraceEventType GetTraceEventTypeForTraceLevel(EnLevel tracelevel)
	{
		TraceEventType result = TraceEventType.Information;
		switch (tracelevel)
		{
			case EnLevel.Error:
				result = TraceEventType.Error;
				break;
			case EnLevel.Warning:
				result = TraceEventType.Warning;
				break;
			case EnLevel.Information:
				result = TraceEventType.Information;
				break;
			case EnLevel.Verbose:
				result = TraceEventType.Verbose;
				break;
		}
		return result;
	}



	public static void Information(Type t, string functionName, string format, params object[] args)
	{
		Trace(t, EnLevel.Information, functionName, format, args);
	}



	public static void LogExCatch(Type t, Exception e,
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
	{
		LogExCatch(t, e, string.Empty, sourceLineNumber, memberName, sourceFilePath);
	}


	public static void LogExCatch(Type t, Exception e, string msg,
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
	{
		Diag.Dug(e, msg, memberName, sourceFilePath, sourceLineNumber);
		EnSqlTraceId traceIdForType = EnSqlTraceId.SqlEditorAndLanguageServices;
		SqlTracer.TraceEvent(TraceEventType.Error, traceIdForType, "caught exception:  " + e.Message);
		SqlTracer.TraceException(traceIdForType, e, msg, 100, "Tracer.cs", "LogExCatch");
	}


	public static void LogExThrow(Type t, Exception e,
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
	{
		Diag.Dug(e, string.Empty, memberName, sourceFilePath, sourceLineNumber);
		SqlTracer.TraceException(EnSqlTraceId.SqlEditorAndLanguageServices, e, 76, "TraceUtils.cs", "LogExThrow");
	}

	public static void LogExThrow(Type t, string message,
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
	{
		Diag.Dug(true, message, memberName, sourceFilePath, sourceLineNumber);
		EnSqlTraceId traceIdForType = EnSqlTraceId.SqlEditorAndLanguageServices;
		SqlTracer.TraceEvent(TraceEventType.Error, traceIdForType, message);
	}


	public static void Trace(Type t, string functionName)
	{
		Trace(t, functionName, "");
	}
	public static void Trace(string type, string functionName)
	{
		Trace(type, functionName, "");
	}


	public static void Trace(Type t, string functionName, string format, params object[] args)
	{
		Trace(t.FullName, functionName, format, args);
	}

	public static void Trace(string type, string functionName, string format, params object[] args)
	{
		if (SqlTracer.ShouldTrace(TraceEventType.Information))
		{
			Trace(type, TraceEventType.Information, functionName, format, args);
		}
		else if (PersistentSettings.EnableTracer)
		{
			StackFrame frame = null;

			for (int i = 1; i < 4; i++)
			{
				frame = new StackTrace(i, true).GetFrame(0);
				if (frame == null || !frame.GetFileName().EndsWith("\\Tracer.cs", StringComparison.OrdinalIgnoreCase)
					|| (frame.GetMethod().Name != "Trace" && frame.GetMethod().Name != "Warning"
					&& frame.GetMethod().Name != "Information"))
				{
					break;
				}
			}

			Diag.Dug(false, type + " func: " + functionName
				+ (format != null ? args != null ? (" " + string.Format(format, args)) : (" " + format) : ""),
				frame == null ? "ERRNULL" : frame.GetMethod().Name, frame == null ? "ERRNULL" : frame.GetFileName(),
				frame == null ? int.MinValue : frame.GetFileLineNumber());
		}

	}


	public static void Trace(Type t, EnLevel traceLevel, string functionName, string format, params object[] args)
	{
		Trace(t.FullName, traceLevel, functionName, format, args);
	}

	public static void Trace(string type, EnLevel traceLevel, string functionName, string format, params object[] args)
	{
		TraceEventType traceEventTypeForTraceLevel = GetTraceEventTypeForTraceLevel(traceLevel);
		if (SqlTracer.ShouldTrace(traceEventTypeForTraceLevel))
		{
			Trace(type, traceEventTypeForTraceLevel, functionName, format, args);
		}
		else if (PersistentSettings.EnableTracer)
		{
			StackFrame frame = null;

			for (int i = 1; i < 4; i++)
			{
				frame = new StackTrace(i, true).GetFrame(0);
				if (frame == null || !frame.GetFileName().EndsWith("\\Tracer.cs", StringComparison.OrdinalIgnoreCase)
					|| (frame.GetMethod().Name != "Trace" && frame.GetMethod().Name != "Warning"
					&& frame.GetMethod().Name != "Information"))
				{
					break;
				}
			}

			Diag.Dug(false, type + " func: " + functionName
				+ (format != null ? args != null ? (" " + string.Format(format, args)) : (" " + format) : ""),
				frame == null ? "ERRNULL" : frame.GetMethod().Name, frame == null ? "ERRNULL" : frame.GetFileName(),
				frame == null ? int.MinValue : frame.GetFileLineNumber());
		}
	}


	private static void Trace(string type, TraceEventType eventType, string functionName, string format, params object[] args)
	{
		if (PersistentSettings.EnableTracer)
		{
			StackFrame frame = null;

			for (int i = 1; i < 4; i++)
			{
				frame = new StackTrace(i, true).GetFrame(0);
				if (frame == null || !frame.GetFileName().EndsWith("\\Tracer.cs", StringComparison.OrdinalIgnoreCase)
					|| (frame.GetMethod().Name != "Trace" && frame.GetMethod().Name != "Warning"
					&& frame.GetMethod().Name != "Information"))
				{
					break;
				}
			}

			Diag.Dug(false, type + " func: " + functionName
				+ (format != null ? args != null ? (" " + string.Format(format, args)) : (" " + format) : ""),
				frame == null ? "ERRNULL" : frame.GetMethod().Name, frame == null ? "ERRNULL" : frame.GetFileName(),
				frame == null ? int.MinValue : frame.GetFileLineNumber());
		}

		if (SqlTracer.ShouldTrace(eventType))
		{
			EnSqlTraceId traceIdForType = EnSqlTraceId.SqlEditorAndLanguageServices;
			string componentNameForType = type;
			Trace(eventType, traceIdForType, componentNameForType, functionName, format, args);
		}
	}


	private static void Trace(TraceEventType eventType, EnSqlTraceId traceId, string componentName, string functionName, string format, object[] args)
	{
		if (SqlTracer.ShouldTrace(eventType))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(componentName ?? "null");
			stringBuilder.Append("::");
			stringBuilder.Append(functionName ?? "null");
			stringBuilder.Append(" - ");
			if (format == null)
			{
				stringBuilder.Append("null");
			}
			else if (args != null)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, format, args));
			}
			else
			{
				stringBuilder.Append(format);
			}
			SqlTracer.TraceEvent(eventType, traceId, stringBuilder.ToString());
		}
	}



	public bool TraceEvent(TraceEventType eventType, int traceId, string message, params object[] args)
	{
		Source.TraceEvent(eventType, traceId, message, args);
		Source.Flush();
		return true;
	}

	internal static bool TraceEvent(TraceEventType eventType, EnSqlTraceId traceId, string format, params object[] args)
	{
		Source.TraceEvent(eventType, (int)traceId, HashLog.Format(CultureInfo.CurrentCulture, format, args));
		Source.Flush();
		return true;
	}



	public bool TraceException(TraceEventType eventType, int traceId, Exception exception, string message, int lineNumber = 0, string fileName = "", string memberName = "")
	{
		string messageForException = GetMessageForException(exception);
		return TraceEvent(eventType, traceId, string.Format(CultureInfo.CurrentCulture, "{0} {1}", message, messageForException));
	}



	public static void Warning(Type t, string functionName, string format, params object[] args)
	{
		Trace(t, EnLevel.Warning, functionName, format, args);
	}

}
