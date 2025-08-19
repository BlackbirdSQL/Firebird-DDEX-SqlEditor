// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedClrType

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaUserDefinedClrType : AbstractSmoMetaSchemaOwnedObject<UserDefinedType>, IUserDefinedClrType, IUserDefinedType, IDataType, IMetadataObject, ISchemaOwnedObject, IDatabaseObject, IClrDataType, IScalarDataType
{
	public SmoMetaUserDefinedClrType(UserDefinedType smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
		_DataMembers = Collection<IUdtDataMember>.Empty;
		m_methods = Collection<IUdtMethod>.Empty;
	}



	private readonly IMetadataCollection<IUdtDataMember> _DataMembers;

	private readonly IMetadataCollection<IUdtMethod> m_methods;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public bool IsScalar => true;

	public bool IsTable => false;

	public bool IsCursor => false;

	public bool IsUnknown => false;

	public IScalarDataType AsScalarDataType => this;

	public ITableDataType AsTableDataType => null;

	public IUserDefinedType AsUserDefinedType => this;

	public string AssemblyName => _SmoMetadataObject.AssemblyName;

	public string ClassName => _SmoMetadataObject.ClassName;

	public bool IsBinaryOrdered => _SmoMetadataObject.IsBinaryOrdered;

	public bool IsComVisible => _SmoMetadataObject.IsComVisible;

	public bool IsNullable => _SmoMetadataObject.IsNullable;

	public IMetadataCollection<IUdtMethod> Methods => m_methods;

	public IMetadataCollection<IUdtDataMember> DataMembers => _DataMembers;

	public bool IsSystem => false;

	public bool IsClr => true;

	public bool IsXml => false;

	public bool IsVoid => false;

	public ISystemDataType BaseSystemDataType => null;

	public IClrDataType AsClrDataType => this;


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
