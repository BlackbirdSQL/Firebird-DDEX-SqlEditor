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
//											LsbIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Index for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbIndex : IIndex, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbIndex
	// ---------------------------------------------------------------------------------


	protected LsbIndex(IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
	{
		m_parent = parent;
		m_smoIndex = smoIndex;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbIndex
	// =========================================================================================================


	protected readonly IDatabaseTable m_parent;

	protected readonly Microsoft.SqlServer.Management.Smo.Index m_smoIndex;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbIndex
	// =========================================================================================================


	public string Name => m_smoIndex.Name;

	public SqlSmoObject SmoObject => m_smoIndex;

	public ITabular Parent => m_parent;

	public bool DisallowPageLocks => m_smoIndex.DisallowPageLocks;

	public bool DisallowRowLocks => m_smoIndex.DisallowRowLocks;

	public byte FillFactor => m_smoIndex.FillFactor;

	public bool IgnoreDuplicateKeys => m_smoIndex.IgnoreDuplicateKeys;

	public bool IsDisabled
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoIndex, "IsDisabled", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool PadIndex => m_smoIndex.PadIndex;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType Type
	{
		get
		{
			if (Cmd.TryGetPropertyValue((SqlSmoObject)m_smoIndex, "IsXmlIndex", out bool? value) && value.HasValue && value.Value)
			{
				return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Xml;
			}
			if (Cmd.TryGetPropertyValue((SqlSmoObject)m_smoIndex, "IsSpatialIndex", out value) && value.HasValue && value.Value)
			{
				return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Spatial;
			}
			return Microsoft.SqlServer.Management.SqlParser.Metadata.IndexType.Relational;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbIndex
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
	#region									Nested types - LsbIndex
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.IndexCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.UnorderedCollectionHelperI<IIndex, Microsoft.SqlServer.Management.Smo.Index>
	{
		public CollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.IndexCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}


		private readonly Microsoft.SqlServer.Management.Smo.IndexCollection smoCollection;

		private readonly IDatabaseTable dbTable;


		protected override AbstractDatabaseObject.IMetadataListI<Microsoft.SqlServer.Management.Smo.Index> RetrieveSmoMetadataList()
		{
			return new AbstractDatabaseObject.SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Index>(m_database.Server, smoCollection);
		}

		protected override IMutableMetadataCollection<IIndex> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.IndexCollection(initialCapacity, collationInfo);
		}

		protected override IIndex CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Index smoObject)
		{
			if (IsSpatialIndex(smoObject))
			{
				return new LsbSpatialIndex(dbTable, smoObject);
			}
			if (IsXmlIndex(smoObject))
			{
				return new LsbXmlIndex(dbTable, smoObject);
			}
			return new LsbRelationalIndex(m_database, dbTable, smoObject);
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

		public IndexedColumnCollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.IndexedColumnCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}

		protected override AbstractDatabaseObject.IMetadataListI<IndexedColumn> RetrieveSmoMetadataList()
		{
			return new AbstractDatabaseObject.SmoCollectionMetadataListI<IndexedColumn>(m_database.Server, smoCollection);
		}

		protected override IIndexedColumn CreateMetadataObject(IndexedColumn smoObject)
		{
			IIndexFactory index = LsbMetadataFactory.Instance.Index;
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
