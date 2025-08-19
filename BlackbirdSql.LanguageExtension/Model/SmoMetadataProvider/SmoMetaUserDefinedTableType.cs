// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedTableType

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaUserDefinedTableType : AbstractSmoMetaTableViewTableType<Microsoft.SqlServer.Management.Smo.UserDefinedTableType>, IUserDefinedTableType, IUserDefinedType, IDataType, IMetadataObject, ISchemaOwnedObject, IDatabaseObject, ITableDataType, IDatabaseTable, ITabular
{
	public SmoMetaUserDefinedTableType(Microsoft.SqlServer.Management.Smo.UserDefinedTableType metadataObject, SmoMetaSchema schema)
		: base(metadataObject, schema)
	{
		constraintCollection = new SmoMetaConstraint.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject);
		indexCollection = new AbstractSmoMetaIndex.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Indexes);
	}



	private readonly SmoMetaConstraint.CollectionHelperI constraintCollection;

	private readonly AbstractSmoMetaIndex.CollectionHelperI indexCollection;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public override TabularType TabularType => TabularType.TableDataType;

	protected override SmoMetaConstraint.CollectionHelperI ConstraintCollection => constraintCollection;

	protected override AbstractSmoMetaIndex.CollectionHelperI IndexCollection => indexCollection;

	public bool IsScalar => false;

	public bool IsTable => true;

	public bool IsCursor => false;

	public bool IsUnknown => false;

	public IScalarDataType AsScalarDataType => null;

	public ITableDataType AsTableDataType => this;

	public IUserDefinedType AsUserDefinedType => this;


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
