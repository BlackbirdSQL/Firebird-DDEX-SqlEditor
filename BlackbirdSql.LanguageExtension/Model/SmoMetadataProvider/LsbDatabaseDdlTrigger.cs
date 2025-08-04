// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseDdlTrigger

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.LsbDatabase;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbDatabaseDdlTrigger Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseDdlTrigger for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbDatabaseDdlTrigger : LsbDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger>, IDatabaseDdlTrigger, ITrigger, IMetadataObject, IDatabaseOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDatabaseDdlTrigger
	// ---------------------------------------------------------------------------------


	public LsbDatabaseDdlTrigger(Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger smoMetadataObject, LsbDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbDatabaseDdlTrigger
	// =========================================================================================================


	private IExecutionContext m_executionContext;

	private TriggerEventTypeSet m_databaseEventSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbDatabaseDdlTrigger
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public ITriggerEventTypeSet DatabaseDdlEvents
	{
		get
		{
			m_databaseEventSet ??= Cmd.DdlTriggerI.GetDatabaseTriggerEvents(m_smoMetadataObject);
			return m_databaseEventSet;
		}
	}

	public bool IsQuotedIdentifierOn => m_smoMetadataObject.QuotedIdentifierStatus;

	public string BodyText => m_smoMetadataObject.TextBody;

	public bool IsEncrypted => m_smoMetadataObject.IsEncrypted;

	public bool IsEnabled => m_smoMetadataObject.IsEnabled;

	public bool IsSqlClr => m_smoMetadataObject.ImplementationType == ImplementationType.SqlClr;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (m_executionContext == null)
			{
				IDatabase database = base.Database;
				m_executionContext = Cmd.GetExecutionContext(database, m_smoMetadataObject);
			}
			return m_executionContext;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDatabaseDdlTrigger
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbDatabaseDdlTrigger
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.DatabaseDdlTriggerCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : CollectionHelperI<IDatabaseDdlTrigger, DatabaseDdlTrigger>
	{
		public CollectionHelperI(LsbDatabase database)
		: base(database)
		{
		}

		protected override IMetadataListI<DatabaseDdlTrigger> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<DatabaseDdlTrigger>(m_database.Server, m_database.SmoMetadataObject.Triggers);
		}

		protected override IMutableMetadataCollection<IDatabaseDdlTrigger> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseDdlTriggerCollection(initialCapacity, collationInfo);
		}

		protected override IDatabaseDdlTrigger CreateMetadataObject(DatabaseDdlTrigger smoObject)
		{
			return new LsbDatabaseDdlTrigger(smoObject, m_database);
		}
	}

	#endregion Nested types

}
