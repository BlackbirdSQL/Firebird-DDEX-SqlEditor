//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Collections.Generic;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration;


// =========================================================================================================
//										PackageSupportedObjects Class
//
/// <summary>
/// This class is used by <see cref="VsPackageRegistration.Register"/> and lists all the interfaces supported
/// by this provider (<see cref="Implementations"/>) and their implementation classes (<see cref="Values"/>)
/// </summary>
// =========================================================================================================
internal static class PackageSupportedObjects
{

	#region Variables - PackageSupportedObjects

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This preset defines whether implementations are defined in the private registry or handled in <see cref="ProviderObjectFactory.CreateObject"/>
	/// </summary>
	/// <remarks>
	/// Setting to false inserts registry values against implementations allowing VS to manage implementations.
	/// Setting to true only creates the key for each implementation for the ProviderObjectFactory service
	/// to manage.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	const bool _UseFactoryOnly = false;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Dictionary of all supported provider interfaces with the <see cref="_useProviderObjectFactoryOnly"/>
	/// defining where their implementations are defined or handled.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface name (aka registry key) and value is the number (qty) of registry
	/// values under the key.
	/// For example if IVsDataViewSupport has 3 values they will be listed as IVsDataViewSupport:0, IVsDataViewSupport:1
	/// and IVsDataViewSupport:2 in the <see cref="Values"/> dictionary.
	/// If the number of registry values is zero, the implementation is handled in <see cref="TProviderObjectFactory.CreateObject"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		{ "IVsDataAsyncCommand", 0 },
		{ "IVsDataCommand", 0 },
		{ "IVsDataConnectionEquivalencyComparer", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionPromptDialog", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionProperties", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionSupport", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIConnector", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIControl", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIProperties", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataMappedObjectConverter", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectIdentifierConverter", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectIdentifierResolver", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectMemberComparer", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectSelector", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectSupport", _UseFactoryOnly ? 0 : 2 },
		{ "IVsDataSourceInformation", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataSourceVersionComparer", _UseFactoryOnly ? 0 : 1 },
		{ "IVsDataTransaction", 0 },
		{ "IVsDataViewSupport", _UseFactoryOnly ? 0 : 4 },
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Lists the values to be registered under an interface implementation's registry key if <see cref="_UseFactoryOnly"/>
	/// is set to false.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface (aka registry key) followed by the registry value's unique index (:index).
	/// The dictionary value is the <see cref="RegistryValue"/> for that registry value entry.
	/// Note: Microsoft uses null as the name of the default value of a registry key.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly IDictionary<string, RegistryValue> Values = new Dictionary<string, RegistryValue>()
	{
		{ "IVsDataConnectionEquivalencyComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TConnectionEquivalencyComparer") },
		{ "IVsDataConnectionPromptDialog:0", new(null, "BlackbirdSql.VisualStudio.Ddex.TConnectionPromptDialog") },
		{ "IVsDataConnectionProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.TConnectionProperties") },
		{ "IVsDataConnectionSupport:0", new (null, "BlackbirdSql.VisualStudio.Ddex.TConnectionSupport") },
		{ "IVsDataConnectionUIConnector:0", new (null, "BlackbirdSql.VisualStudio.Ddex.TConnectionUIConnector") },
		{ "IVsDataConnectionUIControl:0", new(null, "BlackbirdSql.VisualStudio.Ddex.TConnectionUIControl") },
		{ "IVsDataConnectionUIProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.TConnectionUIProperties") },
		{ "IVsDataMappedObjectConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TMappedObjectConverter") },
		{ "IVsDataObjectIdentifierConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TObjectIdentifierConverter") },
		{ "IVsDataObjectIdentifierResolver:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TObjectIdentifierResolver") },
		{ "IVsDataObjectMemberComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TObjectMemberComparer") },
		{ "IVsDataObjectSelector:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TObjectSelector") },

		{ "IVsDataObjectSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TObjectSupport") },
		{ "IVsDataObjectSupport:1", new("XmlResource",  "BlackbirdSql.VisualStudio.Ddex.TObjectSupport.xml") },

		{ "IVsDataSourceInformation:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TSourceInformation") },
		{ "IVsDataSourceVersionComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TSourceVersionComparer") },


		{ "IVsDataViewSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.TViewSupport") },
		{ "IVsDataViewSupport:1", new("AllowAsynchronousEnumerations",  "true") },
		{ "IVsDataViewSupport:2", new("HasDocumentProvider",  0) },
		// { "IVsDataViewSupport:3", new("PersistentCommands",  "501822E1-B5AF-11d0-B4DC-00A0C91506EF,0x3528,3") },
		{ "IVsDataViewSupport:3", new("XmlResource",  "BlackbirdSql.VisualStudio.Ddex.TViewSupport.xml") }
	};


	#endregion Variables





	// =========================================================================================================
	#region Child classes - PackageSupportedObjects
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// A container class for a registry value consisting of the value's name and value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class RegistryValue
	{
		public readonly string Name;
		public readonly Object Value;

		public RegistryValue(string name, Object value)
		{
			Name = name;
			Value = value;
		}
	}


	#endregion Child classes

}
