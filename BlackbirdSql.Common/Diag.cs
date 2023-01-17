using System;
using System.IO;

namespace BlackbirdSql.Common
{
	/* Provides 3 levels of diagnostics
	 * 0. Exceptions only	(_EnableTrace = false, _EnableDiagnostics = false)
	 *		Only Diag.Dug exceptions are processed
	 * 1. Debug				(_EnableTrace = false, _EnableDiagnostics = true)
	 *		Only Diag.Dug calls are processed
	 * 2. Full trace		(_EnableTrace = true, _EnableDiagnostics = true)
	 *		Both Diag.Dug and Diag.Trace calls are processed
	 *		
	 *	Use Diag.Dug for exceptions and in a localized region where you are debugging
	 *	Use Diag.Trace freely wherever you want to log a trace and then _EnableTrace
	 *	specifically to perform a trace
	 *		
	 */
	public static class Diag
	{
		// Specify your own trace log file and settings here
		static bool _EnableTrace = true;
		static bool _EnableDiagnostics = true;
		static bool _EnableWriteLog = true;
		static string _LogFile = "C:\\bin\\vsdiag.log";


		public static bool EnableTrace
		{
			get { return _EnableTrace; }
			set { _EnableTrace = value; }
		}

		public static bool EnableDiagnostics
		{
			get { return _EnableDiagnostics; }
			set { _EnableDiagnostics = value; }
		}

		public static bool EnableWriteLog
		{
			get { return _EnableWriteLog; }
			set { _EnableWriteLog = value; }
		}

		public static string LogFile
		{
			get { return _LogFile; }
			set { _LogFile = value; }
		}



#if DEBUG
		public static void Dug(bool isException = false, string message = "Debug trace",
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
			if (!isException && !EnableDiagnostics && !EnableTrace)
				return;


			int pos = sourceFilePath.IndexOf("\\BlackbirdSql");

			if (pos != -1)
				sourceFilePath = sourceFilePath[(pos+1)..];

			string str = (isException ? "EXCEPTION: " : "T0005: ") + DateTime.Now.ToString("hh.mm.ss.ffffff") + ":   "
				+ memberName + " :: " + sourceFilePath + " :: " + sourceLineNumber + Environment.NewLine
				+ "\t" + message + Environment.NewLine;

#if DEBUG
			// Remove conditional for a full trace
			try
			{
				if (EnableWriteLog)
				{

					StreamWriter sw = File.AppendText(LogFile);

					sw.WriteLine(str);

					sw.Close();
				}
			}
			catch (Exception) { }
#endif

			System.Diagnostics.Debug.WriteLine(str, "BlackbirSql");
		}

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
			if (!isException && !EnableDiagnostics && !EnableTrace)
				return;

			Dug(isException, ex?.Message + (message != "" ? (" " + message) : "") + ":" + Environment.NewLine + ex?.StackTrace?.ToString(),
				memberName, sourceFilePath, sourceLineNumber);

		}

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
			Dug(true, ex?.Message + (message != "" ? (" " + message) : "") + ":" + Environment.NewLine + ex?.StackTrace?.ToString(),
				memberName, sourceFilePath, sourceLineNumber);

		}


		// Trace methods

#if DEBUG
		public static void Trace(string message = "Debug trace",
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
			if (!EnableTrace)
				return;

			Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
		}




	}

}
