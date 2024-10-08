﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql;


// =========================================================================================================
//											Diag Class
//
/// <summary>
/// BlackbirdSql Diagnostics, TaskHandler, StatusBar and OutputWindow reporter
/// </summary>
/// <remarks>
/// Provides 3 levels of diagnostics
///		0. Exceptions only	(_EnableTrace = false, _EnableDiagnostics = false)
///			Only Diag.Dug exceptions are processed.
///		1. Debug	(_EnableTrace = false, _EnableDiagnostics = true)
///			Only Diag.Dug calls are processed
///		2. Full trace	(_EnableTrace = true, _EnableDiagnostics = true)
///			Both Diag.Dug and Diag.Trace calls are processed
///
/// Use Diag.Dug for exceptions and in a localized region where you are debugging
/// Use Diag.Trace freely wherever you want to log a trace and set the EnableTrace option
/// specifically to perform a trace
/// </remarks>
// =========================================================================================================
public static class Diag
{

	// ---------------------------------------------------------------------------------
	#region Fields
	// ---------------------------------------------------------------------------------


	// A static class lock
	private static readonly object _LockGlobal = new();
	private static int _InternalActive = 0;
	private static int _TaskLogActive = 0;
	private static int _IgnoreSettings = 0;

	static string _Context = "APP";

	static IVsOutputWindowPane _OutputPane = null;
	static readonly string _OutputPaneName = "BlackbirdSql";
	static Guid _OutputPaneGuid = default;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - Diag
	// =========================================================================================================



	// All diagnostics settings are configurable from VS Options.

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the context the call was made from. Either 'IDE' or 'APP'
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string Context
	{
		get
		{
			return _Context;
		}
		set
		{
			_Context = value;
		}
	}


