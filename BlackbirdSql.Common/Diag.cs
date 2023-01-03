using System;
using System.IO;

namespace BlackbirdSql.Common
{

	public static class Diag
	{
		// Specify your own trace log file here
		const string _logFile = "C:\\bin\\vsdiag.log";

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
				if (true || isException)
				{

					StreamWriter sw = File.AppendText(_logFile);

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
			Dug(isException, ex.Message + (message != "" ? (" " + message) : "") + ":" + Environment.NewLine + ex.StackTrace.ToString(),
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
			Dug(true, ex.Message + (message != "" ? (" " + message) : "") + ":" + Environment.NewLine + ex.StackTrace.ToString(),
				memberName, sourceFilePath, sourceLineNumber);

		}
#if DEBUG
		public static void Dug(string message,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
		public static void Dug(string message,
			string memberName = "Release:Unavailable",
			string sourceFilePath = "Release:Unavailable",
			int sourceLineNumber = 0)
#endif
		{
			Dug(false, message, memberName, sourceFilePath, sourceLineNumber);
		}
	}

}
