// Microsoft.SqlServer.Management.SqlParser, Version=17.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderBase
using System;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals;



namespace BlackbirdSql.LanguageExtension.Model;


/// <summary>
/// Placeholder. Under development.
/// </summary>
public abstract class AbstractMetadataProvider : IMetadataProvider
{
	private IBuiltInFunctionLookup m_builtInFunctionLookup;

	private ICollationLookup m_collationLookup;

	private ISystemDataTypeLookup m_systemDataTypeLookup;

	private IMetadataFactory m_metadataFactory;

	public abstract IServer Server { get; }

	public IBuiltInFunctionLookup BuiltInFunctionLookup
	{
		get
		{
			return m_builtInFunctionLookup;
		}
		protected set
		{
			m_builtInFunctionLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public ICollationLookup CollationLookup
	{
		get
		{
			return m_collationLookup;
		}
		protected set
		{
			m_collationLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public ISystemDataTypeLookup SystemDataTypeLookup
	{
		get
		{
			return m_systemDataTypeLookup;
		}
		protected set
		{
			m_systemDataTypeLookup = value ?? throw new ArgumentNullException("value");
		}
	}

	public IMetadataFactory MetadataFactory
	{
		get
		{
			return m_metadataFactory;
		}
		protected set
		{
			m_metadataFactory = value ?? throw new ArgumentNullException("value");
		}
	}

	public abstract MetadataProviderEventHandler BeforeBindHandler { get; }

	public abstract MetadataProviderEventHandler AfterBindHandler { get; }

	protected AbstractMetadataProvider()
		: this(null, null, null, null)
	{
	}

	protected AbstractMetadataProvider(IBuiltInFunctionLookup builtInFunctionLookup, ICollationLookup collationLookup, ISystemDataTypeLookup systemDataTypeLookup, IMetadataFactory metadataFactory)
	{
		// TBC: Implementation of metadata factory and providers. These classes will provide the real 'meat' for predictive text.
		// m_builtInFunctionLookup = builtInFunctionLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.BuiltInFunctionLookup.Instance;
		// m_collationLookup = collationLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.CollationLookup.Instance;
		// m_systemDataTypeLookup = systemDataTypeLookup ?? Microsoft.SqlServer.Management.SqlParser.MetadataProvider.Internals.SystemDataTypeLookup.Instance;
		// m_metadataFactory = metadataFactory ?? new MetadataFactory();
	}
}
