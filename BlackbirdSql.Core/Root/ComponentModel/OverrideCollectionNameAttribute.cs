//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;

namespace BlackbirdSql.Core.ComponentModel
{
	//
	// Summary:
	//     Apply this attribute on an individual get/set property in your Community.VisualStudio.Toolkit.BaseOptionModel`1
	//     derived class to use a specific CollectionName to store a given property in the
	//     Microsoft.VisualStudio.Settings.SettingsStore rather than using the Community.VisualStudio.Toolkit.BaseOptionModel`1.CollectionName.
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class OverrideCollectionNameAttribute : Attribute
	{
		//
		// Summary:
		//     This value is used as the collectionPath parameter when reading and writing using
		//     the Microsoft.VisualStudio.Settings.SettingsStore.
		public string CollectionName { get; }

		//
		// Summary:
		//     Specifies the CollectionName in the Microsoft.VisualStudio.Settings.SettingsStore
		//     where this setting is stored rather than using the default.
		//
		// Parameters:
		//   collectionName:
		//     This value is used as the collectionPath parameter when reading and writing using
		//     the Microsoft.VisualStudio.Settings.SettingsStore.
		public OverrideCollectionNameAttribute(string collectionName)
		{
			CollectionName = collectionName;
		}
	}
}
