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
//											SmoMetaRelationalIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo RelationalIndex for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaRelationalIndex : AbstractSmoMetaIndex, IRelationalIndex, IIndex, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaRelationalIndex
	// ---------------------------------------------------------------------------------


	public SmoMetaRelationalIndex(SmoMetaDatabase database, IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
		: base(parent, smoIndex)
	{
		_ColumnCollection = new AbstractSmoMetaIndex.IndexedColumnCollectionHelperI(database, parent, _SmoIndex.IndexedColumns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaRelationalIndex
	// =========================================================================================================


	private readonly AbstractSmoMetaIndex.IndexedColumnCollectionHelperI _ColumnCollection;

	private IUniqueConstraintBase _IndexKey;

	public bool CompactLargeObjects
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "CompactLargeObjects", out bool? value);
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
			Cmd.TryGetPropertyObject<string>(_SmoIndex, "FilterDefinition", out var value);
			return value;
		}
	}

	public IMetadataOrderedCollection<IIndexedColumn> IndexedColumns => _ColumnCollection.MetadataCollection;

	public IUniqueConstraintBase IndexKey
	{
		get
		{
			if (_IndexKey == null)
			{
				switch (_SmoIndex.IndexKeyType)
				{
				case IndexKeyType.DriPrimaryKey:
					_IndexKey = new SmoMetaPrimaryKeyConstraint(_Parent, this);
					break;
				case IndexKeyType.DriUniqueKey:
					_IndexKey = new SmoMetaUniqueConstraint(_Parent, this);
					break;
				}
			}
			return _IndexKey;
		}
	}

	public bool IsClustered => _SmoIndex.IsClustered;

	public bool IsSystemNamed => Cmd.GetPropertyValue<bool>(_SmoIndex, "IsSystemNamed").GetValueOrDefault();

	public bool IsUnique => _SmoIndex.IsUnique;

	public bool NoAutomaticRecomputation => _SmoIndex.NoAutomaticRecomputation;

	public bool OnlineIndexOperation => _SmoIndex.OnlineIndexOperation;

	public IPartitionScheme PartitionScheme => null;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaRelationalIndex
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
