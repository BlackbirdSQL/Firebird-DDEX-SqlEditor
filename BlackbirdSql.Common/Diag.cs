using System;
using System.IO;



namespace BlackbirdSql.Common;


// =========================================================================================================
//											Diag Class
//
/// <summary>
/// BlackbirdSql Diagnostics reporter
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
/// Use Diag.Trace freely wherever you want to log a trace and then _EnableTrace
/// specifically to perform a trace
/// </remarks>
// =========================================================================================================
public static class Diag
{
	#region Variables


	// Specify your own trace log file and settings here or override in VS options
	static bool _EnableTrace = true;
	static bool _EnableDiagnostics = true;
	static bool _EnableFbDiagnostics = true;
	static bool _EnableWriteLog = true;
	static string _LogFile = "C:\\bin\\vsdiag.log";
	static string _FbLogFile = "C:\\bin\\vsdiagfb.log";

	static string _Context = "APP";

	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - Diag
	// =========================================================================================================


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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Trace"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTrace
	{
		get { return _EnableTrace; }
		set { _EnableTrace = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Dug"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnostics
	{
		get { return _EnableDiagnostics; }
		set { _EnableDiagnostics = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Firebird debug library diagnostics calls are logged
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableFbDiagnostics
	{
		get { return _EnableFbDiagnostics; }
		set { _EnableFbDiagnostics = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableWriteLog
	{
		get { return _EnableWriteLog; }
		set { _EnableWriteLog = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile
	{
		get { return _LogFile; }
		set { _LogFile = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The Firebird log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FbLogFile
	{
		get { return _FbLogFile; }
		set { _FbLogFile = value; }
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - Diag
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The common Diag diagnostics method
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Dug(bool isException = false, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
	public static void Dug(bool isException = false, string message = "Debug trace",
		string memberName = "Release:Unavailable",
		string sourceFilePath = "Release:Unavailable",
		int sourceLineNumber = 0)
#endif
	{
		if (!isException && !_EnableDiagnostics && !_EnableTrace)
			return;

		int pos;
		string logfile = _LogFile;


		if ((pos = sourceFilePath.IndexOf("\\BlackbirdSql")) == -1)
		{
			if ((pos = sourceFilePath.IndexOf("\\FirebirdSql")) == -1)
				pos = sourceFilePath.IndexOf("\\EntityFramework.Firebird");

			if (pos != -1)
			{
				if (!isException && !_EnableFbDiagnostics)
					return;
				logfile = _FbLogFile;
			}
		}


		if (pos != -1)
			sourceFilePath = sourceFilePath[(pos + 1)..];

		string str = _Context + ":" + (isException ? ":EXCEPTION: " : " ") + DateTime.Now.ToString("hh.mm.ss.ffffff") + ":   "
			+ memberName + " :: " + sourceFilePath + " :: " + sourceLineNumber +
			(message == "" ? "" : Environment.NewLine + "\t" + message) + Environment.NewLine;

#if DEBUG
		// Remove conditional for a full trace
		try
		{
			if (_EnableWriteLog)
			{

				StreamWriter sw = File.AppendText(logfile);

				sw.WriteLine(str);

				sw.Close();
			}
		}
		catch (Exception) { }
#endif

		System.Diagnostics.Debug.WriteLine(str, "BlackbirSql");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method that can flag a call as an Exception even if the Exception parameter is null
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Dug(bool isException, Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
	public static void Dug(bool isException, Exception ex, string message = "",
		string memberName = "Release:Unavailable",
		string sourceFilePath = "Release:Unavailable",
		int sourceLineNumber = 0)
#endif
	{
		if (!isException && !_EnableDiagnostics && !_EnableTrace)
			return;

		Dug(isException, ex?.Message + (message != "" ? " " + message : "") + ":" + Environment.NewLine + ex?.StackTrace?.ToString(),
			memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Dug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
	public static void Dug(Exception ex, string message = "",
		string memberName = "Release:Unavailable",
		string sourceFilePath = "Release:Unavailable",
		int sourceLineNumber = 0)
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

		Dug(true, ex.Message + " " + message, memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Trace(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
	public static void Trace(string message = "Debug trace",
		string memberName = "Release:Unavailable",
		string sourceFilePath = "Release:Unavailable",
		int sourceLineNumber = 0)
#endif
	{
		if (!_EnableTrace)
			return;

		Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
	}


	#endregion Methods

}
