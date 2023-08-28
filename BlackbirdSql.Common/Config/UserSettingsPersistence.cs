// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.UserSettingsPersistence

using System;
using System.Drawing;
using System.Globalization;

using BlackbirdSql.Common.Config.Enums;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;


namespace BlackbirdSql.Common.Config;

public sealed class UserSettingsPersistence : IUserSettingsPersistence, IUserSettings, ICloneable
{
	private static class SettingsConstants
	{
		public static class General
		{
			public static readonly string PromptForSaveWhenClosingQueryWindowsKey = "PromptForSaveWhenClosingQueryWindows";
		}

		public static class StatusBar
		{
			public static readonly string DisplayTimeOptionKey = "DisplayTimeOption";

			public static readonly string StatusBarIncludeServerNameKey = "StatusBarIncludeServerName";

			public static readonly string StatusBarIncludeDatabaseNameKey = "StatusBarIncludeDatabaseName";

			public static readonly string StatusBarIncludeLoginNameKey = "StatusBarIncludeLoginName";

			public static readonly string StatusBarIncludeRowCountKey = "StatusBarIncludeRowCount";

			public static readonly string StatusBarBackgroundColor = "StatusBarBackgroundColor";

			public static readonly string StatusBarBackgroundColorIsNamedColor = "StatusBarBackgroundColorIsNamedColor";

			public static readonly string TabTextIncludeServerNameKey = "TabTextIncludeServerName";

			public static readonly string TabTextIncludeDatabaseNameKey = "TabTextIncludeDatabaseName";

			public static readonly string TabTextIncludeLoginNameKey = "TabTextIncludeLoginName";

			public static readonly string TabTextIncludeFileNameKey = "TabTextIncludeFileName";
		}

		public static class Execution
		{
			public static readonly string BatchSeparatorKey = "BatchSeparator";

			public static readonly string DisconnectAfterQueryExecutesKey = "DisconnectAfterQueryExecutes";

			public static readonly string ExecutionTimeoutKey = "ExecutionTimeout";

			public static readonly string OLESQLScriptingByDefaultKey = "OLESQLScriptingByDefault";

			public static readonly string SetAnsiNullDefaultKey = "SetAnsiNullDefault";

			public static readonly string SetAnsiNullDefaultStringKey = "SetAnsiNullDefaultString";

			public static readonly string SetAnsiNullsKey = "SetAnsiNulls";

			public static readonly string SetAnsiNullsStringKey = "SetAnsiNullsString";

			public static readonly string SetAnsiPaddingKey = "SetAnsiPadding";

			public static readonly string SetAnsiPaddingStringKey = "SetAnsiPaddingString";

			public static readonly string SetAnsiWarningsKey = "SetAnsiWarnings";

			public static readonly string SetAnsiWarningsStringKey = "SetAnsiWarningsString";

			public static readonly string SetArithAbortKey = "SetArithAbort";

			public static readonly string SetArithAbortStringKey = "SetArithAbortString";

			public static readonly string SetConcatenationNullKey = "SetConcatenationNull";

			public static readonly string SetConcatenationNullStringKey = "SetConcatenationNullString";

			public static readonly string SetCursorCloseOnCommitKey = "SetCursorCloseOnCommit";

			public static readonly string SetCursorCloseOnCommitStringKey = "SetCursorCloseOnCommitString";

			public static readonly string SetDeadlockPriorityLowKey = "SetDeadlockPriorityLow";

			public static readonly string SetDeadlockPriorityStringKey = "SetDeadlockPriorityString";

			public static readonly string SetFmtOnlyKey = "SetFmtOnly";

			public static readonly string SetForceplanKey = "SetForceplan";

			public static readonly string SetImplicitTransactionKey = "SetImplicitTransaction";

			public static readonly string SetImplicitTransactionStringKey = "SetImplicitTransactionString";

			public static readonly string SetLockTimeoutKey = "SetLockTimeout";

			public static readonly string SetLockTimeoutStringKey = "SetLockTimeoutString";

			public static readonly string SetNoCountKey = "SetNoCount";

			public static readonly string SetNoCountStringKey = "SetNoCountString";

			public static readonly string SetNoExecKey = "SetNoExec";

			public static readonly string SetNumericAbortKey = "SetNumericAbort";

			public static readonly string SetParseOnlyKey = "SetParseOnly";

			public static readonly string SetQueryGovernorCostKey = "SetQueryGovernorCost";

			public static readonly string SetQueryGovernorCostStringKey = "SetQueryGovernorCostString";

			public static readonly string SetQuotedIdentifierKey = "SetQuotedIdentifier";

			public static readonly string SetQuotedIdentifierStringKey = "SetQuotedIdentifierString";

