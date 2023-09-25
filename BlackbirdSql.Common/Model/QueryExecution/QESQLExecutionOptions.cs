#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.QueryExecution;


public class QESQLExecutionOptions : ICloneable
{
	private readonly BitArray _ExecOptions;

	private string _batchSeparator = "GO";

	private int _ExecTimeout = AbstractQESQLExec.DefaultSqlExecTimeout;

	private const string C_On = "ON";
	private const string C_Off = "OFF";
	private const string C_SetNoExec = "SET PLANONLY {0}"; // "SET NOEXEC {0}";
	private const string C_SetShowplanText = "SET SHOWPLAN_TEXT {0}";
	private const string C_SetStatisticsTime = "SET STATISTICS TIME {0}";
	private const string C_SetStatisticsIO = "SET STATISTICS IO {0}";
	private const string C_SetStatisticsProfile = "SET STATISTICS PROFILE {0}";
	private const string C_SetParseOnly = "SET PARSEONLY {0}";
	private const string C_SetExecutionPlanAll = "SET SHOWPLAN_ALL {0}";
	private const string C_SetExecutionPlanXml = "SET EXPLAIN {0}"; // "SET SHOWPLAN_XML {0}";
	private const string C_SetStatisticsXml = "SET STATS {0}"; // "SET STATISTICS XML {0}";
	private const string C_ResetOptions = "SET NOEXEC, PARSEONLY, FMTONLY OFF";

	private static readonly Dictionary<string, bool> _Supported = new()
	{
		{ C_SetNoExec, false },
		{ C_SetShowplanText, false },
		{ C_SetStatisticsTime, false },
		{ C_SetStatisticsIO, false },
		{ C_SetStatisticsProfile, false },
		{ C_SetParseOnly, false },
		{ C_SetExecutionPlanAll, false },
		{ C_SetExecutionPlanXml, false },
		{ C_SetStatisticsXml, false },
		{ C_ResetOptions, false }
	};


	public bool WithExecutionPlan
	{
		get
		{
			return _ExecOptions[0];
		}
		set
		{
			_ExecOptions[0] = value;
		}
	}

	public bool WithClientStats
	{
		get
		{
			return _ExecOptions[1];
		}
		set
		{
			_ExecOptions[1] = value;
		}
	}

	public bool WithProfiling
	{
		get
		{
			return _ExecOptions[2];
		}
		set
		{
			_ExecOptions[2] = value;
		}
	}

	public bool ParseOnly
	{
		get
		{
			return _ExecOptions[3];
		}
		set
		{
			_ExecOptions[3] = value;
		}
	}

	public bool WithNoExec
	{
		get
		{
			return _ExecOptions[4];
		}
		set
		{
			_ExecOptions[4] = value;
		}
	}

	public bool WithExecutionPlanText
	{
		get
		{
			return _ExecOptions[5];
		}
		set
		{
			_ExecOptions[5] = value;
		}
	}

	public bool WithStatisticsTime
	{
		get
		{
			return _ExecOptions[6];
		}
		set
		{
			_ExecOptions[6] = value;
		}
	}

	public bool WithStatisticsIO
	{
		get
		{
			return _ExecOptions[7];
		}
		set
		{
			_ExecOptions[7] = value;
		}
	}

	public bool WithStatisticsProfile
	{
		get
		{
			return _ExecOptions[8];
		}
		set
		{
			_ExecOptions[8] = value;
		}
	}

	public bool WithEstimatedExecutionPlan
	{
		get
		{
			return _ExecOptions[9];
		}
		set
		{
			_ExecOptions[9] = value;
		}
	}

	public bool WithOleSqlScripting
	{
		get
		{
			return _ExecOptions[13];
		}
		set
		{
			_ExecOptions[13] = value;
		}
	}

	public bool SuppressProviderMessageHeaders
	{
		get
		{
			return _ExecOptions[14];
		}
		set
		{
			_ExecOptions[14] = value;
		}
	}

	public bool WithDebugging
	{
		get
		{
			return _ExecOptions[15];
		}
		set
		{
			_ExecOptions[15] = value;
		}
	}

	public string BatchSeparator
	{
		get
		{
			return _batchSeparator;
		}
		set
		{
			_batchSeparator = value;
		}
	}

	public int ExecutionTimeout
	{
		get
		{
			return _ExecTimeout;
		}
		set
		{
			_ExecTimeout = value;
		}
	}

	public QESQLExecutionOptions()
	{
		_ExecOptions = new BitArray(16);
	}

	public QESQLExecutionOptions(QESQLExecutionOptions initSettings)
	{
		_ExecOptions = initSettings._ExecOptions.Clone() as BitArray;
		_ExecTimeout = initSettings._ExecTimeout;
		_batchSeparator = initSettings._batchSeparator;
	}

	public object Clone()
	{
		return new QESQLExecutionOptions(this);
	}

	public static bool IsSupported(QESQLBatch batch)
	{
		return IsSupported(batch.SqlScript);
	}

	public static bool IsSupported(string statement)
	{
		statement = statement.TrimSuffix(C_On);
		statement = statement.TrimSuffix(C_Off);

		if (!_Supported.TryGetValue(statement, out bool value))
			return false;

		return value;

	}

	public static void AppendToStringBuilder(StringBuilder sb, string cmd)
	{
		if (!IsSupported(cmd))
			return;
		sb.AppendFormat("{0} ", cmd);
	}

	public static string GetSetStatisticsXml(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetStatisticsXml, on ? C_On : C_Off);
	}

	public static string GetSetNoExecString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetNoExec, on ? C_On : C_Off);
	}

	public static string GetResetOptionsString()
	{
		return C_ResetOptions;
	}

	public static string GetSetShowplanTextString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetShowplanText, on ? C_On : C_Off);
	}

	public static string GetSetStatisticsTimeString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetStatisticsTime, on ? C_On : C_Off);
	}

	public static string GetSetStatisticsIOString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetStatisticsIO, on ? C_On : C_Off);
	}

	public static string GetSetStatisticsProfileString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetStatisticsProfile, on ? C_On : C_Off);
	}

	public static string GetSetParseOnlyString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetParseOnly, on ? C_On : C_Off);
	}

	public static string GetSetExecutionPlanAllString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetExecutionPlanAll, on ? C_On : C_Off);
	}

	public static string GetSetExecutionPlanXmlString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, C_SetExecutionPlanXml, on ? C_On : C_Off);
	}
}
