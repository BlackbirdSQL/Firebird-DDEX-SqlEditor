using System;
using System.Collections.Generic;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration;



// ---------------------------------------------------------------------------------------------------
//
//								SupportedObjects Class
//
// ---------------------------------------------------------------------------------------------------



/// <summary>
/// This class is used by <see cref="VsPackageRegistration.Register"/> and lists all the interfaces supported by this provider (<see cref="Implementations"/>) and their implementation classes (<see cref="Values"/>)
/// </summary>
internal static class SupportedObjects
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
	const bool _useProviderObjectFactory = true;



	/// <summary>
	/// Dictionary of all supported provider interfaces with the <see cref="_useProviderObjectFactory"/> defining where their implementations are defined or handled.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface name (aka registry key) and value is the number (qty) of registry values under the key.
	/// For example if IVsDataViewSupport has 3 values they will be listed as IVsDataViewSupport:0, IVsDataViewSupport:1 and IVsDataViewSupport:2 in the <see cref="Values"/> dictionary.
	/// If the number of registry values is zero, the implementation is handled in <see cref="ProviderObjectFactory.CreateObject"/>.
	/// </remarks>
	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		{ "IVsDataConnectionEquivalencyComparer", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataConnectionProperties", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataConnectionSupport", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataConnectionUIControl", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataConnectionUIProperties", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataMappedObjectConverter", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataObjectIdentifierConverter", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataObjectIdentifierResolver", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataObjectMemberComparer", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataObjectSelector", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataObjectSupport", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataSourceInformation", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataSourceVersionComparer", _useProviderObjectFactory ? 0 : 1 },
		{ "IVsDataViewSupport", _useProviderObjectFactory ? 0 : 5 },
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
		{ "IVsDataConnectionEquivalencyComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.DataConnectionEquivalencyComparer") },
		{ "IVsDataConnectionProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.ConnectionProperties") },
		{ "IVsDataConnectionSupport:0", new (null, "BlackbirdSql.VisualStudio.Ddex.ConnectionSupport") },
		{ "IVsDataConnectionUIControl:0", new(null, "BlackbirdSql.VisualStudio.Ddex.ConnectionUIControl") },
		{ "IVsDataConnectionUIProperties:0", new(null, "BlackbirdSql.VisualStudio.Ddex.ConnectionProperties") },
		{ "IVsDataMappedObjectConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.MappedObjectConverter") },
		{ "IVsDataObjectIdentifierConverter:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ObjectIdentifierConverter") },
		{ "IVsDataObjectIdentifierResolver:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ObjectIdentifierResolver") },
		{ "IVsDataObjectMemberComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ObjectMemberComparer") },
		{ "IVsDataObjectSelector:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ObjectSelector") },
		{ "IVsDataObjectSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ObjectSupport") },
		{ "IVsDataSourceInformation:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.SourceInformation") },
		{ "IVsDataSourceVersionComparer:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.SourceVersionComparer") },

		{ "IVsDataViewSupport:0", new(null,  "BlackbirdSql.VisualStudio.Ddex.ViewSupport") },
		{ "IVsDataViewSupport:1", new("AllowAsynchronousEnumerations",  "true") },
		{ "IVsDataViewSupport:2", new("HasDocumentProvider",  1) },
		{ "IVsDataViewSupport:3", new("PersistentCommands",  "501822E1-B5AF-11d0-B4DC-00A0C91506EF,0x3528,3") },
		{ "IVsDataViewSupport:4", new("XmlResource",  "BlackbirdSql.VisualStudio.Ddex.ViewSupport.xml") }
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