			public static readonly string SetRowCountKey = "SetRowCount";

			public static readonly string SetRowCountStringKey = "SetRowCountString";

			public static readonly string SetShowplanTextKey = "SetShowplanText";

			public static readonly string SetStatisticsIOKey = "SetStatisticsIO";

			public static readonly string SetStatisticsProfileKey = "SetStatisticsProfile";

			public static readonly string SetStatisticsTimeKey = "SetStatisticsTime";

			public static readonly string SetTextSizeKey = "SetTextSize";

			public static readonly string SetTextSizeStringKey = "SetTextSizeString";

			public static readonly string SetTransactionIsolationLevelKey = "SetTransactionIsolationLevel";

			public static readonly string SetTransactionIsolationLevelStringKey = "SetTransactionIsolationLevelString";

			public static readonly string SetXACTAbortKey = "SetXACTAbort";

			public static readonly string SuppressProviderMessageHeadersKey = "SuppressProviderMessageHeaders";
		}

		public static class Results
		{
			public static readonly string ColumnDelimiterForTextKey = "ColumnDelimiterForText";

			public static readonly string DiscardResultsForGridKey = "DiscardResultsForGrid";

			public static readonly string DiscardResultsForTextKey = "DiscardResultsForText";

			public static readonly string DisplayResultInSeparateTabForGridKey = "DisplayResultInSeparateTabForGrid";

			public static readonly string DisplayResultInSeparateTabForTextKey = "DisplayResultInSeparateTabForText";

			public static readonly string IncludeColumnHeadersWhileSavingGridResultsKey = "IncludeColumnHeadersWhileSavingGridResults";

			public static readonly string MaxCharsPerColumnForGridKey = "MaxCharsPerColumnForGrid";

			public static readonly string MaxCharsPerColumnForTextKey = "MaxCharsPerColumnForText";

			public static readonly string MaxCharsPerColumnForXmlKey = "MaxCharsPerColumnForXml";

			public static readonly string OutputQueryForGridKey = "OutputQueryForGrid";

			public static readonly string OutputQueryForTextKey = "OutputQueryForText";

			public static readonly string PrintColumnHeadersForTextKey = "PrintColumnHeadersForText";

			public static readonly string ProvideFeedbackWithSoundsKey = "ProvideFeedbackWithSounds";

			public static readonly string QuoteStringsContainingCommasKey = "QuoteStringsContainingCommas";

			public static readonly string ResultsDirectoryKey = "ResultsDirectory";

			public static readonly string RightAlignNumericsForTextKey = "RightAlignNumericsForText";

			public static readonly string ScrollResultsAsReceivedForTextKey = "ScrollResultsAsReceivedForText";

			public static readonly string ShowAllGridsInTheSameTabKey = "ShowAllGridsInTheSameTab";

			public static readonly string ShowGridLinesInMapKey = "ShowGridLinesInMap";

			public static readonly string ShowMessagesInNewTabForTextKey = "ShowMessagesInNewTabForText";

			public static readonly string SqlExecutionModeKey = "SqlExecutionMode";

			public static readonly string SwitchToResultsTabAfterQueryExecutesForGridKey = "SwitchToResultsTabAfterQueryExecutesForGrid";

			public static readonly string SwitchToResultsTabAfterQueryExecutesForTextKey = "SwitchToResultsTabAfterQueryExecutesForText";
		}

		public static class EditorContext
		{
			public static readonly string StatusBarPositionKey = "StatusBarPosition";
		}

		public static readonly string SqlEditorUserSettingsPath = "\\BlackbirdSqlEditorUserSettings";
	}

	private IGeneralSettings _generalSettings = new GeneralSettings();

	private IEditorTabAndStatusBarSettings _statusBarSettings = new EditorTabAndStatusBarSettings();

	private IQueryExecutionSettings _executionSettings = new ExecutionSettings();

	private IQueryExecutionResultsSettings _resultsSettings = new ExecutionResultsSettings();

	private IEditorContextSettings _editorContextSettings = new EditorContextSettings();

	public IGeneralSettings General
	{
		get
		{
			return _generalSettings;
		}
		set
		{
			_generalSettings = value;
		}
	}

	public IEditorTabAndStatusBarSettings StatusBar
	{
		get
		{
			return _statusBarSettings;
		}
		set
		{
			_statusBarSettings = value;
		}
	}

	public IQueryExecutionSettings Execution
	{
		get
		{
			return _executionSettings;
		}
		set
		{
			_executionSettings = value;
		}
	}

	public IQueryExecutionResultsSettings ExecutionResults
	{
		get
		{
			return _resultsSettings;
		}
		set
		{
			_resultsSettings = value;
		}
	}

