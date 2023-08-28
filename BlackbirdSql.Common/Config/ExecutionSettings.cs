// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.ExecutionSettings

using System;
using System.Globalization;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Config;


public sealed class ExecutionSettings : IQueryExecutionSettings, ICloneable
{
	public static class Defaults
	{
		public static readonly string BatchSeparator = "GO";

		public static readonly bool DisconnectAfterQueryExecutes = false;

		public static readonly int ExecutionTimeout = 0;

		public static readonly bool OLESQLScriptingByDefault = false;

		public static readonly bool SetAnsiNullDefault = true;

		public static readonly bool SetAnsiNulls = true;

		public static readonly bool SetAnsiPadding = true;

		public static readonly bool SetAnsiWarnings = true;

		public static readonly bool SetArithAbort = true;

		public static readonly bool SetConcatenationNull = true;

		public static readonly bool SetCursorCloseOnCommit = false;

		public static readonly bool SetDeadlockPriorityLow = false;

		public static readonly bool SetFmtOnly = false;

		public static readonly bool SetForceplan = false;

		public static readonly bool SetImplicitTransaction = false;

		public static readonly int SetLockTimeout = -1;

		public static readonly bool SetNoCount = false;

		public static readonly bool SetNoExec = false;

		public static readonly bool SetNumericAbort = false;

		public static readonly bool SetParseOnly = false;

		public static readonly int SetQueryGovernorCost = 0;

		public static readonly bool SetQuotedIdentifier = true;

		public static readonly int SetRowCount = 0;

		public static readonly bool SetShowplanText = false;

		public static readonly bool SetStatisticsIO = false;

		public static readonly bool SetStatisticsProfile = false;

		public static readonly bool SetStatisticsTime = false;

		public static readonly int SetTextSize = int.MaxValue;

		public static readonly string SetTransactionIsolationLevel = "READ COMMITTED";

		public static readonly bool SetXACTAbort = false;

		public static readonly bool SuppressProviderMessageHeaders = true;
	}

	public class FormatStrings
	{
		public static readonly string SetAnsiNullDefaultString = "SET ANSI_NULL_DFLT_ON {0}";

		public static readonly string SetAnsiNullsString = "SET ANSI_NULLS {0}";

		public static readonly string SetEnableLedgerString = "SET ENABLELEDGER {0}";

		public static readonly string SetAnsiPaddingString = "SET ANSI_PADDING {0}";

		public static readonly string SetAnsiWarningsString = "SET ANSI_WARNINGS {0}";

		public static readonly string SetArithAbortString = "SET ARITHABORT {0}";

		public static readonly string SetConcatenationNullString = "SET CONCAT_NULL_YIELDS_NULL {0}";

		public static readonly string SetCursorCloseOnCommitString = "SET CURSOR_CLOSE_ON_COMMIT {0}";

		public static readonly string SetDeadlockPriorityLowString = "SET DEADLOCK_PRIORITY {0}";

		public static readonly string SetImplicitTransactionString = "SET IMPLICIT_TRANSACTIONS {0}";

		public static readonly string SetLockTimeoutString = "SET LOCK_TIMEOUT {0}";

		public static readonly string SetNoCountString = "SET NOCOUNT {0}";

		public static readonly string SetQueryGovernorCostString = "SET QUERY_GOVERNOR_COST_LIMIT {0}";

		public static readonly string SetQuotedIdentifierString = "SET QUOTED_IDENTIFIER {0}";

		public static readonly string SetRowCountString = "SET ROWCOUNT {0}";

		public static readonly string SetTextSizeString = "SET TEXTSIZE {0}";

		public static readonly string SetTransactionIsolationLevelString = "SET TRANSACTION ISOLATION LEVEL {0}";
	}

	private string _batchSeparator;

	private bool? _disconnectAfterQueryExecutes;

	private int? _executionTimeout;

	private bool? _oLESQLScriptingByDefault;

	private bool? _setAnsiNullDefault;

	private bool? _setAnsiNulls;

	private bool? _setAnsiPadding;

	private bool? _setAnsiWarnings;

	private bool? _setArithAbort;

	private bool? _setConcatenationNull;

	private bool? _setCursorCloseOnCommit;

	private bool? _setDeadlockPriorityLow;

	private bool? _setFmtOnly;

	private bool? _setForceplan;

	private bool? _setImplicitTransaction;

	private int? _setLockTimeout;

	private bool? _setNoCount;

	private bool? _setNoExec;

	private bool? _setNumericAbort;

	private bool? _setParseOnly;

	private int? _setQueryGovernorCost;

