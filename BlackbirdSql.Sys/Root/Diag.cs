// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data.Common;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl.Config;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;
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
///			Only Diag.Ex exceptions are processed.
///		1. Debug	(_EnableTrace = false, _EnableDiagnostics = true)
///			Only Diag.Ex calls are processed
///		2. Full trace	(_EnableTrace = true, _EnableDiagnostics = true)
///			Both Diag.Ex and Diag.Trace calls are processed
///
/// Use Diag.Ex for exceptions and in a localized region where you are debugging
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

	private static string _Context = "APP";
	private static int _IgnoreSettings = 0;
	private static int _InternalActive = 0;
	private static IVsOutputWindowPane _OutputPane = null;
	private static Guid _OutputPaneGuid = default;
	private static readonly string _OutputPaneName = "BlackbirdSql";
	private static int _TaskLogActive = 0;


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
		get { return _Context; }
		set { _Context = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag"/> messages are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool EnableDiagnostics => PersistentSettings.EnableDiagnostics;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool EnableDiagnosticsLog => _InternalActive > 0 || PersistentSettings.EnableDiagnosticsLog;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Expected"/> exceptions are output.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool EnableExpected => _InternalActive > 0 || PersistentSettings.EnableExpected;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool EnableTaskLog => _TaskLogActive == 0
		&& (_InternalActive > 0 || PersistentSettings.EnableTaskLog);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Evs Trace calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool EnableTrace => PersistentSettings.EnableTrace;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => PersistentSettings.LogFile;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - Diag
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for outputting unexpected Exceptions that will display on
	/// DEBUG builds but be swallowed in release builds.
	/// For example an object that is unavailable on shutdown but that could possibly
	/// have been checked, or a ConnectionSource state that has not yet been
	/// identified.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Debug(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
#if DEBUG
		InternalLog(ex, message, line, memberName, sourcePath, EnEventLevel.Debug);
#endif
	}



#if DEBUG
	/// <summary>
	/// Diagnostics method for outputting DataExplorerNodeEventArgs info.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void DebugNode(Type classType, string method, DataExplorerNodeEventArgs e, EnConnectionSource connectionSource, bool onMainThread, bool initialized)
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

				Evs.Info(classType, method, str);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
			}
		}

	}
