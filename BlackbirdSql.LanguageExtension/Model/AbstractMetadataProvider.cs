// Microsoft.SqlServer.Management.SqlParser, Version=17.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderBase

using System;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals;



namespace BlackbirdSql.LanguageExtension.Model;




// =========================================================================================================
//
//											AbstractMetadataProvider Class
//
/// <summary>
/// Language service IMetadataProvider base implementation.
/// </summary>
// =========================================================================================================
public abstract class AbstractMetadataProvider : IMetadataProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractMetadataProvider
	// ---------------------------------------------------------------------------------


	protected AbstractMetadataProvider()
		: this(null, null, null, null)
	{
	}

	protected AbstractMetadataProvider(IBuiltInFunctionLookup builtInFunctionLookup, ICollationLookup collationLookup, ISystemDataTypeLookup systemDataTypeLookup, IMetadataFactory metadataFactory)
	{
		// TBC: Implementation of metadata factory and providers. These classes will provide the real 'meat' for predictive text.
		// _BuiltInFunctionLookup = builtInFunctionLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.BuiltInFunctionLookup.Instance;
		// _CollationLookup = collationLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.CollationLookup.Instance;
		// _SystemDataTypeLookup = systemDataTypeLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.SystemDataTypeLookup.Instance;
		// _MetadataFactory = metadataFactory ?? new MetadataFactory();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractMetadataProvider
	// =========================================================================================================


	private IBuiltInFunctionLookup _BuiltInFunctionLookup;

	private ICollationLookup _CollationLookup;

	private ISystemDataTypeLookup _SystemDataTypeLookup;

	private IMetadataFactory _MetadataFactory;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractMetadataProvider
	// =========================================================================================================


	public abstract IServer Server { get; }

	public IBuiltInFunctionLookup BuiltInFunctionLookup
	{
		get
		{
			return _BuiltInFunctionLookup;
		}
		protected set
		{
			_BuiltInFunctionLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public ICollationLookup CollationLookup
	{
		get
		{
			return _CollationLookup;
		}
		protected set
		{
			_CollationLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public ISystemDataTypeLookup SystemDataTypeLookup
	{
		get
		{
			return _SystemDataTypeLookup;
		}
		protected set
		{
			_SystemDataTypeLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public IMetadataFactory MetadataFactory
	{
		get
		{
			return _MetadataFactory;
		}
		protected set
		{
			_MetadataFactory = value ?? throw new ArgumentNullException("value");
		}
	}


	public abstract MetadataProviderEventHandler BeforeBindHandler { get; }

	public abstract MetadataProviderEventHandler AfterBindHandler { get; }


	#endregion Property accessors

}
