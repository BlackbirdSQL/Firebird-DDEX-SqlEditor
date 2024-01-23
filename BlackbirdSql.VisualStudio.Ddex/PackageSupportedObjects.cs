// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.IO;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.VisualStudio.Ddex.Controls;
using BlackbirdSql.VisualStudio.Ddex.Ctl;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex;


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

	#region Fields - PackageSupportedObjects

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
	/// For example if IVsDataViewSupport has 3 values they will be listed as IVsDataViewSupport, IVsDataViewSupport1
	/// and IVsDataViewSupport2 in the <see cref="Values"/> dictionary.
	/// If the number of registry values is zero, the implementation is handled in <see cref="TProviderObjectFactory.CreateObject"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		// { nameof(IVsDataAsyncCommand), 0 },
		// { nameof(IVsDataCommand), 0 },
		{ nameof(IVsDataConnectionEquivalencyComparer), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionPromptDialog), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionProperties), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionSupport), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIConnector), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIControl), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIProperties), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataMappedObjectConverter), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectIdentifierConverter), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectIdentifierResolver), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectMemberComparer), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectSelector), true ? 0 : 1 },
		{ nameof(IVsDataObjectSupport), _UseFactoryOnly ? 0 : 2 },
		{ nameof(IVsDataSourceInformation), _UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataSourceVersionComparer), _UseFactoryOnly ? 0 : 1 },
		// { nameof(IVsDataTransaction), 0 },
		{ nameof(IVsDataViewSupport), _UseFactoryOnly ? 0 : 5 },
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Lists the values to be registered under an interface implementation's registry
	/// key if <see cref="_UseFactoryOnly"/>
	/// is set to false.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface (aka registry key) followed by the registry
	/// value's unique index (:index), for indexes > 0.
	/// The dictionary value is the <see cref="RegistryValue"/> for that registry value
	/// entry.
	/// Note: Microsoft uses null as the name of the default value of a registry key.
	/// </remarks>
	// ---------------------------------------------------------------------------------

	public static readonly IDictionary<string, RegistryValue> Values = new Dictionary<string, RegistryValue>()
	{
		{ nameof(IVsDataConnectionEquivalencyComparer), new(null, typeof(TConnectionEquivalencyComparer).FullName) },
		{ nameof(IVsDataConnectionPromptDialog), new(null, typeof(TConnectionPromptDialog).FullName) },
		{ nameof(IVsDataConnectionProperties), new(null, typeof(TConnectionProperties).FullName) },
		{ nameof(IVsDataConnectionSupport), new (null, typeof(TConnectionSupport).FullName) },
		{ nameof(IVsDataConnectionUIConnector), new (null, typeof(TConnectionUIConnector).FullName) },
		{ nameof(IVsDataConnectionUIControl), new(null, typeof(TConnectionUIControl).FullName) },
		{ nameof(IVsDataConnectionUIProperties), new(null, typeof(TConnectionUIProperties).FullName) },
		{ nameof(IVsDataMappedObjectConverter), new(null, typeof(TMappedObjectConverter).FullName) },
		{ nameof(IVsDataObjectIdentifierConverter), new(null, typeof(TObjectIdentifierConverter).FullName) },
		{ nameof(IVsDataObjectIdentifierResolver), new(null, typeof(TObjectIdentifierResolver).FullName) },
		{ nameof(IVsDataObjectMemberComparer), new(null, typeof(TObjectMemberComparer).FullName) },
		{ nameof(IVsDataObjectSelector), new(null, typeof(TObjectSelector).FullName) },

		{ nameof(IVsDataObjectSupport), new(null, typeof(TObjectSupport).FullName) },
		{ nameof(IVsDataObjectSupport)+1, new("XmlResource", typeof(TObjectSupport).FullName + ".xml") },

		{ nameof(IVsDataSourceInformation), new(null, typeof(TSourceInformation).FullName) },
		{ nameof(IVsDataSourceVersionComparer), new(null, typeof(TSourceVersionComparer).FullName) },


		{ nameof(IVsDataViewSupport), new(null, typeof(TViewSupport).FullName) },
		{ nameof(IVsDataViewSupport)+1, new("XmlResource", typeof(TViewSupport).FullName + ".xml") },
		{ nameof(IVsDataViewSupport)+2, new("AllowAsynchronousEnumerations", "true") },
		{ nameof(IVsDataViewSupport)+3, new("HasDocumentProvider", 0) },
		{ nameof(IVsDataViewSupport)+4, new("PersistentCommands", $"{VS.SeDataCommandSetGuid},{CommandProperties.C_CmdIdSERetrieveData},3") }
	};


	#endregion Fields





	// =========================================================================================================
	#region Child classes - PackageSupportedObjects
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// A container class for a registry value consisting of the value's name and value.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public readonly struct RegistryValue(string name, Object value)
	{
		public readonly string Name = name;
		public readonly object Value = value;
	}


	#endregion Child classes

}
