// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.ExecutionSettings

using System;
using System.Globalization;
using BlackbirdSql.Common.Config.Interfaces;



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

	private string _BatchSeparator;

	private bool? _DisconnectAfterQueryExecutes;

	private int? _ExecutionTimeout;

	private bool? _OLESQLScriptingByDefault;

	private bool? _SetAnsiNullDefault;

	private bool? _SetAnsiNulls;

	private bool? _SetAnsiPadding;

	private bool? _SetAnsiWarnings;

	private bool? _SetArithAbort;

	private bool? _SetConcatenationNull;

	private bool? _SetCursorCloseOnCommit;

	private bool? _SetDeadlockPriorityLow;

	private bool? _SetFmtOnly;

	private bool? _SetForceplan;

	private bool? _SetImplicitTransaction;

	private int? _SetLockTimeout;

	private bool? _SetNoCount;

	private bool? _SetNoExec;

	private bool? _SetNumericAbort;

	private bool? _SetParseOnly;

	private int? _SetQueryGovernorCost;

	private bool? _SetQuotedIdentifier;

	private int? _SetRowCount;

	private bool? _SetShowplanText;

	private bool? _SetStatisticsIO;

	private bool? _SetStatisticsProfile;

	private bool? _SetStatisticsTime;

	private int? _SetTextSize;

	private string _SetTransactionIsolationLevel;

	private bool? _SetXACTAbort;

	private bool? _suppressProviderMessageHeaders;

	public string BatchSeparator
	{
		get
		{
			if (string.IsNullOrEmpty(_BatchSeparator))
			{
				return Defaults.BatchSeparator;
			}

			return _BatchSeparator;
		}
		set
		{
			_BatchSeparator = value;
		}
	}

	public bool DisconnectAfterQueryExecutes
	{
		get
		{
			if (!_DisconnectAfterQueryExecutes.HasValue)
			{
				return Defaults.DisconnectAfterQueryExecutes;
			}

			return _DisconnectAfterQueryExecutes.Value;
		}
		set
		{
			_DisconnectAfterQueryExecutes = value;
		}
	}

	public int ExecutionTimeout
	{
		get
		{
			if (!_ExecutionTimeout.HasValue)
			{
				return Defaults.ExecutionTimeout;
			}

			return _ExecutionTimeout.Value;
		}
		set
		{
			_ExecutionTimeout = value;
		}
	}

	public bool OLESQLScriptingByDefault
	{
		get
		{
			if (!_OLESQLScriptingByDefault.HasValue)
			{
				return Defaults.OLESQLScriptingByDefault;
			}

			return _OLESQLScriptingByDefault.Value;
		}
		set
		{
			_OLESQLScriptingByDefault = value;
		}
	}

	public bool SetAnsiNullDefault
	{
		get
		{
			if (!_SetAnsiNullDefault.HasValue)
			{
				return Defaults.SetAnsiNullDefault;
			}

			return _SetAnsiNullDefault.Value;
		}
		set
		{
			_SetAnsiNullDefault = value;
		}
	}

	public string SetAnsiNullDefaultString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiNullDefaultString, ConvertToOnOff(SetAnsiNullDefault));

	public bool SetAnsiNulls
	{
		get
		{
			if (!_SetAnsiNulls.HasValue)
			{
				return Defaults.SetAnsiNulls;
			}

			return _SetAnsiNulls.Value;
		}
		set
		{
			_SetAnsiNulls = value;
		}
	}

	public string SetAnsiNullsString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiNullsString, ConvertToOnOff(SetAnsiNulls));

	public string SetEnableLedgerString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetEnableLedgerString, ConvertToOnOff(SetAnsiNulls));

	public bool SetAnsiPadding
	{
		get
		{
			if (!_SetAnsiPadding.HasValue)
			{
				return Defaults.SetAnsiPadding;
			}

			return _SetAnsiPadding.Value;
		}
		set
		{
			_SetAnsiPadding = value;
		}
	}

	public string SetAnsiPaddingString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiPaddingString, ConvertToOnOff(SetAnsiPadding));

	public bool SetAnsiWarnings
	{
		get
		{
			if (!_SetAnsiWarnings.HasValue)
			{
				return Defaults.SetAnsiWarnings;
			}

			return _SetAnsiWarnings.Value;
		}
		set
		{
			_SetAnsiWarnings = value;
		}
	}

	public string SetAnsiWarningsString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetAnsiWarningsString, ConvertToOnOff(SetAnsiWarnings));

	public bool SetArithAbort
	{
		get
		{
			if (!_SetArithAbort.HasValue)
			{
				return Defaults.SetArithAbort;
			}

			return _SetArithAbort.Value;
		}
		set
		{
			_SetArithAbort = value;
		}
	}

	public string SetArithAbortString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetArithAbortString, ConvertToOnOff(SetArithAbort));

	public bool SetConcatenationNull
	{
		get
		{
			if (!_SetConcatenationNull.HasValue)
			{
				return Defaults.SetConcatenationNull;
			}

			return _SetConcatenationNull.Value;
		}
		set
		{
			_SetConcatenationNull = value;
		}
	}

	public string SetConcatenationNullString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetConcatenationNullString, ConvertToOnOff(SetConcatenationNull));

	public bool SetCursorCloseOnCommit
	{
		get
		{
			if (!_SetCursorCloseOnCommit.HasValue)
			{
				return Defaults.SetCursorCloseOnCommit;
			}

			return _SetCursorCloseOnCommit.Value;
		}
		set
		{
			_SetCursorCloseOnCommit = value;
		}
	}

	public string SetCursorCloseOnCommitString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetCursorCloseOnCommitString, ConvertToOnOff(SetCursorCloseOnCommit));

	public bool SetDeadlockPriorityLow
	{
		get
		{
			if (!_SetDeadlockPriorityLow.HasValue)
			{
				return Defaults.SetDeadlockPriorityLow;
			}

			return _SetDeadlockPriorityLow.Value;
		}
		set
		{
			_SetDeadlockPriorityLow = value;
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
			if (!_SetFmtOnly.HasValue)
			{
				return Defaults.SetFmtOnly;
			}

			return _SetFmtOnly.Value;
		}
		set
		{
			_SetFmtOnly = value;
		}
	}

	public bool SetForceplan
	{
		get
		{
			if (!_SetForceplan.HasValue)
			{
				return Defaults.SetForceplan;
			}

			return _SetForceplan.Value;
		}
		set
		{
			_SetForceplan = value;
		}
	}

	public bool SetImplicitTransaction
	{
		get
		{
			if (!_SetImplicitTransaction.HasValue)
			{
				return Defaults.SetImplicitTransaction;
			}

			return _SetImplicitTransaction.Value;
		}
		set
		{
			_SetImplicitTransaction = value;
		}
	}

	public string SetImplicitTransactionString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetImplicitTransactionString, ConvertToOnOff(SetImplicitTransaction));

	public int SetLockTimeout
	{
		get
		{
			if (!_SetLockTimeout.HasValue)
			{
				return Defaults.SetLockTimeout;
			}

			return _SetLockTimeout.Value;
		}
		set
		{
			_SetLockTimeout = value;
		}
	}

	public string SetLockTimeoutString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetLockTimeoutString, SetLockTimeout);

	public bool SetNoCount
	{
		get
		{
			if (!_SetNoCount.HasValue)
			{
				return Defaults.SetNoCount;
			}

			return _SetNoCount.Value;
		}
		set
		{
			_SetNoCount = value;
		}
	}

	public string SetNoCountString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetNoCountString, ConvertToOnOff(SetNoCount));

	public bool SetNoExec
	{
		get
		{
			if (!_SetNoExec.HasValue)
			{
				return Defaults.SetNoExec;
			}

			return _SetNoExec.Value;
		}
		set
		{
			_SetNoExec = value;
		}
	}

	public bool SetNumericAbort
	{
		get
		{
			if (!_SetNumericAbort.HasValue)
			{
				return Defaults.SetNumericAbort;
			}

			return _SetNumericAbort.Value;
		}
		set
		{
			_SetNumericAbort = value;
		}
	}

	public bool SetParseOnly
	{
		get
		{
			if (!_SetParseOnly.HasValue)
			{
				return Defaults.SetParseOnly;
			}

			return _SetParseOnly.Value;
		}
		set
		{
			_SetParseOnly = value;
		}
	}

	public int SetQueryGovernorCost
	{
		get
		{
			if (!_SetQueryGovernorCost.HasValue)
			{
				return Defaults.SetQueryGovernorCost;
			}

			return _SetQueryGovernorCost.Value;
		}
		set
		{
			_SetQueryGovernorCost = value;
		}
	}

	public string SetQueryGovernorCostString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetQueryGovernorCostString, SetQueryGovernorCost);

	public bool SetQuotedIdentifier
	{
		get
		{
			if (!_SetQuotedIdentifier.HasValue)
			{
				return Defaults.SetQuotedIdentifier;
			}

			return _SetQuotedIdentifier.Value;
		}
		set
		{
			_SetQuotedIdentifier = value;
		}
	}

	public string SetQuotedIdentifierString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetQuotedIdentifierString, ConvertToOnOff(SetQuotedIdentifier));

	public int SetRowCount
	{
		get
		{
			if (!_SetRowCount.HasValue)
			{
				return Defaults.SetRowCount;
			}

			return _SetRowCount.Value;
		}
		set
		{
			_SetRowCount = value;
		}
	}

	public string SetRowCountString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetRowCountString, SetRowCount);

	public bool SetShowplanText
	{
		get
		{
			if (!_SetShowplanText.HasValue)
			{
				return Defaults.SetShowplanText;
			}

			return _SetShowplanText.Value;
		}
		set
		{
			_SetShowplanText = value;
		}
	}

	public bool SetStatisticsIO
	{
		get
		{
			if (!_SetStatisticsIO.HasValue)
			{
				return Defaults.SetStatisticsIO;
			}

			return _SetStatisticsIO.Value;
		}
		set
		{
			_SetStatisticsIO = value;
		}
	}

	public bool SetStatisticsProfile
	{
		get
		{
			if (!_SetStatisticsProfile.HasValue)
			{
				return Defaults.SetStatisticsProfile;
			}

			return _SetStatisticsProfile.Value;
		}
		set
		{
			_SetStatisticsProfile = value;
		}
	}

	public bool SetStatisticsTime
	{
		get
		{
			if (!_SetStatisticsTime.HasValue)
			{
				return Defaults.SetStatisticsTime;
			}

			return _SetStatisticsTime.Value;
		}
		set
		{
			_SetStatisticsTime = value;
		}
	}

	public int SetTextSize
	{
		get
		{
			if (!_SetTextSize.HasValue)
			{
				return Defaults.SetTextSize;
			}

			return _SetTextSize.Value;
		}
		set
		{
			_SetTextSize = value;
		}
	}

	public string SetTextSizeString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetTextSizeString, SetTextSize);

	public string SetTransactionIsolationLevel
	{
		get
		{
			if (string.IsNullOrEmpty(_SetTransactionIsolationLevel))
			{
				return Defaults.SetTransactionIsolationLevel;
			}

			return _SetTransactionIsolationLevel;
		}
		set
		{
			_SetTransactionIsolationLevel = value;
		}
	}

	public string SetTransactionIsolationLevelString => string.Format(CultureInfo.InvariantCulture, FormatStrings.SetTransactionIsolationLevelString, SetTransactionIsolationLevel);

	public bool SetXACTAbort
	{
		get
		{
			if (!_SetXACTAbort.HasValue)
			{
				return Defaults.SetXACTAbort;
			}

			return _SetXACTAbort.Value;
		}
		set
		{
			_SetXACTAbort = value;
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
		_BatchSeparator = Defaults.BatchSeparator;
		_DisconnectAfterQueryExecutes = Defaults.DisconnectAfterQueryExecutes;
		_ExecutionTimeout = Defaults.ExecutionTimeout;
		_OLESQLScriptingByDefault = Defaults.OLESQLScriptingByDefault;
		_SetAnsiNullDefault = Defaults.SetAnsiNullDefault;
		_SetAnsiNulls = Defaults.SetAnsiNulls;
		_SetAnsiPadding = Defaults.SetAnsiPadding;
		_SetAnsiWarnings = Defaults.SetAnsiWarnings;
		_SetArithAbort = Defaults.SetArithAbort;
		_SetConcatenationNull = Defaults.SetConcatenationNull;
		_SetCursorCloseOnCommit = Defaults.SetCursorCloseOnCommit;
		_SetDeadlockPriorityLow = Defaults.SetDeadlockPriorityLow;
		_SetFmtOnly = Defaults.SetFmtOnly;
		_SetForceplan = Defaults.SetForceplan;
		_SetImplicitTransaction = Defaults.SetImplicitTransaction;
		_SetLockTimeout = Defaults.SetLockTimeout;
		_SetNoCount = Defaults.SetNoCount;
		_SetNoExec = Defaults.SetNoExec;
		_SetNumericAbort = Defaults.SetNumericAbort;
		_SetParseOnly = Defaults.SetParseOnly;
		_SetQueryGovernorCost = Defaults.SetQueryGovernorCost;
		_SetQuotedIdentifier = Defaults.SetQuotedIdentifier;
		_SetRowCount = Defaults.SetRowCount;
		_SetShowplanText = Defaults.SetShowplanText;
		_SetStatisticsIO = Defaults.SetStatisticsIO;
		_SetStatisticsProfile = Defaults.SetStatisticsProfile;
		_SetStatisticsTime = Defaults.SetStatisticsTime;
		_SetTextSize = Defaults.SetTextSize;
		_SetTransactionIsolationLevel = Defaults.SetTransactionIsolationLevel;
		_SetXACTAbort = Defaults.SetXACTAbort;
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