	private bool? _setQuotedIdentifier;

	private int? _setRowCount;

	private bool? _setShowplanText;

	private bool? _setStatisticsIO;

	private bool? _setStatisticsProfile;

	private bool? _setStatisticsTime;

	private int? _setTextSize;

	private string _setTransactionIsolationLevel;

	private bool? _setXACTAbort;

	private bool? _suppressProviderMessageHeaders;

	public string BatchSeparator
	{
		get
		{
			if (string.IsNullOrEmpty(_batchSeparator))
			{
				return Defaults.BatchSeparator;
			}

			return _batchSeparator;
		}
		set
		{
			_batchSeparator = value;
		}
	}

	public bool DisconnectAfterQueryExecutes
	{
		get
		{
			if (!_disconnectAfterQueryExecutes.HasValue)
			{
				return Defaults.DisconnectAfterQueryExecutes;
			}

			return _disconnectAfterQueryExecutes.Value;
		}
		set
		{
			_disconnectAfterQueryExecutes = value;
		}
	}

	public int ExecutionTimeout
	{
		get
		{
			if (!_executionTimeout.HasValue)
			{
				return Defaults.ExecutionTimeout;
			}

			return _executionTimeout.Value;
		}
		set
		{
			_executionTimeout = value;
		}
	}

	public bool OLESQLScriptingByDefault
	{
		get
		{
			if (!_oLESQLScriptingByDefault.HasValue)
			{
				return Defaults.OLESQLScriptingByDefault;
			}

			return _oLESQLScriptingByDefault.Value;
		}
		set
		{
			_oLESQLScriptingByDefault = value;
		}
	}

	public bool SetAnsiNullDefault
	{
		get
		{
			if (!_setAnsiNullDefault.HasValue)
			{
				return Defaults.SetAnsiNullDefault;
			}

			return _setAnsiNullDefault.Value;
		}
		set
		{
			_setAnsiNullDefault = value;
		}
	}