	private static bool _SettingEnableTaskLog = true;
	public static bool SettingEnableTaskLog
	{
		set { _SettingEnableTaskLog = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog => _TaskLogActive == 0
		&& (_InternalActive > 0 || _SettingEnableTaskLog);


	private static bool _SettingEnableTrace = true;
	public static bool SettingEnableTrace
	{
		set { _SettingEnableTrace = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Trace"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTrace => _SettingEnableTrace;





	private static bool _SettingEnableTracer = true;
	public static bool SettingEnableTracer
	{
		set { _SettingEnableTracer = value; }
	}

	public static bool EnableTracer => _SettingEnableTracer;





	private static bool _SettingEnableDiagnostics = true;
	public static bool SettingEnableDiagnostics
	{
		set { _SettingEnableDiagnostics = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Dug"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnostics => _SettingEnableDiagnostics;


	private static bool _SettingEnableDiagnosticsLog = true;
	public static bool SettingEnableDiagnosticsLog
	{
		set { _SettingEnableDiagnosticsLog = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnosticsLog => _InternalActive > 0 || _SettingEnableDiagnosticsLog;


	private static string _SettingLogFile = "/temp/vsdiag.log";
	public static string SettingLogFile
	{
		set { _SettingLogFile = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => _SettingLogFile;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - Diag
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The common Diag diagnostics method
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Dug(bool isException, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Dug(bool isException = false, string message = "Debug trace",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (_IgnoreSettings == 0 && !isException && !EnableDiagnostics && !EnableTrace)
			return;

		int pos;
		string logfile = LogFile;
		string str;

		bool enableDiagnosticsLog;
		bool enableTaskLog;

		lock (_LockGlobal)
		{
#if DEBUG
			enableDiagnosticsLog = EnableDiagnosticsLog || (_IgnoreSettings > 0);
#else
			enableDiagnosticsLog = EnableDiagnosticsLog;
#endif
			enableTaskLog = EnableTaskLog || (_IgnoreSettings > 0);

			try
			{
				if ((pos = sourceFilePath.IndexOf("\\BlackbirdSql")) == -1)
					pos = sourceFilePath.IndexOf("\\BlackbirdDsl");


				if (pos != -1)
					sourceFilePath = sourceFilePath[(pos + 1)..];

				if (_TaskLogActive > 0)
				{
					str = DateTime.Now.ToString("hh.mm.ss.ffffff") + ":   " + message;
				}
				else
				{
					string prefix = isException ? ":EXCEPTION: " : " ";

					str = _Context + ":" + prefix + DateTime.Now.ToString("hh.mm.ss.ffffff") + ":   "
						+ memberName + " :: " + sourceFilePath + " :: " + sourceLineNumber +
						(message == "" ? "" : Environment.NewLine + "\t" + message) + Environment.NewLine;
				}
			}
			catch (Exception ex)
			{
				str = $"{ex.Message} sourceFilePath: {sourceFilePath} memberName: {memberName} sourceLineNumber: {sourceLineNumber} message: {message}";
			}

			try
			{
				if (enableDiagnosticsLog)
				{
					StreamWriter sw = File.AppendText(logfile);

					sw.WriteLine(str);

					sw.Close();
				}
			}
			catch (Exception ex)
			{
				str = "DIAG EXCEPTION: " + ex.Message + Environment.NewLine + str;
			}
		}

		try
		{
			if (enableTaskLog)
				AsyuiOutputPaneWriteLine(str, isException);
		}
		catch (Exception) { }


		System.Diagnostics.Debug.WriteLine(str, "BlackbirSql");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Dug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1,
		int exceptionType = 0)
#else
	public static void Dug(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		message ??= "";

		if (message != "")
			message += ":";

		if (ex is DbException exd && exd.HasSqlException())
		{
			message += Environment.NewLine + $"{NativeDb.DbEngineName} error code: {exd.GetErrorCode()}, Class: {exd.GetClass()}, Proc: {exd.GetProcedure()}, Line: {exd.GetLineNumber()}.";
		}
		if (ex.StackTrace != null)
		{
			message += Environment.NewLine + "TRACE: " + ex.StackTrace.ToString();
		}
		else
		{
			message += Environment.NewLine + "TRACE: " + Environment.StackTrace.ToString();
		}

		string prefix = exceptionType == 1
			? $"[DEBUG {ex.GetType()}] "
			: (exceptionType == 2 ? $"[EXPECTED {ex.GetType()}] " : $"[{ex.GetType()}] ");

		Dug(true, prefix + ex.Message + " " + message, memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static ServiceUnavailableException ExceptionService(Type type,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		ServiceUnavailableException ex = new(type);
		Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);

		return ex;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises excepton for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static async Task<TResult> ThrowExceptionServiceUnavailableAsync<TResult>(Type serviceType,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
		where TResult : class
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (serviceType != null)
			throw ExceptionService(serviceType, memberName, sourceFilePath, sourceLineNumber);

		return await Task.FromResult<TResult>(null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises excepton for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void ThrowIfServiceUnavailable(object service, Type type,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (service == null)
			throw ExceptionService(type, memberName, sourceFilePath, sourceLineNumber);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for TypeAccessException
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static TypeAccessException ExceptionInstance(Type type,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		TypeAccessException ex = new($"The singleton instance for {type.FullName} has not been initialized.");
		Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);

		return ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Throws an exception for TypeAccessException
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void ThrowIfInstanceNull(object instance, Type type,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (instance == null)
			throw ExceptionInstance(type, memberName, sourceFilePath, sourceLineNumber);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for OnUiThread
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static object ThrowException(Exception ex,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void ThrowException(Exception ex,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);

		throw ex;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for ErrorHandler.ThrowOnFailure
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static int ThrowOnFailure(int hr,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static int ThrowOnFailure(int hr,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		try
		{
			return ErrorHandler.ThrowOnFailure(hr);
		}
		catch (Exception ex)
		{
			Stack();
			// Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);
			throw ex;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for OnUiThread
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static COMException ExceptionThreadOnUI(
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		string message = string.Format(CultureInfo.CurrentCulture, "{0} may NOT be called on the UI thread.", memberName);
		COMException ex = new(message, VSConstants.RPC_E_WRONG_THREAD);

		Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);

		return ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises an exception if OnUiThread.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void ThrowIfOnUIThread(
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (ThreadHelper.CheckAccess())
			throw ExceptionThreadOnUI(memberName, sourceFilePath, sourceLineNumber);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for NotOnUiThread
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static COMException ExceptionThreadNotUI(
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		string message = string.Format(CultureInfo.CurrentCulture, "{0} must be called on the UI thread.", memberName);
		COMException ex = new(message, VSConstants.RPC_E_WRONG_THREAD);

		Dug(ex, "", memberName, sourceFilePath, sourceLineNumber);

		return ex;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Logs and throws an exception if NotOnUiThread and prevents an unecessary UI
	/// thread trail by Intellisense.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void ThrowIfNotOnUIThread(
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static ServiceUnavailableException ExceptionService(Type type,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (!ThreadHelper.CheckAccess())
			throw ExceptionThreadNotUI(memberName, sourceFilePath, sourceLineNumber);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for outputting unexpected Exceptions that will display on
	/// DEBUG builds but be swallowed in release builds.
	/// For example an object that is unavailable on shutdown but that could possibly
	/// have been checked, or a ConnectionSource state that has not yet been
	/// identified.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Debug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Expected(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
#if DEBUG
		Dug(ex, message, memberName, sourceFilePath, sourceLineNumber, 1);
#endif
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for outputting expected Exceptions that will display on
	/// DEBUG builds but be swallowed in release builds.
	/// For example a connection that throws an exception due to a broken network
	/// connection or a temporarily inaccessible tab for text updates. 
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Expected(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Expected(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
#if DEBUG
		Dug(ex, message, memberName, sourceFilePath, sourceLineNumber, 2);
#endif
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Temporary Diagnostics method for Exceptions only.
	/// For easy identification of temporary try/catch statements during debugging.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Tug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Tug(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		Dug(ex, message, memberName, sourceFilePath, sourceLineNumber);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only during debug WITHOUT secondary exceptions
	/// to prevent recursion or when package is not sited yet.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void DebugDug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void DebugDug(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (message != "")
			message += ":";

		if (ex.StackTrace != null)
		{
			message += Environment.NewLine + "TRACE: " + ex.StackTrace.ToString();
		}
		else
		{
			message += " NO STACKTRACE";
		}

		lock (_LockGlobal)
			_InternalActive++;

		_IgnoreSettings++;

		try
		{
			Dug(true, ex.Message + " " + message, memberName, sourceFilePath, sourceLineNumber);
		}
		catch { }

		_IgnoreSettings--;

		lock (_LockGlobal)
			_InternalActive--;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full information stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Stack(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Stack(string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (message != "")
			message += ":";

		message += (message != "" ? Environment.NewLine : "") + "INFORMATION TRACE: " + Environment.StackTrace.ToString();

		lock (_LockGlobal)
			_InternalActive++;

		_IgnoreSettings++;

		try
		{
			Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
		}
		catch { }

		_IgnoreSettings--;

		lock (_LockGlobal)
			_InternalActive--;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full exception stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void StackException(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Stack(Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (message != "")
			message += ":";

		message ??= "";

		if (message != "")
			message += ":";


		if (ex is DbException exd && exd.HasSqlException())
		{
			message += Environment.NewLine + $"{NativeDb.DbEngineName}  error code:  {exd.GetErrorCode()} , Class:  {exd.GetClass()}, Proc: {exd.GetProcedure()}, Line: {exd.GetLineNumber()}.";
		}

		message += Environment.NewLine + "TRACE: " + Environment.StackTrace.ToString();

		Dug(true, message, memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full exception stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void StackException(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Stack(string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (message != "")
			message += ":";

		message += (message != "" ? Environment.NewLine : "") + "TRACE: " + Environment.StackTrace.ToString();

		Dug(true, message, memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug WITHOUT secondary exceptions
	/// to prevent recursion or when package is not sited yet
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void DebugTrace(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void DebugTrace(string message = "Debug trace",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		// if (!EnableTrace)
		//	return;


		lock (_LockGlobal)
			_InternalActive++;

		_IgnoreSettings++;

		try
		{
			Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
		}
		catch { }

		_IgnoreSettings--;

		lock (_LockGlobal)
			_InternalActive--;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug WITHOUT secondary exceptions to
	/// prevent recursion or when package is not sited yet
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void DebugWarning(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void DebugWarning(string message = "Debug trace",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		// if (!EnableTrace)
		//	return;


		lock (_LockGlobal)
			_InternalActive++;

		_IgnoreSettings++;

		try
		{
			Dug(false, "WARNING: " + message, memberName, sourceFilePath, sourceLineNumber);
		}
		catch { }

		_IgnoreSettings--;

		lock (_LockGlobal)
			_InternalActive--;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if !NEWDEBUG
	public static void Trace(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Trace(string message = "Debug trace",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (!EnableTrace)
			return;

		Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
	}


	
	public static void Trace(Type classType, string method, DataExplorerNodeEventArgs e, EnConnectionSource connectionSource, bool onMainThread, bool initialized)
	{
		if (e.Node != null && e.Node.ExplorerConnection != null
			&& e.Node.ExplorerConnection.ConnectionNode != null
			&& e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode))
		{
			try
			{
				IVsDataExplorerNode node = e.Node;
				IVsDataExplorerConnection explorerConnection = node.ExplorerConnection;

				string str = $"\nExplorerConnection.DisplayName: {explorerConnection.DisplayName}, ExplorerConnection type: {explorerConnection.GetType().FullName}"
					+ $"\n\tItemId: {node.ItemId}, Node.Name: {node.Name}, NodeType: {node}, ConnectionState: {explorerConnection.Connection.State}, Connectionsource: {connectionSource}, Initialized: {initialized}, OnMainThread: {onMainThread}, "
					+ $"\n\t\tHasBeenExpanded: {node.HasBeenExpanded}, "
					+ $"\n\t\tIsExpandable: {node.IsExpandable}, IsExpanding: {node.IsExpanding}, IsRefreshing: {node.IsRefreshing}, "
					+ $"\n\t\tIsDiscarded: {node.IsDiscarded}, IsExpanded: {node.IsExpanded}, IsPlaced: {node.IsPlaced}, IsVisible: {node.IsVisible}";

				if (Reflect.GetFieldValueBase(node, "_object") != null)
				{
					string datasetKey = (string)(node.Object.Properties != null
						&& node.Object.Properties.ContainsKey("DatasetKey")
						? node.Object.Properties["DatasetKey"] : "Null");
					string connectionKey = (string)(node.Object.Properties != null
						&& node.Object.Properties.ContainsKey("ConnectionKey")
						? node.Object.Properties["ConnectionKey"] : "Null");

					EnConnectionSource objectConnectionSource = (EnConnectionSource)(int)(node.Object.Properties != null
						&& node.Object.Properties.ContainsKey("ConnectionSource")
						? node.Object.Properties["ConnectionSource"] : EnConnectionSource.Unknown);

					str += $"\n\tObject.Name: {node.Object.Name}, Object.DatasetKey: {datasetKey}, Object.ConnectionSource: {objectConnectionSource}, Object.ConnectionKey: {connectionKey}, Object.Type: {node.Object.Type.Name}, Object.IsDeleted: {node.Object.IsDeleted}.";
				}
				else
				{
					str += "\n\tNode.Object is null.";
				}

				Tracer.Information(classType, method, str);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches TaskHandlerProgressAsync from a thread in the thread pool so that it
	/// can switch to the UI thread and be clear to update the IDE task handler progress
	/// bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool TaskHandlerProgress(IBsTaskHandlerClient client, string text, bool completed = false)
	{
		// Fire and forget - Switch to threadpool.

		Task.Factory.StartNew(() => TaskHandlerProgressAsync(client, text, completed),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static async Task<bool> TaskHandlerProgressAsync(IBsTaskHandlerClient client, string text, bool completed = false)
	{
		bool enableDiagnosticsLog;
		bool enableTaskLog;

		lock (_LockGlobal)
		{
			enableDiagnosticsLog = EnableDiagnosticsLog;
			enableTaskLog = EnableTaskLog;
		}

		try
		{
			// Switch to main thread
			// await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


			ITaskHandler taskHandler = client.GetTaskHandler();
			TaskProgressData progressData = client.GetProgressData();
			string title;

			lock (_LockGlobal)
			{
				title = enableTaskLog && taskHandler != null ? taskHandler.Options.Title.Replace("BlackbirdSql", "").Trim() + ": " : "";

				// Check again since joining UI thread.
				taskHandler = client.GetTaskHandler();
			}

			if (!enableTaskLog && taskHandler == null)
				return await Task.FromResult(false);

			string[] arr = text.Split('\n');

			lock (_LockGlobal)
			{
				if (enableTaskLog)
				{
					string[] log = new string[arr.Length];

					for (int i = 0; i < arr.Length; i++)
						log[i] = title + arr[i];


					text = string.Join("\n", log);

					if (completed)
					{
						StringBuilder sb = new(log[^1].Length);

						sb.Append('=', (log[^1].Length - 10) / 2);
						sb.Append(" Done ");
						sb.Append('=', log[^1].Length - sb.Length);

						text += "\n" + sb + "\n";
					}

					_ = OutputPaneWriteLineAsync(text, false);

				}

				// Check again.
				taskHandler = client.GetTaskHandler();
			}

			if (taskHandler == null)
				return await Task.FromResult(enableTaskLog);

			lock (_LockGlobal)
			{
				for (int i = 0; i < arr.Length; i++)
				{
					progressData.ProgressText = arr[i];
					taskHandler.Progress.Report(progressData);
				}
			}

		}
		catch (Exception ex)
		{
			lock (_LockGlobal)
			{
				_TaskLogActive++;
				Dug(ex);
				_TaskLogActive--;
			}

			throw ex;
		}

		return await Task.FromResult(true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches UpdateStatusBarAsync from a thread in the thread pool so that it can
	/// switch to the UI thread and be clear to update the IDE status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool UpdateStatusBar(string value, bool clear)
	{
		// Fire and discard remember.

		_ = Task.Factory.StartNew(() => UpdateStatusBarAsync(value, clear), default,
			TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default);

		return true;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static async Task<bool> UpdateStatusBarAsync(string value, bool clear)
	{
		// Switch to main thread
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		try
		{
			IVsStatusbar statusBar = await ServiceProvider.GetGlobalServiceAsync<SVsStatusbar, IVsStatusbar>(swallowExceptions: false);

			// statusBar.FreezeOutput(0);

			if (clear && value == null)
			{
				statusBar.Clear();
			}
			else
			{
				statusBar.SetText(value);

				if (clear)
				{
					// Fire and wait.

					_ = Task.Run(async delegate
					{
						await Task.Delay(4000);
						_ = UpdateStatusBarAsync(null, true);
					});
				}
			}
		}
		catch (Exception ex)
		{
			lock (_LockGlobal)
			{
				_TaskLogActive++;
				Dug(ex);
				_TaskLogActive--;
			}

			throw ex;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Async on UI thread]: Writes the given text followed by a new line to the Output
	/// window pane.
	/// </summary>
	/// <param name="value">The text value to write.</param>
	// ---------------------------------------------------------------------------------
	public static void AsyuiOutputPaneWriteLine(string value, bool isException)
	{
		string outputMsg = value;
		_ = Task.Run(() => OutputPaneWriteLineAsync(outputMsg, isException));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Writes the given text followed by a new line to the Output window pane.
	/// </summary>
	/// <param name="value">
	/// The text value to write. May be an empty string, in which case a newline is written.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static async Task OutputPaneWriteLineAsync(string value, bool isException)
	{
		string outputMsg = value;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (_OutputPane == null)
			await EnsureOutputPaneAsync();

		if (_OutputPane == null)
		{
			NullReferenceException ex = new("OutputWindowPane is null");

			if (_IgnoreSettings == 0)
			{
				lock (_LockGlobal)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}
			}

			throw ex;
		}

		outputMsg ??= "";


		try
		{
			if (_OutputPane is IVsOutputWindowPaneNoPump noPump)
				noPump.OutputStringNoPump(outputMsg + Environment.NewLine);
			else
				_OutputPane.OutputStringThreadSafe(outputMsg + Environment.NewLine);
		}
		catch (Exception ex)
		{
			if (_IgnoreSettings == 0)
			{

				lock (_LockGlobal)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}
			}

			throw ex;
		}

		if (isException && _OutputPane != null)
			_OutputPane.Activate();

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the BlackbirdSql output pane.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static async Task EnsureOutputPaneAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (_OutputPane == null)
		{
			IVsOutputWindow outputWindow;

			try
			{
				outputWindow = await ServiceProvider.GetGlobalServiceAsync<SVsOutputWindow, IVsOutputWindow>(swallowExceptions: false);

				if (outputWindow == null)
					throw new ServiceUnavailableException(typeof(IVsOutputWindow));
			}
			catch (Exception ex)
			{
				if (_IgnoreSettings == 0)
				{
					lock (_LockGlobal)
					{
						_TaskLogActive++;
						Dug(ex);
						_TaskLogActive--;
					}
				}
				throw;
			}

			if (_OutputPaneGuid == default)
				_OutputPaneGuid = new(LibraryData.C_OutputPaneGuid);

			const int visible = 1;
			const int clearWithSolution = 1;

			try
			{
				outputWindow.CreatePane(ref _OutputPaneGuid, _OutputPaneName, visible, clearWithSolution);
			}
			catch (Exception ex)
			{
				if (_IgnoreSettings == 0)
				{
					lock (_LockGlobal)
					{
						_TaskLogActive++;
						Dug(ex);
						_TaskLogActive--;
					}
				}
				throw;
			}

			try
			{
				outputWindow.GetPane(ref _OutputPaneGuid, out _OutputPane);
			}
			catch (Exception ex)
			{
				if (_IgnoreSettings == 0)
				{
					lock (_LockGlobal)
					{
						_TaskLogActive++;
						Dug(ex);
						_TaskLogActive--;
					}
				}
				throw;
			}
		}
	}

#endregion Methods

}
