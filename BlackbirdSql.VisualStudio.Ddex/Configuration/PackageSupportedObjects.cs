using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlackbirdSql.VisualStudio.Ddex.Configuration;



// ---------------------------------------------------------------------------------------------------
//
//								SupportedObjects Class
//
// ---------------------------------------------------------------------------------------------------



/// <summary>
/// This class is used by <see cref="VsPackageRegistration.Register"/> and lists all the interfaces supported by this provider (<see cref="Implementations"/>) and their implementation classes (<see cref="Values"/>)
/// </summary>
internal static class PackageSupportedObjects
{

	 /* _useProviderObjectFactory
	 * Setting this to false inserts registry values against implementations
	 * Setting this to true only creates the key for each implementation
	 * It seems there's a way for VS to manage implementations instead of the ProviderObjectFactory service.
	 * Set to false if you want to test this. (It currently does not work. We may be missing a trick)
	*/
	/// <summary>
	/// This preset defines whether implementations are defined in the private registry or handled in <see cref="ProviderObjectFactory.CreateObject"/>
	/// </summary>
	const bool _useFactoryOnly = false;



	/// <summary>
	/// Dictionary of all supported provider interfaces with the <see cref="_useProviderObjectFactoryOnly"/> defining where their implementations are defined or handled.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface name (aka registry key) and value is the number (qty) of registry values under the key.
	/// For example if IVsDataViewSupport has 3 values they will be listed as IVsDataViewSupport:0, IVsDataViewSupport:1 and IVsDataViewSupport:2 in the <see cref="Values"/> dictionary.
	/// If the number of registry values is zero, the implementation is handled in <see cref="DdexProviderObjectFactory.CreateObject"/>.
	/// </remarks>
	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		{ "IVsDataAsyncCommand", 0 },
		{ "IVsDataCommand", 0 },
		{ "IVsDataConnectionEquivalencyComparer", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionPromptDialog", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionProperties", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionSupport", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIConnector", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIControl", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataConnectionUIProperties", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataMappedObjectConverter", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectIdentifierConverter", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectIdentifierResolver", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectMemberComparer", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectSelector", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataObjectSupport", _useFactoryOnly ? 0 : 2 },
		{ "IVsDataSourceInformation", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataSourceVersionComparer", _useFactoryOnly ? 0 : 1 },
		{ "IVsDataTransaction", 0 },
		{ "IVsDataViewSupport", _useFactoryOnly ? 0 : 4 },
	};



	/// <summary>
	/// Lists the values to be registered under an interface implementation's registry key if <see cref="_useProviderObjectFactory"/> is set to false.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface (aka registry key) followed by the registry value's unique index (:index).
	/// The dictionary value is the <see cref="RegistryValue"/> for that registry value entry.
	/// Note: Microsoft uses null as the name of the default value of a registry key.
	/// </remarks>
	public static readonly IDictionary<string, RegistryValue> Values = new Dictionary<string, RegistryValue>()
	{
		{ "IVsDataConnectionEquivalencyComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexConnectionEquivalencyComparer") },
		{ "IVsDataConnectionPromptDialog:0", new(null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionPromptDialog") },
		{ "IVsDataConnectionProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionProperties") },
		{ "IVsDataConnectionSupport:0", new (null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionSupport") },
		{ "IVsDataConnectionUIConnector:0", new (null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionUIConnector") },
		{ "IVsDataConnectionUIControl:0", new(null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionUIControl") },
		{ "IVsDataConnectionUIProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.DdexConnectionUIProperties") },
		{ "IVsDataMappedObjectConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexMappedObjectConverter") },
		{ "IVsDataObjectIdentifierConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexObjectIdentifierConverter") },
		{ "IVsDataObjectIdentifierResolver:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexObjectIdentifierResolver") },
		{ "IVsDataObjectMemberComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexObjectMemberComparer") },
		{ "IVsDataObjectSelector:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexObjectSelector") },

		{ "IVsDataObjectSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexObjectSupport") },
		{ "IVsDataObjectSupport:1", new("XmlResource",  "BlackbirdSql.VisualStudio.Ddex.DdexObjectSupport.xml") },

		{ "IVsDataSourceInformation:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexSourceInformation") },
		{ "IVsDataSourceVersionComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexSourceVersionComparer") },

		{ "IVsDataViewSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DdexViewSupport") },
		{ "IVsDataViewSupport:1", new("AllowAsynchronousEnumerations",  "true") },
		{ "IVsDataViewSupport:2", new("HasDocumentProvider",  0) },
		// { "IVsDataViewSupport:3", new("PersistentCommands",  "501822E1-B5AF-11d0-B4DC-00A0C91506EF,0x3528,3") },
		{ "IVsDataViewSupport:3", new("XmlResource",  "BlackbirdSql.VisualStudio.Ddex.DdexViewSupport.xml") }
	};


	/// <summary>
	/// A container class for a registry value consisting of the value's name and value.
	/// </summary>
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

}
