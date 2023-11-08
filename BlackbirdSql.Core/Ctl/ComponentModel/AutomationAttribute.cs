// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.DependsOnPropertyAttribute

using System;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

/// <summary>
/// Identifies a property as an enabled/disabled automation property.
/// A dependent property's driver/automator is specified in the constructor.
/// Use the default constructor for the automator property and specify
/// RefreshProperties(RefreshProperties.All).
/// Automators must have a TypeConverter that descends from AbstractBoolConverter
/// or AbstractEnumConverter.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class AutomationAttribute: Attribute
{
	public string Automator { get; }
	public bool Invert { get; }
	public int EnableValue { get; } = int.MinValue;

	/// <summary>
	/// Default .ctor for specifying an automator/driver property. The property
	/// must have a TypeConverter descendent from AbstractBoolConverter.
	/// </summary>
	public AutomationAttribute()
	{
		Automator = null;
	}

	/// <summary>
	/// .ctor for specifying a dependent property.
	/// </summary>
	/// <param name="automator">
	/// The property name of the automator that determines the readonly state
	/// of a dependent property.
	/// </param>
	/// <param name="invert">
	/// Set to true if the boolean value of an AbstractBoolConverter
	/// boolean automator inverts the readonly state of the property.
	/// Defaults to false.
	/// </param>
	public AutomationAttribute(string automator, bool invert = false)
	{
		Automator = automator;
		Invert = invert;
	}


	/// <summary>
	/// .ctor for specifying a dependent property.
	/// </summary>
	/// <param name="automator">
	/// The property name of the automator that determines the readonly state
	/// of a dependent property.
	/// </param>
	/// <param name="enableValue">
	/// The enum value of an AbstractEnumConverter automator that determines the
	/// readonly state of the property. If enableValue is equal to the enum of
	/// the automator, the property is enabled. If the enableValue is negative
	/// the automator inverts the state dependent on the absolute of enableValue.
	/// An enum of zero is always considered positive.
	/// </param>
	public AutomationAttribute(string automator, int enableValue)
	{
		Automator = automator;
		EnableValue = enableValue;
	}
}
