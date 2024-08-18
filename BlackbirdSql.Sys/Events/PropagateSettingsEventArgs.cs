using System;
using System.Collections.Generic;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Sys.Events;


// =============================================================================================================
//										PropagateSettingsEventArgs Class
//
/// <summary>
/// Contains the package settings/options payload used to push/propogate all extension packages'
/// settings/options updates. Allows us to add options/settings without having to define variables
/// each and every time.
/// </summary>
// =============================================================================================================
public class PropagateSettingsEventArgs : EventArgs
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PropagateSettingsEventArgs
	// ---------------------------------------------------------------------------------


	public PropagateSettingsEventArgs()
	{
	}


	public PropagateSettingsEventArgs(string package, string group)
	{
		_Package = package;
		_Group = group;
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
		_Package = package;
		_Group = group;
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
		_Package = package;
		_Group = group;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PropagateSettingsEventArgs
	// =========================================================================================================


	private readonly string _Package = null;
	private readonly string _Group = null;

	/// <summary>
	/// Package options with legacy options for brevity
	/// </summary>
	private List<MutablePair<string, object>> _Arguments = null;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PropagateSettingsEventArgs
	// =========================================================================================================


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

	public string Group => _Group;

	public string Package => _Package;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PropagateSettingsEventArgs
	// =========================================================================================================


	public void Add(MutablePair<string, object> pair)
	{
		Arguments.Add(pair);
	}



	public void Add(IBsSettingsModel model, IBsModelPropertyWrapper wrapper)
	{
		object value = wrapper.WrappedPropertyGetMethod(model);
		Arguments.Add(new MutablePair<string, object>(model.PropertyPrefix + wrapper.PropertyName, value));
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



	public void AddRange(IBsSettingsModel model)
	{
		if (model == null)
			return;

		foreach (IBsModelPropertyWrapper wrapper in model.PropertyWrappersEnumeration)
		{
			Add(model, wrapper);
		}
	}



	public MutablePair<string, object> MutatedPair(string key, object value)
	{
		return new MutablePair<string, object>(key, value);
	}



	public KeyValuePair<string, object> ValuePair(string key, object value)
	{
		return new KeyValuePair<string, object>(key, value);
	}


	#endregion Methods

}
