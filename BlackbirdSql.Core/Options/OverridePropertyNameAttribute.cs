//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;

namespace BlackbirdSql.Core.Options
{
	//
	// Summary:
	//     Apply this attribute on an individual public get/set property in your Community.VisualStudio.Toolkit.BaseOptionModel`1
	//     derived class to use a specific propertyName to store a given property in the
	//     Microsoft.VisualStudio.Settings.SettingsStore rather than using the name of the
	//     property.
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class OverridePropertyNameAttribute : Attribute
	{
		//
		// Summary:
		//     This value is used as the propertyName parameter when reading and writing to
		//     the Microsoft.VisualStudio.Settings.SettingsStore.
		public string PropertyName { get; }

		//
		// Summary:
		//     Specifies the propertyName in the Microsoft.VisualStudio.Settings.SettingsStore
		//     where this setting is stored rather than using the default, which is the name
		//     of the property.
		//
		// Parameters:
		//   propertyName:
		//     This value is used as the propertyName parameter when reading and writing to
		//     the Microsoft.VisualStudio.Settings.SettingsStore.
		public OverridePropertyNameAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}
