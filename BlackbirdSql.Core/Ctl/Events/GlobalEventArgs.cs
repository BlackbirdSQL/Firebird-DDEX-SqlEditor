using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl.Events;


//
/// <summary>
/// Contains the package settings/options payload used by GlobalsAgent
/// to update package wide settings/options. Allows us to add options
/// without having to define variables each and every time.
/// </summary>
public class GlobalEventArgs : EventArgs
{
	public readonly string Group;

	/// <summary>
	/// Package options with legacy options for brevity
	/// </summary>
	private List<MutablePair<string, object>> _Arguments = null;
	/*
	public readonly MutablePair<string, object>[] Arguments = new MutablePair<string, object>[]
		{
			/*
			// General options
			ValuePair( "EnableDiagnostics", false ),
			ValuePair( "EnableTaskLog", false ),
			ValuePair( "ValidateConfig", false ),
			ValuePair( "ValidateEdmx", "" )

			// Debug options
 				ValuePair( "PersistentValidation", false ),
			ValuePair( "EnableTrace", false ),
			ValuePair( "EnableDiagnosticsLog", false ),
			ValuePair( "LogFile", "" ),
			ValuePair( "EnableFbDiagnostics", false ),
			ValuePair( "FbLogFile", "" )
		};
	*/




	public object this[string name]
	{
		get
		{
			MutablePair<string, object> pair = MutablePair<string, object>.Find(Arguments, name);

			if (pair.Key == null)
				return null;

			return pair.Value;
		}

		set
		{
			MutablePair<string, object> pair = MutablePair<string, object>.Find(Arguments, name);

			if (pair.Key == null)
			{
				pair = new MutablePair<string, object>(name, value);
				Arguments.Add(pair);
			}
			else
			{
				pair.Value = value;
			}
		}
	}


	private List<MutablePair<string, object>> Arguments => _Arguments ??= new();



	public GlobalEventArgs(KeyValuePair<string, object>[] arguments)
	{

		if (arguments != null)
		{
			_Arguments = new(arguments.Length);

			foreach (KeyValuePair<string, object> pair in arguments)
			{
				if (pair.Value != null)
					Arguments.Add(new MutablePair<string, object>(pair));
			}
		}
	}

	public GlobalEventArgs(string group, KeyValuePair<string, object>[] arguments) : this(arguments)
	{
		Group = group;
	}

	public GlobalEventArgs(MutablePair<string, object>[] arguments)
	{
		if (arguments != null)
		{
			_Arguments = new(arguments.Length);

			foreach (MutablePair<string, object> pair in arguments)
			{
				if (pair.Value != null)
					Arguments.Add(pair);
			}
		}
	}

	public GlobalEventArgs(string group, MutablePair<string, object>[] arguments) : this(arguments)
	{
		Group = group;
	}


	public static KeyValuePair<string, object> ValuePair(string key, object value)
	{
		return new KeyValuePair<string, object>(key, value);
	}



	public static MutablePair<string, object> MutatedPair(string key, object value)
	{
		return new MutablePair<string, object>(key, value);
	}
}
