#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Globalization;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.QueryExecution;


public class QESQLExecutionOptions : ICloneable
{
	private readonly BitArray _ExecOptions;

	private string _batchSeparator = "GO";

	private int _execTimeout = AbstractQESQLExec.DefaultSqlExecTimeout;

	private static readonly string _on = "ON";

	private static readonly string _off = "OFF";

	private static readonly string _setNoExec = "SET NOEXEC {0}";

	private static readonly string _setShowplanText = "SET SHOWPLAN_TEXT {0}";

	private static readonly string _setStatisticsTime = "SET STATISTICS TIME {0}";

	private static readonly string _setStatisticsIO = "SET STATISTICS IO {0}";

	private static readonly string _setStatisticsProfile = "SET STATISTICS PROFILE {0}";

	private static readonly string _setParseOnly = "SET PARSEONLY {0}";

	private static readonly string _setExecutionPlanAll = "SET SHOWPLAN_ALL {0}";

	private static readonly string _setExecutionPlanXml = "SET SHOWPLAN_XML {0}";

	private static readonly string _setStatisticsXml = "SET STATISTICS XML {0}";

	private static readonly string _resetOptions = "SET NOEXEC, PARSEONLY, FMTONLY OFF";

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
			return _execTimeout;
		}
		set
		{
			_execTimeout = value;
		}
	}

	public QESQLExecutionOptions()
	{
		_ExecOptions = new BitArray(16);
	}

	public QESQLExecutionOptions(QESQLExecutionOptions initSettings)
	{
		_ExecOptions = initSettings._ExecOptions.Clone() as BitArray;
		_execTimeout = initSettings._execTimeout;
		_batchSeparator = initSettings._batchSeparator;
	}

	public object Clone()
	{
		return new QESQLExecutionOptions(this);
	}

	public static string GetSetStatisticsXml(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setStatisticsXml, bOn ? _on : _off);
	}

	public static string GetSetNoExecString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setNoExec, bOn ? _on : _off);
	}

	public static string GetResetOptionsString()
	{
		return _resetOptions;
	}

	public static string GetSetShowplanTextString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setShowplanText, bOn ? _on : _off);
	}

	public static string GetSetStatisticsTimeString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setStatisticsTime, bOn ? _on : _off);
	}

	public static string GetSetStatisticsIOString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setStatisticsIO, bOn ? _on : _off);
	}

	public static string GetSetStatisticsProfileString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setStatisticsProfile, bOn ? _on : _off);
	}

	public static string GetSetParseOnlyString(bool bOn)
	{
		return string.Format(CultureInfo.InvariantCulture, _setParseOnly, bOn ? _on : _off);
	}

	public static string GetSetExecutionPlanAllString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, _setExecutionPlanAll, on ? _on : _off);
	}

	public static string GetSetExecutionPlanXmlString(bool on)
	{
		return string.Format(CultureInfo.InvariantCulture, _setExecutionPlanXml, on ? _on : _off);
	}
}
