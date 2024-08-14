// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.VisualStudio.Ddex.Controls;
using BlackbirdSql.VisualStudio.Ddex.Ctl;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex;

// =========================================================================================================
//										ExtensionData Class
//
/// <summary>
/// Container class for all extension specific constants and is also is used by
/// <see cref="VsPackageRegistrationAttribute.Register"/> and lists all the interfaces supported
/// by this provider (<see cref="Implementations"/>) and their implementation classes (<see cref="Values"/>)
/// </summary>
// =========================================================================================================
static class ExtensionData
{


	// ---------------------------------------------------------------------------------
	#region Constants - ExtensionData
	// ---------------------------------------------------------------------------------

	public const string C_VsixCompany = "BlackbirdSql";
	public const string C_VsixProduct = "BlackbirdSql.VisualStudio.Ddex";
	public const string C_VsixName = "BlackbirdSql DDEX and SqlEditor for Firebird";
	public const string C_VsixDescription = "The Ultimate Firebird DDEX 2.0 Provider and SqlEditor with the \"look and feel\" of Microsoft's SqlServer extensions";
	public const string C_VsixVersion = "14.1.0.1";


	public const string C_PackageControllerServiceName = "SBsPackageController";
	public const string C_ProviderObjectFactoryServiceName = "SBsProviderObjectFactory";


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This preset defines whether DDEX implementations are defined in the private
	/// registry or handled in <see cref="TProviderObjectFactory.CreateObject"/>
	/// </summary>
	/// <remarks>
	/// Setting to false inserts registry values against implementations allowing VS
	/// to manage implementations.
	/// Setting to true only creates the key for each implementation for the
	/// ProviderObjectFactory service
	/// to manage.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	const bool C_UseFactoryOnly = false;


	#endregion Constants





	// =====================================================================================================
	#region Constants - GUIDS - ExtensionData
	// =====================================================================================================


	public const string AssemblyGuid = "D5A9B07D-5302-42A5-9509-F877DEC4BEDB";


	// Settings Guids
	public const string GeneralSettingsGuid = "C76D0CE2-CB4D-4594-8A94-6E6833B4A160";
	public const string DebugSettingsGuid = "7E2D7C87-1FAD-42D1-AE67-4EEA3281E52C";
	public const string EquivalencySettingsGuid = "747AD10A-2475-4888-95C5-FECFD13979F0";


	#endregion Constants - GUIDs





	// =====================================================================================================
	#region Constants - Security tokens - ExtensionData
	// =====================================================================================================


	public const string PublicTokenString = "d39a163eb11ac91a";
	public const string PublicTokenStringNET = "";

	public const string PublicHashString = "002400000480000094000000060200000024000052534131000400000100010099b99763c990a25eb0fad128c99cefa4dd9716e5edd609fcc245d0e19fdbcc5b4ac8b1f33349a0a231cc5d0e7702e8289e29d6f6e28074e3e844b24726c7368151dcfa97d109de847521febfead7937cae2933418583cc97630263d849425645721ef381de3c33ef27d3d01c805a8082721f94d5e664c09390f3a3fbf9faa9ca";
	public const string PublicHashStringNET = "";


	#endregion Constants - Security tokens




	// =====================================================================================================
	#region Fields - ExtensionData
	// =====================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Dictionary of all supported provider interfaces with the
	/// <see cref="C_UseFactoryOnly"/> defining where their implementations are defined
	/// or handled.
	/// </summary>
	/// <remarks>
	/// The dictionary key is the interface name (aka registry key) and value is the
	/// number (qty) of registry values under the key.
	/// For example if IVsDataViewSupport has 3 values they will be listed as
	/// IVsDataViewSupport, IVsDataViewSupport1 and IVsDataViewSupport2 in the
	/// <see cref="Values"/> dictionary.
	/// If the number of registry values is zero, the implementation is handled in
	/// <see cref="TProviderObjectFactory.CreateObject"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		// { nameof(IVsDataAsyncCommand), 0 },
		// { nameof(IVsDataCommand), 0 },
		{ nameof(IVsDataConnectionEquivalencyComparer), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionPromptDialog), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionProperties), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionSupport), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIConnector), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIControl), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataConnectionUIProperties), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataMappedObjectConverter), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectIdentifierConverter), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectIdentifierResolver), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectMemberComparer), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataObjectSelector), true ? 0 : 1 },
		{ nameof(IVsDataObjectSupport), C_UseFactoryOnly ? 0 : 2 },
		{ nameof(IVsDataSourceInformation), C_UseFactoryOnly ? 0 : 1 },
		{ nameof(IVsDataSourceVersionComparer), C_UseFactoryOnly ? 0 : 1 },
		// { nameof(IVsDataTransaction), 0 },
		{ nameof(IVsDataViewSupport), C_UseFactoryOnly ? 0 : 5 },
	};


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Lists the values to be registered under an interface implementation's registry
	/// key if <see cref="C_UseFactoryOnly"/>
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

	public static readonly IDictionary<string, RegistryValue> ImplementationValues = new Dictionary<string, RegistryValue>()
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





	// =====================================================================================================
	#region Sub-classes - ExtensionData
	// =====================================================================================================


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


	#endregion Sub-classes



};