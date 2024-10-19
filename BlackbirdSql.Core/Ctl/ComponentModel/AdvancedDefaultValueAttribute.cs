using System;
using System.ComponentModel;
using System.Globalization;

namespace BlackbirdSql.Core.Ctl.ComponentModel;

/// <summary>
/// Gets the default value for a given special type.
/// Currently supports types of:
/// System.Environment (returns the environment variable of value as string.).
/// System.Environment.SpecialFolder (returns the environment variable of the enum value as string).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AdvancedDefaultValueAttribute : DefaultValueAttribute
{
	public AdvancedDefaultValueAttribute(Type type, object value)
	: base(GetAdvancedValue(type, value))
	{
	}


	/*
	// Unused because creating the default as the type to be used in GetAdvancedValue()
	// and then passing it to DefaultValueAttribute cast as object seems to work fine.
	// 
	/// <summary>
	/// Resolves the type passed in 'type' to the type to be used by DefaultValueAttribute.
	/// </summary>
	private static Type ResolveAdvancedType(Type type)
	{
		switch (type.FullName.ToLower())
		{
			case "system.environment":
			case "system.environment+specialfolder":
				return typeof(string);
			default:
				return type;
		}
	}
	*/

	/// <summary>
	/// Gets the default created in it's correct type and then returns it as object
	/// in DefaultValueAttribute constructor.
	/// </summary>
	private static object GetAdvancedValue(Type type, object value)
	{
		object result;

		switch (type.FullName.ToLower())
		{
			case "system.environment+specialfolder":
				result = Environment.GetFolderPath((Environment.SpecialFolder)value);
				// Evs.Debug(typeof(AdvancedDefaultValueAttribute), "GetAdvancedValue()", $"Default for Environment.SpecialFolder: {result}.");
				break;
			default:
				result = value;
				// Evs.Debug(typeof(AdvancedDefaultValueAttribute), "GetAdvancedValue()", $"Default for Type {type.FullName.ToLower()}: {result}.");
				break;
		}

		return result;
	}

}