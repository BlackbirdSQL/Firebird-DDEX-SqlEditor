// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedDataType

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaUserDefinedDataType : AbstractSmoMetaSchemaOwnedObject<Microsoft.SqlServer.Management.Smo.UserDefinedDataType>, IUserDefinedDataType, IUserDefinedType, IDataType, IMetadataObject, ISchemaOwnedObject, IDatabaseObject, IScalarDataType
{
	public SmoMetaUserDefinedDataType(Microsoft.SqlServer.Management.Smo.UserDefinedDataType metadataObject, SmoMetaSchema schema)
		: base(metadataObject, schema)
	{
		_BaseSystemDataType = SmoMetaSmoSystemDataTypeLookup.Instance.RetrieveSystemDataType(metadataObject);
	}



	private readonly ISystemDataType _BaseSystemDataType;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public bool IsScalar => true;

	public bool IsTable => false;

	public bool IsCursor => false;

	public bool IsUnknown => false;

	public IScalarDataType AsScalarDataType => this;

	public ITableDataType AsTableDataType => null;

	public IUserDefinedType AsUserDefinedType => this;

	public bool IsSystem => false;

	public bool IsClr => false;

	public bool IsXml => false;

	public bool IsVoid => false;

	public ISystemDataType BaseSystemDataType => _BaseSystemDataType;

	public IClrDataType AsClrDataType => null;

	public bool Nullable => _SmoMetadataObject.Nullable;


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
