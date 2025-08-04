// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.TableViewBase<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractTableView Class
//
/// <summary>
/// Impersonation of an SQL Server Smo TableViewBase for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractTableView<S> : LsbSchemaOwnedObject<S>, ITableViewBase, IDatabaseTable, ITabular, IMetadataObject, ISchemaOwnedObject, IDatabaseObject where S : TableViewBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbTable
	// ---------------------------------------------------------------------------------


	protected AbstractTableView(S smoMetadataObject, LsbSchema parent)
		: base(smoMetadataObject, parent)
	{
		columnCollection = new LsbColumn.CollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Columns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractTableView
	// =========================================================================================================


	private readonly LsbColumn.CollectionHelperI columnCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractTableView
	// =========================================================================================================


	public abstract TabularType TabularType { get; }

	public IMetadataOrderedCollection<IColumn> Columns => columnCollection.MetadataCollection;

	public ITabular Unaliased => this;

	public IMetadataCollection<IDmlTrigger> Triggers => TriggerCollection.MetadataCollection;

	public abstract bool IsQuotedIdentifierOn { get; }

	public IMetadataCollection<IConstraint> Constraints => ConstraintCollection.MetadataCollection;

	public IMetadataCollection<IIndex> Indexes => IndexCollection.MetadataCollection;

	public IMetadataCollection<IStatistics> Statistics => StatisticsCollection.MetadataCollection;

	protected abstract LsbConstraintCollection.CollectionHelperI ConstraintCollection { get; }

	protected abstract LsbIndex.CollectionHelperI IndexCollection { get; }

	protected abstract Cmd.StatisticsCollectionHelperI StatisticsCollection { get; }

	protected abstract LsbDmlTrigger.CollectionHelperI TriggerCollection { get; }


	#endregion Property accessors

}
