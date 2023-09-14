#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

namespace BlackbirdSql.Common.Ctl.Interfaces;


public interface IBQueryExecutionSettings : ICloneable
{
	string BatchSeparator { get; set; }

	bool DisconnectAfterQueryExecutes { get; set; }

	int ExecutionTimeout { get; set; }

	bool OLESQLScriptingByDefault { get; set; }

	bool SetAnsiNullDefault { get; set; }

	string SetAnsiNullDefaultString { get; }

	bool SetAnsiNulls { get; set; }

	string SetEnableLedgerString { get; }

	string SetAnsiNullsString { get; }

	bool SetAnsiPadding { get; set; }

	string SetAnsiPaddingString { get; }

	bool SetAnsiWarnings { get; set; }

	string SetAnsiWarningsString { get; }

	bool SetArithAbort { get; set; }

	string SetArithAbortString { get; }

	bool SetConcatenationNull { get; set; }

	string SetConcatenationNullString { get; }

	bool SetCursorCloseOnCommit { get; set; }

	string SetCursorCloseOnCommitString { get; }

	bool SetDeadlockPriorityLow { get; set; }

	string SetDeadlockPriorityLowString { get; }

	bool SetFmtOnly { get; set; }

	bool SetForceplan { get; set; }

	bool SetImplicitTransaction { get; set; }

	string SetImplicitTransactionString { get; }

	int SetLockTimeout { get; set; }

	string SetLockTimeoutString { get; }

	bool SetNoCount { get; set; }

	string SetNoCountString { get; }

	bool SetNoExec { get; set; }

	bool SetNumericAbort { get; set; }

	bool SetParseOnly { get; set; }

	int SetQueryGovernorCost { get; set; }

	string SetQueryGovernorCostString { get; }

	bool SetQuotedIdentifier { get; set; }

	string SetQuotedIdentifierString { get; }

	int SetRowCount { get; set; }

	string SetRowCountString { get; }

	bool SetShowplanText { get; set; }

	bool SetStatisticsIO { get; set; }

	bool SetStatisticsProfile { get; set; }

	bool SetStatisticsTime { get; set; }

	int SetTextSize { get; set; }

	string SetTextSizeString { get; }

	string SetTransactionIsolationLevel { get; set; }

	string SetTransactionIsolationLevelString { get; }

	bool SetXACTAbort { get; set; }

	bool SuppressProviderMessageHeaders { get; set; }

	void ResetToDefault();
}
