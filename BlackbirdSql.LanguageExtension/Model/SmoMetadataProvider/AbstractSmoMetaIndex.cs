// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Index

using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Index for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaIndex : IIndex, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaIndex
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaIndex(IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
	{
		_Parent = parent;
		_SmoIndex = smoIndex;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaIndex
	// =========================================================================================================


	protected readonly IDatabaseTable _Parent;

	protected readonly Microsoft.SqlServer.Management.Smo.Index _SmoIndex;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaIndex
	// =========================================================================================================


	public string Name => _SmoIndex.Name;

	public SqlSmoObject SmoObject => _SmoIndex;

	public ITabular Parent => _Parent;

	public bool DisallowPageLocks => _SmoIndex.DisallowPageLocks;

	public bool DisallowRowLocks => _SmoIndex.DisallowRowLocks;

	public byte FillFactor => _SmoIndex.FillFactor;

	public bool IgnoreDuplicateKeys => _SmoIndex.IgnoreDuplicateKeys;

	public bool IsDisabled
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "IsDisabled", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool PadIndex => _SmoIndex.PadIndex;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType Type
	{
		get
		{
			if (Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "IsXmlIndex", out bool? value) && value.HasValue && value.Value)
			{
				return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Xml;
			}
			if (Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "IsSpatialIndex", out value) && value.HasValue && value.Value)
			{
				return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Spatial;
			}
			return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Relational;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaIndex
	// =========================================================================================================


	public abstract T Accept<T>(IMetadataObjectVisitor<T> visitor);



	public static bool IsSpatialIndex(Microsoft.SqlServer.Management.Smo.Index smoIndex)
	{
		Cmd.TryGetPropertyValue((SqlSmoObject)smoIndex, "IsSpatialIndex", out bool? value);
		return value.GetValueOrDefault();
	}


	public static bool IsXmlIndex(Microsoft.SqlServer.Management.Smo.Index smoIndex)
	{
		Cmd.TryGetPropertyValue((SqlSmoObject)smoIndex, "IsXmlIndex", out bool? value);
		return value.GetValueOrDefault();
	}

	#endregion Methods





	// =========================================================================================================
	#region									Nested types - AbstractSmoMetaIndex
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.IndexCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.UnorderedCollectionHelperI<IIndex, Microsoft.SqlServer.Management.Smo.Index>
	{
		public CollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.IndexCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}


		private readonly Microsoft.SqlServer.Management.Smo.IndexCollection smoCollection;

		private readonly IDatabaseTable dbTable;


		protected override AbstractSmoMetaDatabaseObjectBase.IMetadataListI<Microsoft.SqlServer.Management.Smo.Index> RetrieveSmoMetadataList()
		{
			return new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Index>(_Database.Server, smoCollection);
		}

		protected override IMutableMetadataCollection<IIndex> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.IndexCollection(initialCapacity, collationInfo);
		}

		protected override IIndex CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Index smoObject)
		{
			if (IsSpatialIndex(smoObject))
			{
				return new SmoMetaSpatialIndex(dbTable, smoObject);
			}
			if (IsXmlIndex(smoObject))
			{
				return new SmoMetaXmlIndex(dbTable, smoObject);
			}
			return new SmoMetaRelationalIndex(_Database, dbTable, smoObject);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: IndexedColumnCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class IndexedColumnCollectionHelperI : Cmd.OrderedCollectionHelperI<IIndexedColumn, IndexedColumn>
	{
		private readonly Microsoft.SqlServer.Management.Smo.IndexedColumnCollection smoCollection;

		private readonly IDatabaseTable dbTable;

		public IndexedColumnCollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.IndexedColumnCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}

		protected override AbstractSmoMetaDatabaseObjectBase.IMetadataListI<IndexedColumn> RetrieveSmoMetadataList()
		{
			return new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<IndexedColumn>(_Database.Server, smoCollection);
		}

		protected override IIndexedColumn CreateMetadataObject(IndexedColumn smoObject)
		{
			IIndexFactory index = SmoMetaSmoMetadataFactory.Instance.Index;
			IColumn referencedColumn = dbTable.Columns[smoObject.Name];
			IMutableIndexedColumn mutableIndexedColumn = index.CreateIndexedColumn(referencedColumn);
			mutableIndexedColumn.SortOrder = (smoObject.Descending ? Microsoft.SqlServer.Management.SqlParser.Metadata.SortOrder.Descending : Microsoft.SqlServer.Management.SqlParser.Metadata.SortOrder.Ascending);
			Cmd.TryGetPropertyValue((SqlSmoObject)smoObject, "IsIncluded", out bool? value);
			mutableIndexedColumn.IsIncluded = value.GetValueOrDefault();
			return mutableIndexedColumn;
		}
	}


	#endregion Nested types

}