	public string SetAnsiNullDefaultString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiNullDefaultString, ConvertToOnOff(SetAnsiNullDefault));

	public bool SetAnsiNulls
	{
		get
		{
			if (!_setAnsiNulls.HasValue)
			{
				return Defaults.SetAnsiNulls;
			}

			return _setAnsiNulls.Value;
		}
		set
		{
			_setAnsiNulls = value;
		}
	}

	public string SetAnsiNullsString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiNullsString, ConvertToOnOff(SetAnsiNulls));

	public string SetEnableLedgerString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetEnableLedgerString, ConvertToOnOff(SetAnsiNulls));

	public bool SetAnsiPadding
	{
		get
		{
			if (!_setAnsiPadding.HasValue)
			{
				return Defaults.SetAnsiPadding;
			}

			return _setAnsiPadding.Value;
		}
		set
		{
			_setAnsiPadding = value;
		}
	}

	public string SetAnsiPaddingString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiPaddingString, ConvertToOnOff(SetAnsiPadding));

	public bool SetAnsiWarnings
	{
		get
		{
			if (!_setAnsiWarnings.HasValue)
			{
				return Defaults.SetAnsiWarnings;
			}

			return _setAnsiWarnings.Value;
		}
		set
		{
			_setAnsiWarnings = value;
		}
	}

	public string SetAnsiWarningsString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiWarningsString, ConvertToOnOff(SetAnsiWarnings));

	public bool SetArithAbort
	{
		get
		{
			if (!_setArithAbort.HasValue)
			{
				return Defaults.SetArithAbort;
			}

			return _setArithAbort.Value;
		}
		set
		{
			_setArithAbort = value;
		}
	}

	public string SetArithAbortString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetArithAbortString, ConvertToOnOff(SetArithAbort));

	public bool SetConcatenationNull
	{
		get
		{
			if (!_setConcatenationNull.HasValue)
			{
				return Defaults.SetConcatenationNull;
			}

			return _setConcatenationNull.Value;
		}
		set
		{
			_setConcatenationNull = value;
		}
	}

	public string SetConcatenationNullString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetConcatenationNullString, ConvertToOnOff(SetConcatenationNull));

	public bool SetCursorCloseOnCommit
	{
		get
		{
			if (!_setCursorCloseOnCommit.HasValue)
			{
				return Defaults.SetCursorCloseOnCommit;
			}

			return _setCursorCloseOnCommit.Value;
		}
		set
		{
			_setCursorCloseOnCommit = value;
		}
	}

	public string SetCursorCloseOnCommitString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetCursorCloseOnCommitString, ConvertToOnOff(SetCursorCloseOnCommit));

	public bool SetDeadlockPriorityLow
	{
		get
		{
			if (!_setDeadlockPriorityLow.HasValue)
			{
				return Defaults.SetDeadlockPriorityLow;
			}

			return _setDeadlockPriorityLow.Value;
		}
		set
		{
			_setDeadlockPriorityLow = value;
		}
	}

	public string SetDeadlockPriorityLowString
	{
		get
		{
			if (SetDeadlockPriorityLow)
			{
				return string.Format(CultureInfo.InvariantCulture, FormatStrings.SetDeadlockPriorityLowString, "LOW");
			}

			return string.Format(CultureInfo.InvariantCulture, FormatStrings.SetDeadlockPriorityLowString, "NORMAL");
		}
	}

	public bool SetFmtOnly
	{
		get
		{
			if (!_setFmtOnly.HasValue)
			{
				return Defaults.SetFmtOnly;
			}

			return _setFmtOnly.Value;
		}
		set
		{
			_setFmtOnly = value;
		}
	}

	public bool SetForceplan
	{
		get
		{
			if (!_setForceplan.HasValue)
			{
				return Defaults.SetForceplan;
			}

			return _setForceplan.Value;
		}
		set
		{
			_setForceplan = value;
		}
	}

	public bool SetImplicitTransaction
	{
		get
		{
			if (!_setImplicitTransaction.HasValue)
			{
				return Defaults.SetImplicitTransaction;
			}

			return _setImplicitTransaction.Value;
		}
		set
		{
			_setImplicitTransaction = value;
		}
	}

	public string SetImplicitTransactionString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetImplicitTransactionString, ConvertToOnOff(SetImplicitTransaction));

	public int SetLockTimeout
	{
		get
		{
			if (!_setLockTimeout.HasValue)
			{
				return Defaults.SetLockTimeout;
			}

			return _setLockTimeout.Value;
		}
		set
		{
			_setLockTimeout = value;
		}
	}

	public string SetLockTimeoutString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetLockTimeoutString, SetLockTimeout);

	public bool SetNoCount
	{
		get
		{
			if (!_setNoCount.HasValue)
			{
				return Defaults.SetNoCount;
			}

			return _setNoCount.Value;
		}
		set
		{
			_setNoCount = value;
		}
	}

	public string SetNoCountString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetNoCountString, ConvertToOnOff(SetNoCount));

	public bool SetNoExec
	{
		get
		{
			if (!_setNoExec.HasValue)
			{
				return Defaults.SetNoExec;
			}

			return _setNoExec.Value;
		}
		set
		{
			_setNoExec = value;
		}
	}

	public bool SetNumericAbort
	{
		get
		{
			if (!_setNumericAbort.HasValue)
			{
				return Defaults.SetNumericAbort;
			}

			return _setNumericAbort.Value;
		}
		set
		{
			_setNumericAbort = value;
		}
	}

	public bool SetParseOnly
	{
		get
		{
			if (!_setParseOnly.HasValue)
			{
				return Defaults.SetParseOnly;
			}

			return _setParseOnly.Value;
		}
		set
		{
			_setParseOnly = value;
		}
	}

	public int SetQueryGovernorCost
	{
		get
		{
			if (!_setQueryGovernorCost.HasValue)
			{
				return Defaults.SetQueryGovernorCost;
			}

			return _setQueryGovernorCost.Value;
		}
		set
		{
			_setQueryGovernorCost = value;
		}
	}

	public string SetQueryGovernorCostString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetQueryGovernorCostString, SetQueryGovernorCost);

	public bool SetQuotedIdentifier
	{
		get
		{
			if (!_setQuotedIdentifier.HasValue)
			{
				return Defaults.SetQuotedIdentifier;
			}

			return _setQuotedIdentifier.Value;
		}
		set
		{
			_setQuotedIdentifier = value;
		}
	}

	public string SetQuotedIdentifierString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetQuotedIdentifierString, ConvertToOnOff(SetQuotedIdentifier));

	public int SetRowCount
	{
		get
		{
			if (!_setRowCount.HasValue)
			{
				return Defaults.SetRowCount;
			}

			return _setRowCount.Value;
		}
		set
		{
			_setRowCount = value;
		}
	}

	public string SetRowCountString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetRowCountString, SetRowCount);

	public bool SetShowplanText
	{
		get
		{
			if (!_setShowplanText.HasValue)
			{
				return Defaults.SetShowplanText;
			}

			return _setShowplanText.Value;
		}
		set
		{
			_setShowplanText = value;
		}
	}

	public bool SetStatisticsIO
	{
		get
		{
			if (!_setStatisticsIO.HasValue)
			{
				return Defaults.SetStatisticsIO;
			}

			return _setStatisticsIO.Value;
		}
		set
		{
			_setStatisticsIO = value;
		}
	}

	public bool SetStatisticsProfile
	{
		get
		{
			if (!_setStatisticsProfile.HasValue)
			{
				return Defaults.SetStatisticsProfile;
			}

			return _setStatisticsProfile.Value;
		}
		set
		{
			_setStatisticsProfile = value;
		}
	}

	public bool SetStatisticsTime
	{
		get
		{
			if (!_setStatisticsTime.HasValue)
			{
				return Defaults.SetStatisticsTime;
			}

			return _setStatisticsTime.Value;
		}
		set
		{
			_setStatisticsTime = value;
		}
	}

	public int SetTextSize
	{
		get
		{
			if (!_setTextSize.HasValue)
			{
				return Defaults.SetTextSize;
			}

			return _setTextSize.Value;
		}
		set
		{
			_setTextSize = value;
		}
	}

	public string SetTextSizeString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetTextSizeString, SetTextSize);

	public string SetTransactionIsolationLevel
	{
		get
		{
			if (string.IsNullOrEmpty(_setTransactionIsolationLevel))
			{
				return Defaults.SetTransactionIsolationLevel;
			}

			return _setTransactionIsolationLevel;
		}
		set
		{
			_setTransactionIsolationLevel = value;
		}
	}

	public string SetTransactionIsolationLevelString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetTransactionIsolationLevelString, SetTransactionIsolationLevel);

	public bool SetXACTAbort
	{
		get
		{
			if (!_setXACTAbort.HasValue)
			{
				return Defaults.SetXACTAbort;
			}

			return _setXACTAbort.Value;
		}
		set
		{
			_setXACTAbort = value;
		}
	}

	public bool SuppressProviderMessageHeaders
	{
		get
		{
			if (!_suppressProviderMessageHeaders.HasValue)
			{
				return Defaults.SuppressProviderMessageHeaders;
			}

			return _suppressProviderMessageHeaders.Value;
		}
		set
		{
			_suppressProviderMessageHeaders = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void ResetToDefault()
	{
		_batchSeparator = Defaults.BatchSeparator;
		_disconnectAfterQueryExecutes = Defaults.DisconnectAfterQueryExecutes;
		_executionTimeout = Defaults.ExecutionTimeout;
		_oLESQLScriptingByDefault = Defaults.OLESQLScriptingByDefault;
		_setAnsiNullDefault = Defaults.SetAnsiNullDefault;
		_setAnsiNulls = Defaults.SetAnsiNulls;
		_setAnsiPadding = Defaults.SetAnsiPadding;
		_setAnsiWarnings = Defaults.SetAnsiWarnings;
		_setArithAbort = Defaults.SetArithAbort;
		_setConcatenationNull = Defaults.SetConcatenationNull;
		_setCursorCloseOnCommit = Defaults.SetCursorCloseOnCommit;
		_setDeadlockPriorityLow = Defaults.SetDeadlockPriorityLow;
		_setFmtOnly = Defaults.SetFmtOnly;
		_setForceplan = Defaults.SetForceplan;
		_setImplicitTransaction = Defaults.SetImplicitTransaction;
		_setLockTimeout = Defaults.SetLockTimeout;
		_setNoCount = Defaults.SetNoCount;
		_setNoExec = Defaults.SetNoExec;
		_setNumericAbort = Defaults.SetNumericAbort;
		_setParseOnly = Defaults.SetParseOnly;
		_setQueryGovernorCost = Defaults.SetQueryGovernorCost;
		_setQuotedIdentifier = Defaults.SetQuotedIdentifier;
		_setRowCount = Defaults.SetRowCount;
		_setShowplanText = Defaults.SetShowplanText;
		_setStatisticsIO = Defaults.SetStatisticsIO;
		_setStatisticsProfile = Defaults.SetStatisticsProfile;
		_setStatisticsTime = Defaults.SetStatisticsTime;
		_setTextSize = Defaults.SetTextSize;
		_setTransactionIsolationLevel = Defaults.SetTransactionIsolationLevel;
		_setXACTAbort = Defaults.SetXACTAbort;
		_suppressProviderMessageHeaders = Defaults.SuppressProviderMessageHeaders;
	}

	private string ConvertToOnOff(bool enable)
	{
		if (!enable)
		{
			return "OFF";
		}

		return "ON";
	}
}