	public IEditorContextSettings EditorContext
	{
		get
		{
			return _editorContextSettings;
		}
		set
		{
			_editorContextSettings = value;
		}
	}

	public IUserSettings Load()
	{
		_generalSettings.PromptForSaveWhenClosingQueryWindows = ReadBoolean(SettingsConstants.General.PromptForSaveWhenClosingQueryWindowsKey, GeneralSettings.Defaults.PromptForSaveWhenClosingQueryWindows);
		_statusBarSettings.ShowTimeOption = ReadDisplayTimeOptions(SettingsConstants.StatusBar.DisplayTimeOptionKey, EditorTabAndStatusBarSettings.Defaults.ShowTimeOption);
		_statusBarSettings.StatusBarIncludeServerName = ReadBoolean(SettingsConstants.StatusBar.StatusBarIncludeServerNameKey, EditorTabAndStatusBarSettings.Defaults.StatusBarIncludeServerName);
		_statusBarSettings.StatusBarIncludeDatabaseName = ReadBoolean(SettingsConstants.StatusBar.StatusBarIncludeDatabaseNameKey, EditorTabAndStatusBarSettings.Defaults.StatusBarIncludeDatabaseName);
		_statusBarSettings.StatusBarIncludeLoginName = ReadBoolean(SettingsConstants.StatusBar.StatusBarIncludeLoginNameKey, EditorTabAndStatusBarSettings.Defaults.StatusBarIncludeLoginName);
		_statusBarSettings.StatusBarIncludeRowCount = ReadBoolean(SettingsConstants.StatusBar.StatusBarIncludeRowCountKey, EditorTabAndStatusBarSettings.Defaults.StatusBarIncludeRowCount);
		if (ReadBoolean(SettingsConstants.StatusBar.StatusBarBackgroundColorIsNamedColor, defaultValue: false))
		{
			_statusBarSettings.StatusBarColor = Color.FromName(ReadString(SettingsConstants.StatusBar.StatusBarBackgroundColor, EditorTabAndStatusBarSettings.Defaults.StatusBarColor.Name));
		}
		else
		{
			_statusBarSettings.StatusBarColor = EditorTabAndStatusBarSettings.Defaults.StatusBarColor;
			string text = ReadString(SettingsConstants.StatusBar.StatusBarBackgroundColor, string.Empty);
			if (!string.IsNullOrEmpty(text) && int.TryParse(text, out var result))
			{
				_statusBarSettings.StatusBarColor = Color.FromArgb(result);
			}
		}

		_statusBarSettings.TabTextIncludeServerName = ReadBoolean(SettingsConstants.StatusBar.TabTextIncludeServerNameKey, EditorTabAndStatusBarSettings.Defaults.TabTextIncludeServerName);
		_statusBarSettings.TabTextIncludeDatabaseName = ReadBoolean(SettingsConstants.StatusBar.TabTextIncludeDatabaseNameKey, EditorTabAndStatusBarSettings.Defaults.TabTextIncludeDatabaseName);
		_statusBarSettings.TabTextIncludeLoginName = ReadBoolean(SettingsConstants.StatusBar.TabTextIncludeLoginNameKey, EditorTabAndStatusBarSettings.Defaults.TabTextIncludeLoginName);
		_statusBarSettings.TabTextIncludeFileName = ReadBoolean(SettingsConstants.StatusBar.TabTextIncludeFileNameKey, EditorTabAndStatusBarSettings.Defaults.TabTextIncludeFileName);
		_executionSettings.BatchSeparator = ReadString(SettingsConstants.Execution.BatchSeparatorKey, ExecutionSettings.Defaults.BatchSeparator.ToString());
		_executionSettings.DisconnectAfterQueryExecutes = ReadBoolean(SettingsConstants.Execution.DisconnectAfterQueryExecutesKey, ExecutionSettings.Defaults.DisconnectAfterQueryExecutes);
		_executionSettings.ExecutionTimeout = ReadInt(SettingsConstants.Execution.ExecutionTimeoutKey, ExecutionSettings.Defaults.ExecutionTimeout);
		_executionSettings.OLESQLScriptingByDefault = ReadBoolean(SettingsConstants.Execution.OLESQLScriptingByDefaultKey, ExecutionSettings.Defaults.OLESQLScriptingByDefault);
		_executionSettings.SetAnsiNullDefault = ReadBoolean(SettingsConstants.Execution.SetAnsiNullDefaultKey, ExecutionSettings.Defaults.SetAnsiNullDefault);
		_executionSettings.SetAnsiNulls = ReadBoolean(SettingsConstants.Execution.SetAnsiNullsKey, ExecutionSettings.Defaults.SetAnsiNulls);
		_executionSettings.SetAnsiPadding = ReadBoolean(SettingsConstants.Execution.SetAnsiPaddingKey, ExecutionSettings.Defaults.SetAnsiPadding);
		_executionSettings.SetAnsiWarnings = ReadBoolean(SettingsConstants.Execution.SetAnsiWarningsKey, ExecutionSettings.Defaults.SetAnsiWarnings);
		_executionSettings.SetArithAbort = ReadBoolean(SettingsConstants.Execution.SetArithAbortKey, ExecutionSettings.Defaults.SetArithAbort);
		_executionSettings.SetConcatenationNull = ReadBoolean(SettingsConstants.Execution.SetConcatenationNullKey, ExecutionSettings.Defaults.SetConcatenationNull);
		_executionSettings.SetCursorCloseOnCommit = ReadBoolean(SettingsConstants.Execution.SetCursorCloseOnCommitKey, ExecutionSettings.Defaults.SetCursorCloseOnCommit);
		_executionSettings.SetDeadlockPriorityLow = ReadBoolean(SettingsConstants.Execution.SetDeadlockPriorityLowKey, ExecutionSettings.Defaults.SetDeadlockPriorityLow);
		_executionSettings.SetFmtOnly = ReadBoolean(SettingsConstants.Execution.SetFmtOnlyKey, ExecutionSettings.Defaults.SetFmtOnly);
		_executionSettings.SetForceplan = ReadBoolean(SettingsConstants.Execution.SetForceplanKey, ExecutionSettings.Defaults.SetForceplan);
		_executionSettings.SetImplicitTransaction = ReadBoolean(SettingsConstants.Execution.SetImplicitTransactionKey, ExecutionSettings.Defaults.SetImplicitTransaction);
		_executionSettings.SetLockTimeout = ReadInt(SettingsConstants.Execution.SetLockTimeoutKey, ExecutionSettings.Defaults.SetLockTimeout);
		_executionSettings.SetNoCount = ReadBoolean(SettingsConstants.Execution.SetNoCountKey, ExecutionSettings.Defaults.SetNoCount);
		_executionSettings.SetNoExec = ReadBoolean(SettingsConstants.Execution.SetNoExecKey, ExecutionSettings.Defaults.SetNoExec);
		_executionSettings.SetNumericAbort = ReadBoolean(SettingsConstants.Execution.SetNumericAbortKey, ExecutionSettings.Defaults.SetNumericAbort);
		_executionSettings.SetParseOnly = ReadBoolean(SettingsConstants.Execution.SetParseOnlyKey, ExecutionSettings.Defaults.SetParseOnly);
		_executionSettings.SetQueryGovernorCost = ReadInt(SettingsConstants.Execution.SetQueryGovernorCostKey, ExecutionSettings.Defaults.SetQueryGovernorCost);
		_executionSettings.SetQuotedIdentifier = ReadBoolean(SettingsConstants.Execution.SetQuotedIdentifierKey, ExecutionSettings.Defaults.SetQuotedIdentifier);
		_executionSettings.SetRowCount = ReadInt(SettingsConstants.Execution.SetRowCountKey, ExecutionSettings.Defaults.SetRowCount);
		_executionSettings.SetShowplanText = ReadBoolean(SettingsConstants.Execution.SetShowplanTextKey, ExecutionSettings.Defaults.SetShowplanText);
		_executionSettings.SetStatisticsIO = ReadBoolean(SettingsConstants.Execution.SetStatisticsIOKey, ExecutionSettings.Defaults.SetStatisticsIO);
		_executionSettings.SetStatisticsProfile = ReadBoolean(SettingsConstants.Execution.SetStatisticsProfileKey, ExecutionSettings.Defaults.SetStatisticsProfile);
		_executionSettings.SetStatisticsTime = ReadBoolean(SettingsConstants.Execution.SetStatisticsTimeKey, ExecutionSettings.Defaults.SetStatisticsTime);
		_executionSettings.SetTextSize = ReadInt(SettingsConstants.Execution.SetTextSizeKey, ExecutionSettings.Defaults.SetTextSize);
		_executionSettings.SetTransactionIsolationLevel = ReadString(SettingsConstants.Execution.SetTransactionIsolationLevelKey, ExecutionSettings.Defaults.SetTransactionIsolationLevel);
		_executionSettings.SetXACTAbort = ReadBoolean(SettingsConstants.Execution.SetXACTAbortKey, ExecutionSettings.Defaults.SetXACTAbort);
		_executionSettings.SuppressProviderMessageHeaders = ReadBoolean(SettingsConstants.Execution.SuppressProviderMessageHeadersKey, ExecutionSettings.Defaults.SuppressProviderMessageHeaders);
		_resultsSettings.ColumnDelimiterForText = ReadChar(SettingsConstants.Results.ColumnDelimiterForTextKey, ExecutionResultsSettings.Defaults.ColumnDelimiterForText);
		_resultsSettings.DiscardResultsForGrid = ReadBoolean(SettingsConstants.Results.DiscardResultsForGridKey, ExecutionResultsSettings.Defaults.DiscardResultsForGrid);
		_resultsSettings.DiscardResultsForText = ReadBoolean(SettingsConstants.Results.DiscardResultsForTextKey, ExecutionResultsSettings.Defaults.DiscardResultsForText);
		_resultsSettings.DisplayResultInSeparateTabForGrid = ReadBoolean(SettingsConstants.Results.DisplayResultInSeparateTabForGridKey, ExecutionResultsSettings.Defaults.DisplayResultInSeparateTabForGrid);
		_resultsSettings.DisplayResultInSeparateTabForText = ReadBoolean(SettingsConstants.Results.DisplayResultInSeparateTabForTextKey, ExecutionResultsSettings.Defaults.DisplayResultInSeparateTabForText);
		_resultsSettings.IncludeColumnHeadersWhileSavingGridResults = ReadBoolean(SettingsConstants.Results.IncludeColumnHeadersWhileSavingGridResultsKey, ExecutionResultsSettings.Defaults.IncludeColumnHeadersWhileSavingGridResults);
		_resultsSettings.MaxCharsPerColumnForGrid = ReadInt(SettingsConstants.Results.MaxCharsPerColumnForGridKey, ExecutionResultsSettings.Defaults.MaxCharsPerColumnForGrid);
		_resultsSettings.MaxCharsPerColumnForText = ReadInt(SettingsConstants.Results.MaxCharsPerColumnForTextKey, ExecutionResultsSettings.Defaults.MaxCharsPerColumnForText);
		_resultsSettings.MaxCharsPerColumnForXml = ReadInt(SettingsConstants.Results.MaxCharsPerColumnForXmlKey, ExecutionResultsSettings.Defaults.MaxCharsPerColumnForXml);
		_resultsSettings.OutputQueryForGrid = ReadBoolean(SettingsConstants.Results.OutputQueryForGridKey, ExecutionResultsSettings.Defaults.OutputQueryForGrid);
		_resultsSettings.OutputQueryForText = ReadBoolean(SettingsConstants.Results.OutputQueryForTextKey, ExecutionResultsSettings.Defaults.OutputQueryForText);
		_resultsSettings.PrintColumnHeadersForText = ReadBoolean(SettingsConstants.Results.PrintColumnHeadersForTextKey, ExecutionResultsSettings.Defaults.PrintColumnHeadersForText);
		_resultsSettings.ProvideFeedbackWithSounds = ReadBoolean(SettingsConstants.Results.ProvideFeedbackWithSoundsKey, ExecutionResultsSettings.Defaults.ProvideFeedbackWithSounds);
		_resultsSettings.QuoteStringsContainingCommas = ReadBoolean(SettingsConstants.Results.QuoteStringsContainingCommasKey, ExecutionResultsSettings.Defaults.QuoteStringsContainingCommas);
		_resultsSettings.ResultsDirectory = ReadString(SettingsConstants.Results.ResultsDirectoryKey, ExecutionResultsSettings.Defaults.ResultsDirectory);
		_resultsSettings.RightAlignNumericsForText = ReadBoolean(SettingsConstants.Results.RightAlignNumericsForTextKey, ExecutionResultsSettings.Defaults.RightAlignNumericsForText);
		_resultsSettings.ScrollResultsAsReceivedForText = ReadBoolean(SettingsConstants.Results.ScrollResultsAsReceivedForTextKey, ExecutionResultsSettings.Defaults.ScrollResultsAsReceivedForText);
		_resultsSettings.ShowAllGridsInTheSameTab = ReadBoolean(SettingsConstants.Results.ShowAllGridsInTheSameTabKey, ExecutionResultsSettings.Defaults.ShowAllGridsInTheSameTab);
		_resultsSettings.ShowGridLinesInMap = ReadBoolean(SettingsConstants.Results.ShowGridLinesInMapKey, ExecutionResultsSettings.Defaults.ShowGridLinesInMap);
		_resultsSettings.ShowMessagesInNewTabForText = ReadBoolean(SettingsConstants.Results.ShowMessagesInNewTabForTextKey, ExecutionResultsSettings.Defaults.ShowMessagesInNewTabForText);
		_resultsSettings.SqlExecutionMode = ReadSqlExecutionMode(SettingsConstants.Results.SqlExecutionModeKey, ExecutionResultsSettings.Defaults.SqlExecutionMode);
		_resultsSettings.SwitchToResultsTabAfterQueryExecutesForGrid = ReadBoolean(SettingsConstants.Results.SwitchToResultsTabAfterQueryExecutesForGridKey, ExecutionResultsSettings.Defaults.SwitchToResultsTabAfterQueryExecutesForGrid);
		_resultsSettings.SwitchToResultsTabAfterQueryExecutesForText = ReadBoolean(SettingsConstants.Results.SwitchToResultsTabAfterQueryExecutesForTextKey, ExecutionResultsSettings.Defaults.SwitchToResultsTabAfterQueryExecutesForText);
		_editorContextSettings.StatusBarPosition = ReadStatusBarPosition(SettingsConstants.EditorContext.StatusBarPositionKey, EditorContextSettings.Defaults.StatusBarPosition);
		return this;
	}

