#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Threading;

using Babel;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Model;
using BlackbirdSql.LanguageExtension.ColorService;

using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

using Cmd = BlackbirdSql.Common.Cmd;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.LanguageExtension;


public class SourceAgent : Microsoft.VisualStudio.Package.Source, IVsUserDataEvents
{
	internal const int C_MinServerVersionSupported = 9;

	private readonly object _DisposeLock = new object();

	private bool isSqlCmdModeEnabled;

	private bool intelliSenseEnabled;

	private int prvChangeCount;

	private bool isReallyDirty;

	private bool hasPendingRegions;

	private bool isServerSupported;

	private readonly ParseManager parseManager;

	private readonly AbstractLanguageService sqlLanguageService;

	internal ITextUndoTransaction CurrentCommitUndoTransaction { get; set; }

	public bool IsSqlCmdModeEnabled => isSqlCmdModeEnabled;

	// private Ns.SmoMetadataProviderProvider SmoMetadataProviderProvider { get; set; }

	public bool IsServerSupported => isServerSupported;

	public bool IsDisconnectedMode
	{
		get
		{
			bool flag = false;
			/*
			AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
			if (auxiliaryDocData != null)
			{
				flag = auxiliaryDocData.QueryExecutor.IsConnected;
			}
			*/
			return !flag;
		}
	}

	public string DatabaseName
	{
		get
		{
			return GetDatabaseNameFromConnectionInfo(ConnectionInfo);
			/*
			string text = null;
			IMetadataProviderProvider metadataProviderProvider = GetMetadataProviderProvider();
			if (metadataProviderProvider != null && metadataProviderProvider is Ns.SmoMetadataProviderProvider)
			{
				AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
				if (auxiliaryDocData != null)
				{
					IDbConnection connection = auxiliaryDocData.QueryExecutor.ConnectionStrategy.Connection;
					if (connection != null)
					{
						text = connection.Database;
					}
				}
			}

			if (text == null)
			{
				if (metadataProviderProvider != null)
				{
					text = metadataProviderProvider.DatabaseName;
				}
				else if (!IsDisconnectedMode)
				{
					text = GetDatabaseNameFromConnectionInfo(ConnectionInfo);
				}
			}

			return text;
			*/
		}
	}

	public ParseResult ParseResult => parseManager.ParseResult;

	public bool IntelliSenseEnabled => intelliSenseEnabled;

	public IEnumerable<Region> HiddenRegions => parseManager.HiddenRegions;

	public IEnumerable<Error> Errors => parseManager.Errors;

	public bool IsReallyDirty => isReallyDirty;

	public bool HasPendingRegions
	{
		get
		{
			return hasPendingRegions;
		}
		set
		{
			hasPendingRegions = value;
		}
	}

	public UIConnectionInfo ConnectionInfo
	{
		get
		{
			UIConnectionInfo result = null;
			/*
			AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
			if (auxiliaryDocData != null)
			{
				result = auxiliaryDocData.QueryExecutor.ConnectionStrategy.ConnectionInfo;
			}
			*/
			return result;
		}
	}

	internal SourceAgent(AbstractLanguageService service, IVsTextLines textLines, Colorizer colorizer)
		: base(service, textLines, colorizer)
	{
		sqlLanguageService = service;
		intelliSenseEnabled = service.SqlLanguagePreferences.EnableIntellisense;
		/*
		AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
		if (auxiliaryDocData != null)
		{
			if (!auxiliaryDocData.IntellisenseEnabled.HasValue)
			{
				auxiliaryDocData.IntellisenseEnabled = intelliSenseEnabled;
			}
			else
			{
				intelliSenseEnabled = auxiliaryDocData.IntellisenseEnabled.Value;
			}

			auxiliaryDocData.QueryExecutor.ScriptExecutionCompleted += HandleScriptExecutionCompleted;
			auxiliaryDocData.QueryExecutor.BatchExecutionCompleted += HandleBatchExecutionCompleted;
		}
		*/
		isServerSupported = false;
		parseManager = new ParseManager(this);
		prvChangeCount = -1;
		isReallyDirty = false;
		OutliningEnabled = sqlLanguageService.Preferences.AutoOutlining;
		Reset();
	}

