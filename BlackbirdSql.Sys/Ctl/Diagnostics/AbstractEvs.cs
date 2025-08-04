// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.TraceUtils

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI.Design.WebControls;
using BlackbirdSql.Sys.Ctl.Config;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Sys.Ctl.Diagnostics;


/// <summary>
/// First phase implementation of an EventSource wrapper of the original SqlEditor TraceUtils
/// static methods. EventSource is unhooked atm. It will all be hooked up once Telemetry
/// functionality has been introduced.
/// The class family uses a type parameter/specifier of the descendent class for creating the
/// object so that we can have a separate singleton instance for each assembly using minimal
/// implementation code in the final class.
/// </summary>
/// <typeparam name="TEvs"></typeparam>
public class AbstractEvs<TEvs> : EvsProvider<TEvs> where TEvs : AbstractEvs<TEvs>
{
	protected AbstractEvs(string identifier) : base(identifier)
	{
	}


	public static void Debug(Type type, string method,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		TraceLogEvent(EnEvsId.Debug, EnEventLevel.Debug, type, method, "", line, memberName, sourcePath);
	}


	public static void Debug(Type type, string method, string msg,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		TraceLogEvent(EnEvsId.Debug, EnEventLevel.Debug, type, method, msg, line, memberName, sourcePath);
	}



	public static void Error(Type type, string method, string msg)
	{
		TraceLogEvent(EnEvsId.Error, EnEventLevel.Error, type, method, "", -1, "", "", msg);
	}



	public static void Info(Type type, string method, string msg)
	{
		TraceLogEvent(EnEvsId.Information, EnEventLevel.Information, type, method, "", -1, "", "", msg);
	}



	private static void LogTrace(EnEvsId eventId, EnEventLevel eventLevel, int index, Type type,
		string method, string msg, int line, string memberName, string sourcePath)
	{
		string filename = !string.IsNullOrEmpty(sourcePath) ? Cmd.GetFileName(sourcePath) : null;

		if (line == -1 || string.IsNullOrEmpty(memberName) || string.IsNullOrEmpty(filename))
		{

			MethodBase stackMethodBase;

			string stackMemberName;
			string stackFilename;
			int stackLine;

			StackFrame frame;

			try
			{
				frame = new StackTrace(3, true).GetFrame(0);
			}
			catch
			{
				frame = null;
			}

			if (frame != null)
			{
				stackMethodBase = frame.GetMethod();
				stackMemberName = stackMethodBase?.Name;
				stackFilename = frame.GetFileName();
				stackLine = frame != null ? frame.GetFileLineNumber() : -1;

				if (string.IsNullOrEmpty(memberName))
					memberName = stackMemberName;

				if (string.IsNullOrEmpty(filename))
					filename = stackFilename;

				if (line == -1)
					line = stackLine;
			}
		}

		if (string.IsNullOrEmpty(memberName))
			memberName = null;

		if (string.IsNullOrEmpty(filename))
			filename = null;


		msg = !string.IsNullOrEmpty(msg) && msg[0] != '\n' ? (": " + msg) : msg;

		string strEvent;

		if (eventLevel != EnEventLevel.ActivityTracing)
			strEvent = eventLevel.ToString().ToUpperInvariant();
		else if (eventId == EnEvsId.OpStop)
			strEvent = eventLevel.ToString().ToUpperInvariant() + $"(#{Log.TelemCardinals[index]})";
		else
			strEvent = "ACTIVITY" + eventId.ToString().ToUpperInvariant();

		memberName ??= "UNK_METHOD";
		filename ??= "UNK_SOURCEFILE";

		Diag.LogEvs($"[{strEvent}] {type.FullName}::{method}{msg}", line, memberName, filename, eventLevel);
	}



	public static void Resume(Type type, string method, int index)
	{
		string name = method.TrimSuffix("()");

		TraceLogEvent(EnEvsId.OpResume, EnEventLevel.ActivityTracing, type, method, "", -1, "", "", name, index);
	}