	public void Save(IUserSettings settings)
	{
		PersistBoolean(SettingsConstants.General.PromptForSaveWhenClosingQueryWindowsKey, _generalSettings.PromptForSaveWhenClosingQueryWindows);
		PersistDisplayTimeOptions(SettingsConstants.StatusBar.DisplayTimeOptionKey, _statusBarSettings.ShowTimeOption);
		PersistBoolean(SettingsConstants.StatusBar.StatusBarIncludeServerNameKey, _statusBarSettings.StatusBarIncludeServerName);
		PersistBoolean(SettingsConstants.StatusBar.StatusBarIncludeDatabaseNameKey, _statusBarSettings.StatusBarIncludeDatabaseName);
		PersistBoolean(SettingsConstants.StatusBar.StatusBarIncludeLoginNameKey, _statusBarSettings.StatusBarIncludeLoginName);
		PersistBoolean(SettingsConstants.StatusBar.StatusBarIncludeRowCountKey, _statusBarSettings.StatusBarIncludeRowCount);
		PersistBoolean(SettingsConstants.StatusBar.StatusBarBackgroundColorIsNamedColor, _statusBarSettings.StatusBarColor.IsNamedColor);
		if (_statusBarSettings.StatusBarColor.IsNamedColor)
		{
			PersistString(SettingsConstants.StatusBar.StatusBarBackgroundColor, _statusBarSettings.StatusBarColor.Name);
		}
		else
		{
			PersistString(SettingsConstants.StatusBar.StatusBarBackgroundColor, _statusBarSettings.StatusBarColor.ToArgb().ToString(CultureInfo.InvariantCulture));
		}

		PersistBoolean(SettingsConstants.StatusBar.TabTextIncludeServerNameKey, _statusBarSettings.TabTextIncludeServerName);
		PersistBoolean(SettingsConstants.StatusBar.TabTextIncludeDatabaseNameKey, _statusBarSettings.TabTextIncludeDatabaseName);
		PersistBoolean(SettingsConstants.StatusBar.TabTextIncludeLoginNameKey, _statusBarSettings.TabTextIncludeLoginName);
		PersistBoolean(SettingsConstants.StatusBar.TabTextIncludeFileNameKey, _statusBarSettings.TabTextIncludeFileName);
		PersistString(SettingsConstants.Execution.BatchSeparatorKey, _executionSettings.BatchSeparator);
		PersistBoolean(SettingsConstants.Execution.DisconnectAfterQueryExecutesKey, _executionSettings.DisconnectAfterQueryExecutes);
		PersistInt(SettingsConstants.Execution.ExecutionTimeoutKey, _executionSettings.ExecutionTimeout);
		PersistBoolean(SettingsConstants.Execution.OLESQLScriptingByDefaultKey, _executionSettings.OLESQLScriptingByDefault);
		PersistBoolean(SettingsConstants.Execution.SetAnsiNullDefaultKey, _executionSettings.SetAnsiNullDefault);
		PersistBoolean(SettingsConstants.Execution.SetAnsiNullsKey, _executionSettings.SetAnsiNulls);
		PersistBoolean(SettingsConstants.Execution.SetAnsiPaddingKey, _executionSettings.SetAnsiPadding);
		PersistBoolean(SettingsConstants.Execution.SetAnsiWarningsKey, _executionSettings.SetAnsiWarnings);
		PersistBoolean(SettingsConstants.Execution.SetArithAbortKey, _executionSettings.SetArithAbort);
		PersistBoolean(SettingsConstants.Execution.SetConcatenationNullKey, _executionSettings.SetConcatenationNull);
		PersistBoolean(SettingsConstants.Execution.SetCursorCloseOnCommitKey, _executionSettings.SetCursorCloseOnCommit);
		PersistBoolean(SettingsConstants.Execution.SetDeadlockPriorityLowKey, _executionSettings.SetDeadlockPriorityLow);
		PersistBoolean(SettingsConstants.Execution.SetFmtOnlyKey, _executionSettings.SetFmtOnly);
		PersistBoolean(SettingsConstants.Execution.SetForceplanKey, _executionSettings.SetForceplan);
		PersistBoolean(SettingsConstants.Execution.SetImplicitTransactionKey, _executionSettings.SetImplicitTransaction);
		PersistInt(SettingsConstants.Execution.SetLockTimeoutKey, _executionSettings.SetLockTimeout);
		PersistBoolean(SettingsConstants.Execution.SetNoCountKey, _executionSettings.SetNoCount);
		PersistBoolean(SettingsConstants.Execution.SetNoExecKey, _executionSettings.SetNoExec);
		PersistBoolean(SettingsConstants.Execution.SetNumericAbortKey, _executionSettings.SetNumericAbort);
		PersistBoolean(SettingsConstants.Execution.SetParseOnlyKey, _executionSettings.SetParseOnly);
		PersistInt(SettingsConstants.Execution.SetQueryGovernorCostKey, _executionSettings.SetQueryGovernorCost);
		PersistBoolean(SettingsConstants.Execution.SetQuotedIdentifierKey, _executionSettings.SetQuotedIdentifier);
		PersistInt(SettingsConstants.Execution.SetRowCountKey, _executionSettings.SetRowCount);
		PersistBoolean(SettingsConstants.Execution.SetShowplanTextKey, _executionSettings.SetShowplanText);
		PersistBoolean(SettingsConstants.Execution.SetStatisticsIOKey, _executionSettings.SetStatisticsIO);
		PersistBoolean(SettingsConstants.Execution.SetStatisticsProfileKey, _executionSettings.SetStatisticsProfile);
		PersistBoolean(SettingsConstants.Execution.SetStatisticsTimeKey, _executionSettings.SetStatisticsTime);
		PersistInt(SettingsConstants.Execution.SetTextSizeKey, _executionSettings.SetTextSize);
		PersistString(SettingsConstants.Execution.SetTransactionIsolationLevelKey, _executionSettings.SetTransactionIsolationLevel);
		PersistBoolean(SettingsConstants.Execution.SetXACTAbortKey, _executionSettings.SetXACTAbort);
		PersistBoolean(SettingsConstants.Execution.SuppressProviderMessageHeadersKey, _executionSettings.SuppressProviderMessageHeaders);
		PersistChar(SettingsConstants.Results.ColumnDelimiterForTextKey, _resultsSettings.ColumnDelimiterForText);
		PersistBoolean(SettingsConstants.Results.DiscardResultsForGridKey, _resultsSettings.DiscardResultsForGrid);
		PersistBoolean(SettingsConstants.Results.DiscardResultsForTextKey, _resultsSettings.DiscardResultsForText);
		PersistBoolean(SettingsConstants.Results.DisplayResultInSeparateTabForGridKey, _resultsSettings.DisplayResultInSeparateTabForGrid);
		PersistBoolean(SettingsConstants.Results.DisplayResultInSeparateTabForTextKey, _resultsSettings.DisplayResultInSeparateTabForText);
		PersistBoolean(SettingsConstants.Results.IncludeColumnHeadersWhileSavingGridResultsKey, _resultsSettings.IncludeColumnHeadersWhileSavingGridResults);
		PersistInt(SettingsConstants.Results.MaxCharsPerColumnForGridKey, _resultsSettings.MaxCharsPerColumnForGrid);
		PersistInt(SettingsConstants.Results.MaxCharsPerColumnForTextKey, _resultsSettings.MaxCharsPerColumnForText);
		PersistInt(SettingsConstants.Results.MaxCharsPerColumnForXmlKey, _resultsSettings.MaxCharsPerColumnForXml);
		PersistBoolean(SettingsConstants.Results.OutputQueryForGridKey, _resultsSettings.OutputQueryForGrid);
		PersistBoolean(SettingsConstants.Results.OutputQueryForTextKey, _resultsSettings.OutputQueryForText);
		PersistBoolean(SettingsConstants.Results.PrintColumnHeadersForTextKey, _resultsSettings.PrintColumnHeadersForText);
		PersistBoolean(SettingsConstants.Results.ProvideFeedbackWithSoundsKey, _resultsSettings.ProvideFeedbackWithSounds);
		PersistBoolean(SettingsConstants.Results.QuoteStringsContainingCommasKey, _resultsSettings.QuoteStringsContainingCommas);
		PersistString(SettingsConstants.Results.ResultsDirectoryKey, _resultsSettings.ResultsDirectory);
		PersistBoolean(SettingsConstants.Results.RightAlignNumericsForTextKey, _resultsSettings.RightAlignNumericsForText);
		PersistBoolean(SettingsConstants.Results.ScrollResultsAsReceivedForTextKey, _resultsSettings.ScrollResultsAsReceivedForText);
		PersistBoolean(SettingsConstants.Results.ShowAllGridsInTheSameTabKey, _resultsSettings.ShowAllGridsInTheSameTab);
		PersistBoolean(SettingsConstants.Results.ShowGridLinesInMapKey, _resultsSettings.ShowGridLinesInMap);
		PersistBoolean(SettingsConstants.Results.ShowMessagesInNewTabForTextKey, _resultsSettings.ShowMessagesInNewTabForText);
		PersistSqlExecutionMode(SettingsConstants.Results.SqlExecutionModeKey, _resultsSettings.SqlExecutionMode);
		PersistBoolean(SettingsConstants.Results.SwitchToResultsTabAfterQueryExecutesForGridKey, _resultsSettings.SwitchToResultsTabAfterQueryExecutesForGrid);
		PersistBoolean(SettingsConstants.Results.SwitchToResultsTabAfterQueryExecutesForTextKey, _resultsSettings.SwitchToResultsTabAfterQueryExecutesForText);
		PersistStatusBarPosition(SettingsConstants.EditorContext.StatusBarPositionKey, _editorContextSettings.StatusBarPosition);
	}

