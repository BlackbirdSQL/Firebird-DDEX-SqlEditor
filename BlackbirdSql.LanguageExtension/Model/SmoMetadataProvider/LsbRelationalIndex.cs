// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.RelationalIndex

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbRelationalIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo RelationalIndex for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbRelationalIndex : LsbIndex, IRelationalIndex, IIndex, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbRelationalIndex
	// ---------------------------------------------------------------------------------


	public LsbRelationalIndex(LsbDatabase database, IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
		: base(parent, smoIndex)
	{
		columnCollection = new LsbIndex.IndexedColumnCollectionHelperI(database, parent, m_smoIndex.IndexedColumns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbRelationalIndex
	// =========================================================================================================


	private readonly LsbIndex.IndexedColumnCollectionHelperI columnCollection;

	private IUniqueConstraintBase m_indexKey;

	public bool CompactLargeObjects
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoIndex, "CompactLargeObjects", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public IFileGroup FileGroup => null;

	public IFileGroup FileStreamFileGroup => null;

	public IPartitionScheme FileStreamPartitionScheme => null;

	public string FilterDefinition
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(m_smoIndex, "FilterDefinition", out var value);
			return value;
		}
	}

	public IMetadataOrderedCollection<IIndexedColumn> IndexedColumns => columnCollection.MetadataCollection;

	public IUniqueConstraintBase IndexKey
	{
		get
		{
			if (m_indexKey == null)
			{
				switch (m_smoIndex.IndexKeyType)
				{
				case IndexKeyType.DriPrimaryKey:
					m_indexKey = new LsbPrimaryKeyConstraint(m_parent, this);
					break;
				case IndexKeyType.DriUniqueKey:
					m_indexKey = new LsbUniqueConstraint(m_parent, this);
					break;
				}
			}
			return m_indexKey;
		}
	}

	public bool IsClustered => m_smoIndex.IsClustered;

	public bool IsSystemNamed => Cmd.GetPropertyValue<bool>(m_smoIndex, "IsSystemNamed").GetValueOrDefault();

	public bool IsUnique => m_smoIndex.IsUnique;

	public bool NoAutomaticRecomputation => m_smoIndex.NoAutomaticRecomputation;

	public bool OnlineIndexOperation => m_smoIndex.OnlineIndexOperation;

	public IPartitionScheme PartitionScheme => null;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbRelationalIndex
	// =========================================================================================================


	public override T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