	public IMetadataProviderProvider GetMetadataProviderProvider()
	{
		return null;
		/*
		IMetadataProviderProvider metadataProviderProvider = null;
		AuxiliaryDocData auxillaryDocData = EditorExtensionAsyncPackage.Instance.GetAuxiliaryDocData(GetTextLines());
		if (auxillaryDocData != null && auxillaryDocData.IntellisenseEnabled != false)
		{
			metadataProviderProvider = auxillaryDocData.Strategy.MetadataProviderProvider;
			metadataProviderProvider ??= SmoMetadataProviderProvider;

			if (metadataProviderProvider == null && auxillaryDocData.QueryExecutor.IsConnected)
			{
				UIConnectionInfo connectionInfo = auxillaryDocData.QueryExecutor.ConnectionStrategy.ConnectionInfo;
				if (connectionInfo != null && !string.IsNullOrEmpty(GetDatabaseNameFromConnectionInfo(connectionInfo)))
				{
					SmoMetadataProviderProvider = SmoMetadataProviderProvider.Cache.Instance.Acquire(auxillaryDocData.QueryExecutor);
					metadataProviderProvider = SmoMetadataProviderProvider;
				}
			}
		}
		return metadataProviderProvider;
		*/
	}

	public string GetDatabaseNameFromConnectionInfo(UIConnectionInfo uici)
	{
		string result = null;
		if (uici != null)
		{
			result = uici.Dataset;
		}

		return result;
	}

	/*
	private void HandleBatchExecutionCompleted(object sender, QESQLBatchExecutedEventArgs args)
	{
		Ns.SmoMetadataProviderProvider smoMetadataProviderProvider = GetMetadataProviderProvider() as Ns.SmoMetadataProviderProvider;
		AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
		if (smoMetadataProviderProvider != null && auxiliaryDocData != null && SmoMetadataProviderProvider.CheckForDatabaseChangesAfterQueryExecution)
		{
			smoMetadataProviderProvider.AsyncAddDatabaseToDriftDetectionSet(auxiliaryDocData.QueryExecutor.ConnectionStrategy.Connection.Database);
		}
	}
	*/

	/*
	private void HandleScriptExecutionCompleted(object sender, ScriptExecutionCompletedEventArgs args)
	{
		Ns.SmoMetadataProviderProvider smoMetadataProviderProvider = GetMetadataProviderProvider() as Ns.SmoMetadataProviderProvider;
		if (smoMetadataProviderProvider != null && Ns.SmoMetadataProviderProvider.CheckForDatabaseChangesAfterQueryExecution)
		{
			smoMetadataProviderProvider.AsyncCheckForDatabaseChanges();
		}
	}
	*/

	/*
	private AuxiliaryDocData GetAuxiliaryDocData()
	{
		IVsTextLines docData = GetTextLines();
		return EditorExtensionAsyncPackage.Instance.GetAuxiliaryDocData(docData);
	}
	*/


	public bool ExecuteParseRequest(string text)
	{
		isReallyDirty = prvChangeCount != base.ChangeCount;
		prvChangeCount = base.ChangeCount;
		IBinder binder = null;
		IMetadataProviderProvider metadataProviderProvider = GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			try
			{
				ManualResetEvent buildEvent = metadataProviderProvider.BuildEvent;
				Tracer.Trace(GetType(), "ExecuteParseRequest()", "waiting for metadata provider...(ThreadName = " + Thread.CurrentThread.Name + ")");
				if (buildEvent.WaitOne(2000))
				{
					binder = metadataProviderProvider.Binder;
					Tracer.Trace(GetType(), "ExecuteParseRequest()", "...done waiting for metadata provider(ThreadName = " + Thread.CurrentThread.Name + ")");
				}
				else
				{
					Tracer.Trace(GetType(), "ExecuteParseRequest()", "...timed out waiting for metadata provider(ThreadName = " + Thread.CurrentThread.Name + ")");
				}
			}
			catch (Exception e)
			{
				Tracer.LogExCatch(GetType(), e);
			}
		}
		else
		{
			_ = IsDisconnectedMode;
		}

		ParseOptions parseOptions;

		/*
		AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();

		if (auxiliaryDocData != null)
		{
			if (metadataProviderProvider != null)
			{
				parseOptions = metadataProviderProvider.CreateParseOptions();
				parseOptions.BatchSeparator = auxiliaryDocData.QueryExecutor.ExecutionOptions.BatchSeparator;
			}
			else
			{
				parseOptions = new ParseOptions(auxiliaryDocData.QueryExecutor.ExecutionOptions.BatchSeparator);
			}
		}
		else
		{
			parseOptions = new ParseOptions("GO");
		}
		*/
		parseOptions = new ParseOptions("GO");

