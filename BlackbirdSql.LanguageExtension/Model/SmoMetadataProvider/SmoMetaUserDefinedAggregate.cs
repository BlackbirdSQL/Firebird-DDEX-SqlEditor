// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedAggregate

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaUserDefinedAggregate : AbstractSmoMetaSchemaOwnedObject<Microsoft.SqlServer.Management.Smo.UserDefinedAggregate>, IUserDefinedAggregate, IScalarFunction, IFunction, IMetadataObject, IFunctionModuleBase, IScalar, ISchemaOwnedObject, IDatabaseObject
{
	public SmoMetaUserDefinedAggregate(Microsoft.SqlServer.Management.Smo.UserDefinedAggregate aggregate, SmoMetaSchema schema)
		: base(aggregate, schema)
	{
	}

	private IScalarDataType _DataType;

	private ParameterCollection _Parameters;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public bool IsAggregateFunction => true;

	public IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (_Parameters == null)
			{
				SmoMetaDatabase parent = base.Parent.Parent;
				_Parameters = AbstractSmoMetaUserDefinedFunction.CreateParameterCollection(parent, _SmoMetadataObject.Parameters, null);
			}
			return _Parameters;
		}
	}

	public ScalarType ScalarType => ScalarType.ScalarFunction;

	public IScalarDataType DataType
	{
		get
		{
			if (_DataType == null)
			{
				IDataType dataType = Cmd.GetDataType(base.Parent.Parent, _SmoMetadataObject.DataType);
				_DataType = dataType as IScalarDataType;
			}
			return _DataType;
		}
	}

	public bool Nullable => true;


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
