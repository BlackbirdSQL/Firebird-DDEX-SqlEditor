using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;

namespace BlackbirdSql.Core.Ctl.Events;


/// <summary>
/// Contains the package settings/options payload used by GlobalsAgent
/// to push/propogate all extension packages' settings/options updates.
/// Allows us to add options/settings without having to define variables
/// each and every time.
/// </summary>
public class PropagateSettingsEventArgs : EventArgs
{
	public readonly string Package = null;
	public readonly string Group = null;

	/// <summary>
	/// Package options with legacy options for brevity
	/// </summary>
	private List<MutablePair<string, object>> _Arguments = null;


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


	public List<MutablePair<string, object>> Arguments => _Arguments ??= [];



	public PropagateSettingsEventArgs()
	{
	}

	public PropagateSettingsEventArgs(string package, string group)
	{
		Package = package;
		Group = group;
	}



	public PropagateSettingsEventArgs(KeyValuePair<string, object>[] arguments)
	{

		if (arguments != null)
		{
			_Arguments = new(arguments.Length);

			AddRange(arguments);
		}
	}

	public PropagateSettingsEventArgs(string package, string group, KeyValuePair<string, object>[] arguments) : this(arguments)
	{
		Package = package;
		Group = group;
	}

	public PropagateSettingsEventArgs(MutablePair<string, object>[] arguments)
	{
		if (arguments != null)
		{
			_Arguments = new(arguments.Length);

			AddRange(arguments);
		}
	}

	public PropagateSettingsEventArgs(string package, string group, MutablePair<string, object>[] arguments) : this(arguments)
	{
		Package = package;
		Group = group;
	}


	public KeyValuePair<string, object> ValuePair(string key, object value)
	{
		return new KeyValuePair<string, object>(key, value);
	}



	public MutablePair<string, object> MutatedPair(string key, object value)
	{
		return new MutablePair<string, object>(key, value);
	}


	public void Add(MutablePair<string, object> pair)
	{
		Arguments.Add(pair);
	}

	public void Add(IBSettingsModel model, IBSettingsModelPropertyWrapper wrapper)
	{
		object value = wrapper.WrappedPropertyGetMethod(model);
		Arguments.Add(new MutablePair<string, object>(model.LivePrefix + wrapper.PropertyName, value));
	}

	public void AddRange(KeyValuePair<string, object>[] arguments)
	{

		if (arguments != null)
		{
			foreach (KeyValuePair<string, object> pair in arguments)
			{
				// if (pair.Value != null)
				Arguments.Add(new MutablePair<string, object>(pair));
			}
		}
	}


	public void AddRange(MutablePair<string, object>[] arguments)
	{
		if (arguments != null)
		{
			foreach (MutablePair<string, object> pair in arguments)
			{
				// if (pair.Value != null)
				Arguments.Add(pair);
			}
		}
	}

	public void AddRange(IBSettingsModel model)
	{
		foreach (IBSettingsModelPropertyWrapper wrapper in model.PropertyWrappers)
		{
			Add(model, wrapper);
		}
	}


}
