// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl.Config;

#if BLACKBIRD
using BlackbirdSql.Core.Ctl.Interfaces;
#endif
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;


#if BLACKBIRD
namespace BlackbirdSql.Core;
#else
namespace FirebirdSql.Data;
#endif

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
	#region Variables


	// A static class lock
	private static readonly object _LockClass = new();
	private static int _InternalActive = 0;
	private static int _TaskLogActive = 0;


	static string _Context = "APP";

	static IVsOutputWindowPane _OutputPane = null;
	static readonly string _OutputPaneName = "BlackbirdSql";
	static Guid _OutputPaneGuid = default;


#endregion Variables





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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog => _InternalActive == 0 && _TaskLogActive == 0
		&& UserSettings.EnableTaskLog;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Trace"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTrace => UserSettings.EnableTrace;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Dug"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnostics => UserSettings.EnableDiagnostics;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Firebird debug library diagnostics calls are logged
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableFbDiagnostics => UserSettings.EnableFbDiagnostics;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableDiagnosticsLog => _InternalActive > 0 || UserSettings.EnableDiagnosticsLog;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => UserSettings.LogFile;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The Firebird log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FbLogFile => UserSettings.FbLogFile;


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
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Dug(bool isException = false, string message = "Debug trace",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

		int pos;
		string logfile = LogFile;
		string str;

		bool enableDiagnosticsLog;
		bool enableTaskLog;

		lock (_LockClass)
		{
			enableDiagnosticsLog = EnableDiagnosticsLog;
			enableTaskLog = EnableTaskLog;

			try
			{
				if ((pos = sourceFilePath.IndexOf("\\BlackbirdSql")) == -1)
				{
					if ((pos = sourceFilePath.IndexOf("\\BlackbirdDsl")) == -1)
					{
						if ((pos = sourceFilePath.IndexOf("\\FirebirdSql")) == -1)
							pos = sourceFilePath.IndexOf("\\EntityFramework.Firebird");

						if (pos != -1)
						{
							if (!isException && !EnableFbDiagnostics)
								return;
							logfile = FbLogFile;
						}
					}
				}


				if (pos != -1)
					sourceFilePath = sourceFilePath[(pos + 1)..];

				str = _Context + ":" + (isException ? ":EXCEPTION: " : " ") + DateTime.Now.ToString("hh.mm.ss.ffffff") + ":   "
					+ memberName + " :: " + sourceFilePath + " :: " + sourceLineNumber +
					(message == "" ? "" : Environment.NewLine + "\t" + message) + Environment.NewLine;
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
				OutputPaneWriteLine(str);
		}
		catch (Exception) { }


		System.Diagnostics.Debug.WriteLine(str, "BlackbirSql");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method that can flag a call as an Exception even if the Exception
	/// parameter is null
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Dug(bool isException, Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Dug(bool isException, Exception ex, string message = "",
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{

		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

		Dug(isException, ex?.Message + (message != "" ? " " + message : "") + ":" + Environment.NewLine + ex?.StackTrace?.ToString(),
			memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method with formatting
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static void Dug(string message, Exception ex,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
#else
	public static void Dug(string message, Exception ex,
		string memberName = "[Release: MemberName Unavailable]",
		string sourceFilePath = "[Release: SourcePath Unavailable]",
		int sourceLineNumber = -1)
#endif
	{
		Dug(true, String.Format(message, ex) + ":" + Environment.NewLine + ex?.StackTrace?.ToString(),
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
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
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
	/// Diagnostics method for Exceptions only during debug WITHOUT tasklog to prevent
	/// recursion or when package is not sited yet
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
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

		lock (_LockClass)
			_InternalActive++;

		Dug(true, ex.Message + " " + message, memberName, sourceFilePath, sourceLineNumber);

		lock (_LockClass)
			_InternalActive--;
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

		message += Environment.NewLine + "TRACE: " + Environment.StackTrace.ToString();

		Dug(true, message, memberName, sourceFilePath, sourceLineNumber);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug WITHOUT tasklog to prevent
	/// recursion or when package is not sited yet
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
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
		if (!EnableTrace)
			return;


		lock (_LockClass)
			_InternalActive++;

		Dug(false, message, memberName, sourceFilePath, sourceLineNumber);

		lock (_LockClass)
			_InternalActive--;

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches TaskHandlerProgressAsync from a thread in the thread pool so that it
	/// can switch to the UI thread and be clear to update the IDE task handler progress
	/// bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool TaskHandlerProgress(IBTaskHandlerClient client, string text, bool completed = false)
	{
		_ = Task.Factory.StartNew(() => TaskHandlerProgressAsync(client, text, completed),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default);

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public static async Task<bool> TaskHandlerProgressAsync(IBTaskHandlerClient client, string text, bool completed = false)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		bool enableDiagnosticsLog;
		bool enableTaskLog;

		lock (_LockClass)
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

			lock (_LockClass)
			{
				title = enableTaskLog && taskHandler != null ? taskHandler.Options.Title.Replace("BlackbirdSql", "").Trim() + ": " : "";

				// Check again since joining UI thread.
				taskHandler = client.GetTaskHandler();

				if (!enableTaskLog && taskHandler == null)
					return false;


				string[] arr = text.Split('\n');

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

					_ = OutputPaneWriteLineAsync(text);
				}

				// Check again.
				taskHandler = client.GetTaskHandler();

				if (taskHandler == null)
					return enableTaskLog;

				for (int i = 0; i < arr.Length; i++)
				{
					progressData.ProgressText = arr[i];
					taskHandler.Progress.Report(progressData);
				}
			}

		}
		catch (Exception ex)
		{
			lock (_LockClass)
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
	/// Launches UpdateStatusBarAsync from a thread in the thread pool so that it can
	/// switch to the UI thread and be clear to update the IDE status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool UpdateStatusBar(string value, bool clear)
	{
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
			lock (_LockClass)
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
	/// Writes the given text followed by a new line to the Output window pane.
	/// </summary>
	/// <param name="value">The text value to write.</param>
	// ---------------------------------------------------------------------------------
	public static void OutputPaneWriteLine(string value)
	{
		_ = Task.Run(() => OutputPaneWriteLineAsync(value));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Writes the given text followed by a new line to the Output window pane.
	/// </summary>
	/// <param name="value">
	/// The text value to write. May be an empty string, in which case a newline is written.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static async Task OutputPaneWriteLineAsync(string value = null)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (_OutputPane == null)
			await EnsureOutputPaneAsync();

		if (_OutputPane == null)
		{
			NullReferenceException ex = new("OutputWindowPane is null");

			lock (_LockClass)
			{
				_TaskLogActive++;
				Dug(ex);
				_TaskLogActive--;
			}

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
				lock (_LockClass)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}

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
				lock (_LockClass)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}

				throw ex;
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the BlackbirdSql output pane.
	/// </summary>
	// ---------------------------------------------------------------------------------
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
				lock (_LockClass)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}
				throw;
			}

			if (_OutputPaneGuid == default)
				_OutputPaneGuid = new(SystemData.OutputPaneGuid);

			const int visible = 1;
			const int clearWithSolution = 1;

			try
			{
				outputWindow.CreatePane(ref _OutputPaneGuid, _OutputPaneName, visible, clearWithSolution);
			}
			catch (Exception ex)
			{
				lock (_LockClass)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}
				throw;
			}

			try
			{
				outputWindow.GetPane(ref _OutputPaneGuid, out _OutputPane);
			}
			catch (Exception ex)
			{
				lock (_LockClass)
				{
					_TaskLogActive++;
					Dug(ex);
					_TaskLogActive--;
				}
				throw;
			}
		}
	}

	#endregion Methods

}
