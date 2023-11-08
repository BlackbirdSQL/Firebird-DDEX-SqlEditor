// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLExecutionOptions
using System;
using System.Collections;
using System.Collections.Generic;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Model.QueryExecution;


public class QESQLCommandBuilder : LiveUserSettings, ICloneable
{
	private readonly BitArray _ExecOptions;


	public bool WithExecutionPlan
	{
		get { return _ExecOptions[0]; }
		set { _ExecOptions[0] = value; }
	}

	public bool WithClientStats
	{
		get { return _ExecOptions[1]; }
		set { _ExecOptions[1] = value; }
	}
	
	public bool WithProfiling
	{
		get { return _ExecOptions[2]; }
		set { _ExecOptions[2] = value; }
	}

	public bool ParseOnly
	{
		get { return _ExecOptions[3]; }
		set { _ExecOptions[3] = value; }
	}

	public bool WithNoExec
	{
		get { return _ExecOptions[4]; }
		set { _ExecOptions[4] = value; }
	}

	public bool WithExecutionPlanText
	{
		get { return _ExecOptions[5]; }
		set { _ExecOptions[5] = value; }
	}

	public bool WithStatisticsTime
	{
		get { return _ExecOptions[6]; }
		set { _ExecOptions[6] = value; }
	}

	public bool WithStatisticsIO
	{
		get { return _ExecOptions[7]; }
		set { _ExecOptions[7] = value; }
	}

	public bool WithStatisticsProfile
	{
		get { return _ExecOptions[8]; }
		set { _ExecOptions[8] = value; }
	}

	public bool WithEstimatedExecutionPlan
	{
		get { return _ExecOptions[9]; }
		set { _ExecOptions[9] = value; }
	}

	public bool WithOleSqlScripting
	{
		get { return _ExecOptions[13]; }
		set { _ExecOptions[13] = value; }
	}

	public bool SuppressProviderMessageHeaders
	{
		get { return _ExecOptions[14]; }
		set { _ExecOptions[14] = value; }
	}

	public bool WithDebugging
	{
		get { return _ExecOptions[15]; }
		set { _ExecOptions[15] = value; }
	}



	protected QESQLCommandBuilder() : base()
	{
		_ExecOptions = new BitArray(16);
	}

	protected QESQLCommandBuilder(BitArray execOptions) : base()
	{
		_ExecOptions = execOptions.Clone() as BitArray;
	}


	public static new QESQLCommandBuilder CreateInstance()
	{
		QESQLCommandBuilder builder = new()
		{
			_LiveStore = new(_SettingsStore)
		};

		return builder;
	}

	public static QESQLCommandBuilder CreateInstance(BitArray execOptions)
	{
		QESQLCommandBuilder builder = new(execOptions)
		{
			_LiveStore = new(_SettingsStore)
		};

		return builder;
	}

	public static new QESQLCommandBuilder CreateInstance(IDictionary<string, object> liveStore)
	{
		QESQLCommandBuilder builder = new()
		{
			_LiveStore = new(liveStore)
		};

		return builder;
	}

	public static QESQLCommandBuilder CreateInstance(IDictionary<string, object> liveStore, BitArray execOptions)
	{
		QESQLCommandBuilder builder = new(execOptions)
		{
			_LiveStore = new(liveStore)
		};

		return builder;
	}


	public object Clone()
	{
		lock (_LockClass)
			return CreateInstance(_LiveStore, _ExecOptions);
	}


	public static bool IsSupported(object property)
	{
		Type type = property.GetType();

		string cmd = SqlResources.ResourceManager.GetString(type.Name);

		return (cmd == null);
	}



	public static string GetResetCommandText()
	{
		return string.Empty;
	}

}