		return parseManager.ExecuteParseRequest(text, parseOptions, binder, DatabaseName);
	}

	public void Reset()
	{
		hasPendingRegions = true;
		parseManager.Reset();
		ReleaseSmoMetadataProviderProvider();
	}

	public override CommentInfo GetCommentFormat()
	{
		return ColorConfiguration.MyCommentInfo;
	}

	public override TextSpan CommentLines(TextSpan span, string lineComment)
	{
		if (span.iStartLine < span.iEndLine && span.iEndIndex == 0)
		{
			span.iEndLine--;
		}

		int num = span.iEndLine - span.iStartLine + 1;
		bool[] array = new bool[num];
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			int line = span.iStartLine + i;
			array[i] = false;
			int num3 = ScanToNonWhitespaceChar(line);
			if (num3 != GetLineLength(line))
			{
				array[i] = true;
				if (num2 == -1 || num3 < num2)
				{
					num2 = num3;
				}
			}
		}

		for (int i = 0; i < num; i++)
		{
			if (array[i])
			{
				int num4 = span.iStartLine + i;
				SetText(num4, num2, num4, num2, lineComment);
			}
		}

		return span;
	}

	public override TextSpan UncommentLines(TextSpan span, string lineComment)
	{
		int length = lineComment.Length;
		for (int i = span.iStartLine; i <= span.iEndLine; i++)
		{
			if (span.iStartLine == span.iEndLine || i != span.iEndLine || span.iEndIndex > 0)
			{
				string line = GetLine(i);
				int num = line.IndexOf(lineComment, StringComparison.Ordinal);
				int num2 = 0;
				if (num >= 0 && num == ScanToNonWhitespaceChar(i))
				{
					SetText(i, num, i, num + length, string.Empty);
					num2 = length;
				}

				if (i == span.iEndLine)
				{
					span.iEndIndex = line.Length - num2;
				}
			}
		}

		span.iStartIndex = 0;
		return span;
	}

	public override CompletionSet CreateCompletionSet()
	{
		return new SqlCompletionSet(base.LanguageService.GetImageList(), this);
	}

	public override void OnCommand(IVsTextView textView, VSConstants.VSStd2KCmdID command, char ch)
	{
		base.OnCommand(textView, command, ch);
		if (textView != null && base.LanguageService != null && base.LanguageService.Preferences.EnableCodeSense)
		{
			SqlCompletionSet sqlCompletionSet = (SqlCompletionSet)base.CompletionSet;
			if (command == VSConstants.VSStd2KCmdID.TYPECHAR && sqlCompletionSet != null && sqlCompletionSet.IsDisplayed && IsExplicitFilteringRequired(sqlCompletionSet.GetTextTypedSoFar()))
			{
				sqlCompletionSet.ExplicitFilterDeclarationList();
			}

			if ((command == VSConstants.VSStd2KCmdID.BACKSPACE || command == VSConstants.VSStd2KCmdID.DELETE) && sqlCompletionSet != null && sqlCompletionSet.IsDisplayed)
			{
				sqlCompletionSet.ResetTextMarker();
				sqlCompletionSet.ExplicitFilterDeclarationList();
			}

			if ((command == VSConstants.VSStd2KCmdID.UP || command == VSConstants.VSStd2KCmdID.DOWN) && base.CompletionSet.IsDisplayed && sqlCompletionSet.InPreviewMode && sqlCompletionSet.InPreviewModeOutline)
			{
				sqlCompletionSet.InPreviewModeOutline = false;
				sqlCompletionSet.UpdateCompletionStatus(forceSelect: true);
			}
		}
	}

	public static bool IsExplicitFilteringRequired(string textTypedSofar)
	{
		if (string.IsNullOrEmpty(textTypedSofar) || textTypedSofar[0] == Cmd.OpenSquareBracket || textTypedSofar[0] == Cmd.DoubleQuote || char.IsLetter(textTypedSofar[0]))
		{
			return false;
		}

		return true;
	}

	void IVsUserDataEvents.OnUserDataChange(ref Guid riidKey, object vtNewValue)
	{
		if (riidKey.Equals(new Guid(ServiceData.PropertyOleSqlGuid)))
		{
			isSqlCmdModeEnabled = (bool)vtNewValue;
			(GetColorizer().Scanner as LineScanner).IsSqlCmdModeEnabled = isSqlCmdModeEnabled;
			IsDirty = true;
		}
		else if (riidKey.Equals(new Guid(ServiceData.PropertyBatchSeparatorGuid)))
		{
			string batchSeparator = vtNewValue as string;
			(GetColorizer().Scanner as LineScanner).BatchSeparator = batchSeparator;
			IsDirty = true;
		}
		else if (riidKey.Equals(new Guid(ServiceData.PropertySqlVersionGuid)))
		{
			int num = 0;
			string text = vtNewValue as string;
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split('.');
				int result = 0;
				if (array.Length != 0 && array[0] != null)
				{
					int.TryParse(array[0], out result);
				}

				num = result;
			}

			if (num != 0)
			{
				Reset();
				if (num >= C_MinServerVersionSupported)
				{
					isServerSupported = true;
				}
				else
				{
					isServerSupported = false;
				}
			}
			else
			{
				isServerSupported = false;
				Reset();
			}

			IsDirty = true;
		}
		else if (riidKey.Equals(new Guid(ServiceData.IntelliSenseEnabledGuid)))
		{
			intelliSenseEnabled = (bool)vtNewValue;
			IsDirty = true;
		}
		else if (riidKey.Equals(ServiceData.CLSID_DatabaseChanged))
		{
			IsDirty = true;
			Reset();
		}
	}

	public override DocumentTask CreateErrorTaskItem(TextSpan span, MARKERTYPE markerType, string filename)
	{
		/*
		AuxiliaryDocData auxillaryDocData = EditorExtensionAsyncPackage.Instance.GetAuxiliaryDocData(GetTextLines());
		if (auxillaryDocData != null)
		{
			ISqlEditorErrorTaskFactory errorTaskFactory = auxillaryDocData.Strategy.GetErrorTaskFactory();
			if (errorTaskFactory != null)
			{
				return errorTaskFactory.CreateErrorTaskItem(span, markerType, filename, GetTextLines());
			}
		}
		*/

		return base.CreateErrorTaskItem(span, markerType, filename);
	}

	public override DocumentTask CreateErrorTaskItem(TextSpan span, string filename, string message, TaskPriority priority, TaskCategory category, MARKERTYPE markerType, TaskErrorCategory errorCategory)
	{
		/*
		AuxiliaryDocData auxillaryDocData = EditorExtensionAsyncPackage.Instance.GetAuxiliaryDocData(GetTextLines());
		if (auxillaryDocData != null)
		{
			ISqlEditorErrorTaskFactory errorTaskFactory = auxillaryDocData.Strategy.GetErrorTaskFactory();
			if (errorTaskFactory != null)
			{
				DocumentTask documentTask = errorTaskFactory.CreateErrorTaskItem(span, markerType, filename, GetTextLines());
				if (documentTask != null)
				{
					documentTask.Priority = priority;
					documentTask.Category = category;
					documentTask.ErrorCategory = errorCategory;
					documentTask.Text = message;
					documentTask.IsTextEditable = false;
					documentTask.IsCheckedEditable = false;
				}

				return documentTask;
			}
		}
		*/

		return base.CreateErrorTaskItem(span, filename, message, priority, category, markerType, errorCategory);
	}

	public override void Dispose()
	{
		lock (_DisposeLock)
		{
			base.Dispose();
			ReleaseSmoMetadataProviderProvider();
			/*
			AuxiliaryDocData auxiliaryDocData = GetAuxiliaryDocData();
			if (auxiliaryDocData != null)
			{
				auxiliaryDocData.QueryExecutor.BatchExecutionCompleted -= HandleBatchExecutionCompleted;
				auxiliaryDocData.QueryExecutor.ScriptExecutionCompleted -= HandleScriptExecutionCompleted;
			}
			*/
		}
	}

	internal bool TryEnterDisposeLock()
	{
		return Monitor.TryEnter(_DisposeLock);
	}

	internal void ExitDisposeLock()
	{
		Monitor.Exit(_DisposeLock);
	}

	private void ReleaseSmoMetadataProviderProvider()
	{
		/*
		if (SmoMetadataProviderProvider != null)
		{
			SmoMetadataProviderProvider.Cache.Instance.Release(SmoMetadataProviderProvider);
			SmoMetadataProviderProvider = null;
		}
		*/
	}
}
