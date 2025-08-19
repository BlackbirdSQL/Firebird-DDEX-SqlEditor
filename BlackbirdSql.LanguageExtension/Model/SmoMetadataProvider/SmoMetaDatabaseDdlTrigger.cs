// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseDdlTrigger

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaDatabaseDdlTrigger Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseDdlTrigger for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaDatabaseDdlTrigger : AbstractSmoMetaDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger>, IDatabaseDdlTrigger, ITrigger, IMetadataObject, IDatabaseOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaDatabaseDdlTrigger
	// ---------------------------------------------------------------------------------


	public SmoMetaDatabaseDdlTrigger(Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaDatabaseDdlTrigger
	// =========================================================================================================


	private IExecutionContext _ExecutionContext;

	private TriggerEventTypeSet _DatabaseEventSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaDatabaseDdlTrigger
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public ITriggerEventTypeSet DatabaseDdlEvents
	{
		get
		{
			_DatabaseEventSet ??= Cmd.DdlTriggerI.GetDatabaseTriggerEvents(_SmoMetadataObject);
			return _DatabaseEventSet;
		}
	}

	public bool IsQuotedIdentifierOn => _SmoMetadataObject.QuotedIdentifierStatus;

	public string BodyText => _SmoMetadataObject.TextBody;

	public bool IsEncrypted => _SmoMetadataObject.IsEncrypted;

	public bool IsEnabled => _SmoMetadataObject.IsEnabled;

	public bool IsSqlClr => _SmoMetadataObject.ImplementationType == ImplementationType.SqlClr;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (_ExecutionContext == null)
			{
				IDatabase database = base.Database;
				_ExecutionContext = Cmd.GetExecutionContext(database, _SmoMetadataObject);
			}
			return _ExecutionContext;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaDatabaseDdlTrigger
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
	#region									Nested types - SmoMetaDatabaseDdlTrigger
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.DatabaseDdlTriggerCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : CollectionHelperI<IDatabaseDdlTrigger, DatabaseDdlTrigger>
	{
		public CollectionHelperI(SmoMetaDatabase database)
		: base(database)
		{
		}

		protected override IMetadataListI<DatabaseDdlTrigger> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<DatabaseDdlTrigger>(_Database.Server, _Database.SmoMetadataObject.Triggers);
		}

		protected override IMutableMetadataCollection<IDatabaseDdlTrigger> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseDdlTriggerCollection(initialCapacity, collationInfo);
		}

		protected override IDatabaseDdlTrigger CreateMetadataObject(DatabaseDdlTrigger smoObject)
		{
			return new SmoMetaDatabaseDdlTrigger(smoObject, _Database);
		}
	}

	#endregion Nested types

}