	public static void Resume(Type type, string method, string name, int index)
	{
		TraceLogEvent(EnEvsId.OpResume, EnEventLevel.ActivityTracing, type, method, "", -1, "", "", name, index);
	}



	public static int Start(Type type, string method, string msg, string name, int index)
	{
		return TraceLogEvent(EnEvsId.OpStart, EnEventLevel.ActivityTracing, type, method, "", -1, "", "", name, index);
	}



	public static void Stop(Type type, string method, string msg, string name, int index)
	{
		TraceLogEvent(EnEvsId.OpStop, EnEventLevel.ActivityTracing, type, method, msg, -1, "", "", name, index);
	}



	public static void Suspend(Type type, string method, string msg, string name, int index)
	{
		TraceLogEvent(EnEvsId.OpSuspend, EnEventLevel.ActivityTracing, type, method, msg, -1, "", "", name, index);
	}



	public static void Trace(Type type, string method)
	{
		TraceLogEvent(EnEvsId.Trace, EnEventLevel.Trace, type, method, "", -1, "", "");
	}



	public static void Trace(Type type, string method, string msg)
	{
		TraceLogEvent(EnEvsId.Trace, EnEventLevel.Trace, type, method, msg, -1, "", "");
	}



	private static int TraceLogEvent(EnEvsId eventId, EnEventLevel eventLevel, Type type,
		string method, string msg, int line, string memberName, string sourcePath, params object[] args)
	{
		int result = VSConstants.E_NOTIMPL;

		try
		{
			if (!CanTrace(eventLevel))
				return result;

			if (eventLevel == EnEventLevel.ActivityTracing && eventId != EnEvsId.OpStart && Convert.ToInt32(args[1]) < 0)
				return result;

			if (string.IsNullOrEmpty(memberName))
				memberName = method;

			string componentName = type.FullName;

			StringBuilder sb = new StringBuilder();
			sb.Append(componentName ?? "null");
			sb.Append("::");
			sb.Append(memberName ?? "null");
			if (line != -1)
				sb.Append($"[#{line}]");
			sb.Append(" - ");
			if (msg == "")
				sb.Append("null");
			else
				sb.Append(msg);

			string evsMsg = sb.ToString();

			switch (eventId)
			{
				case EnEvsId.Critical:
					result = Log.CriticalEvent(evsMsg);
					break;
				case EnEvsId.Error:
					result = Log.ErrorEvent(evsMsg);
					break;
				case EnEvsId.Warning:
					result = Log.WarnEvent(evsMsg);
					break;
				case EnEvsId.Information:
					result = Log.InfoEvent(evsMsg);
					break;
				case EnEvsId.Trace:
					result = Log.TraceEvent(evsMsg);
					break;
				case EnEvsId.Debug:
					result = Log.DebugEvent(evsMsg);
					break;
				case EnEvsId.OpStart:
					result = Log.StartEvent((string)args[0], Convert.ToInt32(args[1]), evsMsg);
					break;
				case EnEvsId.OpStop:
					result = Log.StopEvent((string)args[0], Convert.ToInt32(args[1]), evsMsg);
					break;
				case EnEvsId.OpSuspend:
					result = Log.SuspendEvent((string)args[0], Convert.ToInt32(args[1]), evsMsg);
					break;
				case EnEvsId.OpResume:
					result = Log.ResumeEvent((string)args[0], Convert.ToInt32(args[1]), evsMsg);
					break;
				default:
					break;
			}

			if ((eventLevel != EnEventLevel.ActivityTracing && PersistentSettings.EnableEventSourceLogging)
				|| (eventLevel == EnEventLevel.ActivityTracing && PersistentSettings.EnableActivityLogging))
			{
				LogTrace(eventId, eventLevel, result, type, method, msg, line, memberName, sourcePath);
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}

		return result;
	}



	public static void Warning(Type type, string method, string msg,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		TraceLogEvent(EnEvsId.Warning, EnEventLevel.Warning, type, method, msg, line, memberName, sourcePath);
	}
}