#endif



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Ex(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		InternalLog(ex, message, line, memberName, sourcePath, EnEventLevel.Error);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for TypeAccessException instance no initialized.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static TypeAccessException ExceptionInstance(Type type,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		TypeAccessException ex = new(Resources.ExceptionSingletonInstanceNotInitialized.Fmt(type.FullName));
		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		return ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for TypeInitializationException duplicate instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static TypeInitializationException ExceptionInstanceExists(Type type,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		TypeInitializationException ex = new (type.FullName,
			new ArgumentException(Resources.ExceptionDuplicateSingletonInstances.Fmt(type.FullName)));
		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		return ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static ServiceUnavailableException ExceptionService(Type type,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		ServiceUnavailableException ex = new(type);
		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		return ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for outputting expected Exceptions that will display on
	/// DEBUG builds but be swallowed in release builds unless EnableExpected is enabled.
	/// For example a connection that throws an exception due to a broken network
	/// connection or a temporarily inaccessible tab for text updates. 
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Expected(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (EnableExpected)
			InternalLog(ex, message, line, memberName, sourcePath, EnEventLevel.Expected);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates the BlackbirdSql output pane.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static async Task InternalEnsureOutputPaneAsync()
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
						Ex(ex);
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
						Ex(ex);
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
						Ex(ex);
						_TaskLogActive--;
					}
				}
				throw;
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The common Diag diagnostics method
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void InternalLog(bool isException, string message, int line,
		string memberName, string sourcePath)
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
				if ((pos = sourcePath.IndexOf("\\BlackbirdSql")) == -1)
					pos = sourcePath.IndexOf("\\BlackbirdDsl");


				if (pos != -1)
					sourcePath = sourcePath[(pos + 1)..];

				if (_TaskLogActive > 0)
				{
					str = Resources.Diag_FormatMessageOnly.Fmt(DateTime.Now.ToString("hh.mm.ss.ffffff"), message);
				}
				else
				{
					string format;

					string prefix = isException ? ":EXCEPTION: " : " ";
					string datetime = DateTime.Now.ToString("hh.mm.ss.ffffff");

					if (isException && message != "")
						format = Resources.Diag_FormatIsExceptionWithMessage;
					else if (isException)
						format = Resources.Diag_FormatIsExceptionWithoutMessage;
					else if (message != "")
						format = Resources.Diag_FormatNotExceptionWithMessage;
					else
						format = Resources.Diag_FormatNotExceptionWithoutMessage;

					str = InternalFormat(format, _Context, datetime, memberName, sourcePath, line, message);
				}
			}
			catch (Exception ex)
			{
				str = Resources.ExceptionDiagInternalLog.Fmt(ex.Message, sourcePath, memberName, line, message);
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
				str = Resources.ExceptionDiagStreamWriter.Fmt(ex.Message, str);
			}
		}

		try
		{
			if (enableTaskLog)
				OutputPaneWriteLineAsyui(str, isException);
		}
		catch (Exception) { }


		System.Diagnostics.Debug.WriteLine(str, "BlackbirdSql");
	}



	/// <summary>
	/// Replaces named diagnostic message format strings with sequential integers.
	/// </summary>
	/// <param name="format"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	private static string InternalFormat(string format, params object[] args)
	{
		string[] paramList = ["{@context}", "{@datetime}", "{@memberName}", "{@sourcePath}", "{@line}", "{@message}"];

		for (int i = 0; i < paramList.Length; i++)
			format = format.Replace(paramList[i], $"{{{i}}}");

		return format.Fmt(args);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The common Diag exceptions method
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void InternalLog(Exception ex, string message, int line,
		string memberName, string sourcePath, EnEventLevel exceptionLevel)
	{

		message ??= "";

		if (ex is DbException exd && exd.HasSqlException())
		{
			if (message != "")
				message += Environment.NewLine;

			message += InternalFormatSqlException(NativeDb.DbEngineName, exd.GetErrorCode(), exd.GetClass(), exd.GetProcedure(), exd.GetLineNumber());
		}

		if (message != "")
			message += Environment.NewLine;

		message += Resources.Diag_FormatExceptionStackTrace.Fmt(ex.StackTrace != null ? ex.StackTrace.ToString() : Environment.StackTrace.ToString());


		string exMsg = ex.Message;

		if (ex.InnerException != null)
			exMsg += $"\n{ex.InnerException.Message}";

		if (exceptionLevel <= EnEventLevel.Error)
			message = Resources.Diag_FormatExceptionError.Fmt(ex.GetType(), exMsg, message);
		else
			message = Resources.Diag_FormatExceptionNoError.Fmt(exceptionLevel.ToString().ToUpper(), ex.GetType(), exMsg, message);


		InternalLog(true, message, line, memberName, sourcePath);

	}



	/// <summary>
	/// Replaces named sql exception diagnostic message format strings with sequential integers.
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	private static string InternalFormatSqlException(params object[] args)
	{
		string format = Resources.Diag_FormatExceptionSqlException;
		string[] paramList = ["{@dbEngine}", "{@errorCode}", "{@class}", "{@procedure}", "{@line}"];

		for (int i = 0; i < paramList.Length; i++)
			format = format.Replace(paramList[i], $"{{{i}}}");

		return format.Fmt(args);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static async Task<bool> InternalTaskHandlerProgressAsync(IBsTaskHandlerClient client, string text, bool completed = false)
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
				Ex(ex);
				_TaskLogActive--;
			}

			throw ex;
		}

		return await Task.FromResult(true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static async Task<bool> InternalUpdateStatusBarAsync(string value, bool clear)
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
						_ = InternalUpdateStatusBarAsync(null, true);
					});
				}
			}
		}
		catch (Exception ex)
		{
			lock (_LockGlobal)
			{
				_TaskLogActive++;
				Ex(ex);
				_TaskLogActive--;
			}

			throw ex;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The logs a trace message.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void LogEvs(string message, int line, string memberName,
		string sourcePath, 
		EnEventLevel exceptionType = EnEventLevel.Error)
	{
		InternalLog(false, message, line, memberName, sourcePath);
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
			await InternalEnsureOutputPaneAsync();

		if (_OutputPane == null)
		{
			NullReferenceException ex = new(Resources.ExceptionOutputWindowPaneNull);

			if (_IgnoreSettings == 0)
			{
				lock (_LockGlobal)
				{
					_TaskLogActive++;
					Ex(ex);
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
					Ex(ex);
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
	/// [Async on UI thread]: Writes the given text followed by a new line to the Output
	/// window pane.
	/// </summary>
	/// <param name="value">The text value to write.</param>
	// ---------------------------------------------------------------------------------
	public static void OutputPaneWriteLineAsyui(string value, bool isException)
	{
		string outputMsg = value;
		_ = Task.Run(() => OutputPaneWriteLineAsync(outputMsg, isException));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full information stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Stack(string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (message != "")
			message += ":";

		message += (message != "" ? Environment.NewLine : "") + "INFORMATION TRACE: " + Environment.StackTrace.ToString();

		lock (_LockGlobal)
			_InternalActive++;

		_IgnoreSettings++;

		try
		{
			InternalLog(false, message, line, memberName, sourcePath);
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
	public static void StackException(Exception ex, string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
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

		InternalLog(true, message, line, memberName, sourcePath);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full exception stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void StackException(string message = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (message != "")
			message += ":";

		message += (message != "" ? Environment.NewLine : "") + "TRACE: " + Environment.StackTrace.ToString();

		InternalLog(true, message, line, memberName, sourcePath);

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

		Task.Factory.StartNew(() => InternalTaskHandlerProgressAsync(client, text, completed),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for OnUiThread
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static object ThrowException(Exception ex,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		throw ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises excepton for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static async Task<TResult> ThrowExceptionServiceUnavailableAsync<TResult>(Type serviceType,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
		where TResult : class
	{
		if (serviceType != null)
			throw ExceptionService(serviceType, line, memberName, sourcePath);

		return await Task.FromResult<TResult>(null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Throws an exception for TypeAccessException instance exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ThrowIfInstanceExists(object instance,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (instance != null)
			throw ExceptionInstance(instance.GetType(), line, memberName, sourcePath);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Throws an exception for TypeInitializationException duplicate instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ThrowIfInstanceNull(object instance, Type type,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (instance == null)
			throw ExceptionInstanceExists(type, line, memberName, sourcePath);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Logs and throws an exception if NotOnUiThread and prevents an unecessary UI
	/// thread trail by Intellisense.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ThrowIfNotOnUIThread(
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (ThreadHelper.CheckAccess())
			return;

		string message = Resources.ExceptionNotOnUiThread.Fmt(memberName);
		COMException ex = new(message, VSConstants.RPC_E_WRONG_THREAD);

		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		throw ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises an exception if OnUiThread.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ThrowIfOnUIThread(
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (!ThreadHelper.CheckAccess())
			return;

		string message = Resources.ExceptionIsOnUiThread.Fmt(memberName);
		COMException ex = new(message, VSConstants.RPC_E_WRONG_THREAD);

		InternalLog(ex, "", line, memberName, sourcePath, EnEventLevel.Error);

		throw ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises excepton for ServiceUnavailableException
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ThrowIfServiceUnavailable(object service, Type type,
		[System.Runtime.CompilerServices.CallerLineNumber] int line = -1,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "")
	{
		if (service == null)
			throw ExceptionService(type, line, memberName, sourcePath);
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

		_ = Task.Factory.StartNew(() => InternalUpdateStatusBarAsync(value, clear), default,
			TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default);

		return true;
	}


#endregion Methods

}
