// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.RpcContracts;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;

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

	static bool _EnableTaskLog = true;

	// Specify your own trace log file and settings here or override in VS options
	static bool _EnableTrace = true;
	static bool _EnableDiagnostics = true;
	static bool _EnableFbDiagnostics = true;
	static bool _EnableDiagnosticsLog = true;
	static string _LogFile = "C:\\bin\\vsdiag.log";
	static string _FbLogFile = "C:\\bin\\vsdiagfb.log";

	static string _Context = "APP";

	static IVsOutputWindowPane _OutputPane = null;
	static readonly string _OutputPaneName = "BlackbirdSql";
	static Guid _OutputPaneGuid = default;


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
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog
	{
		get { return _EnableTaskLog; }
		set { _EnableTaskLog = value; }
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
	public static bool EnableDiagnosticsLog
	{
		get { return _EnableDiagnosticsLog; }
		set { _EnableDiagnosticsLog = value; }
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
	public static void Dug(bool isException, string message = "",
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
			if (_EnableDiagnosticsLog)
			{

				StreamWriter sw = File.AppendText(logfile);

				sw.WriteLine(str);

				sw.Close();
			}
		}
		catch (Exception) { }

		try
		{
			if (_EnableTaskLog)
				OutputPaneWriteLine(str);
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
	/// Diagnostics method for full stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Stack(string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
#else
	public static void Stack(string message = "",
		string memberName = "Release:Unavailable",
		string sourceFilePath = "Release:Unavailable",
		int sourceLineNumber = 0)
#endif
	{
		if (message != "")
			message += ":";

		message += Environment.NewLine + "TRACE: " + Environment.StackTrace.ToString();

		Dug(true, message, memberName, sourceFilePath, sourceLineNumber);

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



	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches TaskHandlerProgressAsync from a thread in the thread pool so that it
	/// can switch to the UI thread and be clear to update the IDE task handler progress
	/// bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool TaskHandlerProgress(ITaskHandlerClient client, string text, int progress, bool completed = false)
	{
		_ = Task.Factory.StartNew(() => TaskHandlerProgressAsync(client, text, progress, completed).Result,
				default, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
				TaskScheduler.Default);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static async Task<bool> TaskHandlerProgressAsync(ITaskHandlerClient client, string text, int progress, bool completed = false)
	{
		try
		{
			ITaskHandler taskHandler = client.GetTaskHandler();

			if (taskHandler == null)
				return false;

			TaskProgressData progressData = client.GetProgressData();

			bool updateTaskHandler = (progressData.PercentComplete == null || progressData.PercentComplete != progress);

			string title = EnableTaskLog ? taskHandler.Options.Title.Replace("BlackbirdSql", "").Trim() : "";

			// Switch to main thread
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			// Check again since joining UI thread.
			if (updateTaskHandler)
				taskHandler = client.GetTaskHandler();

			updateTaskHandler &= taskHandler != null;

			if (!EnableTaskLog && !updateTaskHandler)
				return false;


			if (updateTaskHandler)
				progressData.PercentComplete = progress;


			string[] arr = text.Split('\n');


			for (int i = 0; i < arr.Length; i++)
			{
				if (updateTaskHandler)
				{
					progressData.ProgressText = arr[i];
					taskHandler.Progress.Report(progressData);
				}

				if (EnableTaskLog)
					arr[i] = title + ": " + arr[i];
			}


			if (EnableTaskLog)
			{
				text = string.Join("\n", arr);

				if (completed)
				{
					StringBuilder sb = new(arr[^1].Length);

					sb.Append('=', (arr[^1].Length - 10) / 2);
					sb.Append(" Finished ");
					sb.Append('=', arr[^1].Length - sb.Length);

					text += "\n" + sb + "\n";
				}

				_ = OutputPaneWriteLineAsync(text);
			}
		}
		catch (Exception ex)
		{
			bool enableTaskLog = _EnableTaskLog;
			_EnableTaskLog = false;
			Dug(ex);
			_EnableTaskLog = enableTaskLog;

			throw ex;
		}

		return true;
	}



	// Deadlock warning message suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
		Justification = "Code logic ensures a deadlock cannot occur")]
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches UpdateStatusBarAsync from a thread in the thread pool so that it can
	/// switch to the UI thread and be clear to update the IDE status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool UpdateStatusBar(string value, bool clear)
	{
		_ = Task.Factory.StartNew(() => UpdateStatusBarAsync(value, clear).Result, default,
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
			bool enableTaskLog = _EnableTaskLog;
			_EnableTaskLog = false;
			Dug(ex);
			_EnableTaskLog = enableTaskLog;

			throw ex;
		}

		return true;
	}



	/// <summary>
	/// Writes the given text followed by a new line to the Output window pane.
	/// </summary>
	/// <param name="value">The text value to write.</param>
	public static void OutputPaneWriteLine(string value)
	{
		ThreadHelper.JoinableTaskFactory.Run(async () =>
		{
			await OutputPaneWriteLineAsync(value);
		});
	}



	/// <summary>
	/// Writes the given text followed by a new line to the Output window pane.
	/// </summary>
	/// <param name="value">The text value to write. May be an empty string, in which case a newline is written.</param>
	public static async Task OutputPaneWriteLineAsync(string value = null)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		await EnsureOutputPaneAsync();

		if (_OutputPane == null)
		{
			NullReferenceException ex = new("OutputWindowPane is null");

			bool enableTaskLog = _EnableTaskLog;
			_EnableTaskLog = false;
			Dug(ex);
			_EnableTaskLog = enableTaskLog;

			throw ex;
		}

		value ??= string.Empty;


		if (_OutputPane is IVsOutputWindowPaneNoPump noPump)
		{
			try
			{
				noPump.OutputStringNoPump(value + Environment.NewLine);
			}
			catch (Exception ex)
			{
				bool enableTaskLog = _EnableTaskLog;
				_EnableTaskLog = false;
				Dug(ex);
				_EnableTaskLog = enableTaskLog;

				throw ex;
			}
		}
		else
		{
			try
			{
				_OutputPane.OutputStringThreadSafe(value + Environment.NewLine);
			}
			catch (Exception ex)
			{
				bool enableTaskLog = _EnableTaskLog;
				_EnableTaskLog = false;
				Dug(ex);
				_EnableTaskLog = enableTaskLog;

				throw ex;
			}
		}
	}





	static async Task EnsureOutputPaneAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (_OutputPane == null)
		{
			IVsOutputWindow outputWindow;

			try
			{
				outputWindow = await ServiceProvider.GetGlobalServiceAsync<SVsOutputWindow, IVsOutputWindow>(swallowExceptions: false);
				Assumes.Present(outputWindow);
			}
			catch (Exception ex)
			{
				Dug(ex);
				throw;
			}

			_OutputPaneGuid = Guid.NewGuid();

			const int visible = 1;
			const int clearWithSolution = 1;

			try
			{
				outputWindow.CreatePane(ref _OutputPaneGuid, _OutputPaneName, visible, clearWithSolution);
			}
			catch (Exception ex)
			{
				Dug(ex);
				throw;
			}

			try
			{ 
				outputWindow.GetPane(ref _OutputPaneGuid, out _OutputPane);
			}
			catch (Exception ex)
			{
				Dug(ex);
				throw;
			}
		}
	}

	#endregion Methods

}
