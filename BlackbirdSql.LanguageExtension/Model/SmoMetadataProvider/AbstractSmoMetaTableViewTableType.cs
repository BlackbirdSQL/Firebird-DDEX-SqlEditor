// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.TableViewTableTypeBase<S>

using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal abstract class AbstractSmoMetaTableViewTableType<S> : AbstractSmoMetaSchemaOwnedObject<S>, IDatabaseTable, ITabular, IMetadataObject where S : TableViewTableTypeBase
{
	protected AbstractSmoMetaTableViewTableType(S smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
		_ColumnCollection = new SmoMetaColumn.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Columns);
	}



	private readonly SmoMetaColumn.CollectionHelperI _ColumnCollection;

	public abstract TabularType TabularType { get; }

	public IMetadataOrderedCollection<IColumn> Columns => _ColumnCollection.MetadataCollection;

	public ITabular Unaliased => this;

	public IMetadataCollection<IConstraint> Constraints => ConstraintCollection.MetadataCollection;

	public IMetadataCollection<IIndex> Indexes => IndexCollection.MetadataCollection;

	public IMetadataCollection<IStatistics> Statistics => Collection<IStatistics>.Empty;

	protected abstract SmoMetaConstraint.CollectionHelperI ConstraintCollection { get; }

	protected abstract AbstractSmoMetaIndex.CollectionHelperI IndexCollection { get; }

}