	public void ResetToDefault()
	{
		_generalSettings.ResetToDefault();
		_executionSettings.ResetToDefault();
		_resultsSettings.ResetToDefault();
		_statusBarSettings.ResetToDefault();
		_editorContextSettings.ResetToDefault();
	}

	public object Clone()
	{
		IUserSettings obj = MemberwiseClone() as IUserSettings;
		obj.General = _generalSettings.Clone() as IGeneralSettings;
		obj.Execution = _executionSettings.Clone() as IQueryExecutionSettings;
		obj.ExecutionResults = _resultsSettings.Clone() as IQueryExecutionResultsSettings;
		obj.StatusBar = _statusBarSettings.Clone() as IEditorTabAndStatusBarSettings;
		obj.EditorContext = _editorContextSettings.Clone() as IEditorContextSettings;
		return obj;
	}

	private void PersistBoolean(string key, bool? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetBool(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value);
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistDisplayTimeOptions(string key, EnDisplayTimeOptions? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetString(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value.ToString());
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistString(string key, string value)
	{
		if (value != null)
		{
			SettingsStoreUtils.SetString(SettingsConstants.SqlEditorUserSettingsPath, key, value);
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistInt(string key, int? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetInt(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value);
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistSqlExecutionMode(string key, EnSqlExecutionMode? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetString(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value.ToString());
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistStatusBarPosition(string key, EnStatusBarPosition? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetString(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value.ToString());
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private void PersistChar(string key, char? value)
	{
		if (value.HasValue)
		{
			SettingsStoreUtils.SetString(SettingsConstants.SqlEditorUserSettingsPath, key, value.Value.ToString());
		}
		else
		{
			SettingsStoreUtils.DeleteProperty(SettingsConstants.SqlEditorUserSettingsPath, key);
		}
	}

	private bool ReadBoolean(string key, bool defaultValue)
	{
		return SettingsStoreUtils.GetBoolOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue);
	}

	private EnDisplayTimeOptions ReadDisplayTimeOptions(string key, EnDisplayTimeOptions defaultValue)
	{
		if (Enum.TryParse<EnDisplayTimeOptions>(SettingsStoreUtils.GetStringOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue.ToString()), ignoreCase: true, out var result))
		{
			return result;
		}

		return defaultValue;
	}

	private string ReadString(string key, string defaultValue)
	{
		string text = SettingsStoreUtils.GetStringOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue);
		text ??= defaultValue;

		return text;
	}

	private int ReadInt(string key, int defaultValue)
	{
		return SettingsStoreUtils.GetIntOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue);
	}

	private EnSqlExecutionMode ReadSqlExecutionMode(string key, EnSqlExecutionMode defaultValue)
	{
		if (Enum.TryParse<EnSqlExecutionMode>(SettingsStoreUtils.GetStringOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue.ToString()), ignoreCase: true, out var result))
		{
			return result;
		}

		return defaultValue;
	}

	private EnStatusBarPosition ReadStatusBarPosition(string key, EnStatusBarPosition defaultValue)
	{
		if (Enum.TryParse<EnStatusBarPosition>(SettingsStoreUtils.GetStringOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue.ToString()), ignoreCase: true, out var result))
		{
			return result;
		}

		return defaultValue;
	}

	private char ReadChar(string key, char defaultValue)
	{
		if (char.TryParse(SettingsStoreUtils.GetStringOrDefault(SettingsConstants.SqlEditorUserSettingsPath, key, defaultValue.ToString()), out var result))
		{
			return result;
		}

		return defaultValue;
	}
}
