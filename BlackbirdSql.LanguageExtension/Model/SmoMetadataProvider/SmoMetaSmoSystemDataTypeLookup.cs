// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoSystemDataTypeLookup

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaSmoSystemDataTypeLookup Class
//
/// <summary>
/// Impersonation of an SQL Server Smo SmoSystemDataTypeLookup for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaSmoSystemDataTypeLookup : SystemDataTypeLookupBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaSmoSystemDataTypeLookup
	// ---------------------------------------------------------------------------------


	private SmoMetaSmoSystemDataTypeLookup()
	{
	}



	public static SmoMetaSmoSystemDataTypeLookup Instance => SingletonI.Instance;


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - SmoMetaSmoSystemDataTypeLookup
	// =========================================================================================================


	public ISystemDataType Find(DataType smoDataType)
	{
		ISystemDataType result = null;
		DataTypeSpec dataTypeSpec = GetDataTypeSpec(smoDataType.SqlDataType);
		if (dataTypeSpec != null)
		{
			result = ((dataTypeSpec.ArgSpec2 != null) ? Find(dataTypeSpec, smoDataType.NumericPrecision, smoDataType.NumericScale) : ((dataTypeSpec.ArgSpec1 == null) ? Find(dataTypeSpec, isMaximum: false) : Find(dataTypeSpec, dataTypeSpec.ArgIsScale ? smoDataType.NumericScale : smoDataType.MaximumLength)));
		}
		return result;
	}

	public ISystemDataType RetrieveSystemDataType(Microsoft.SqlServer.Management.Smo.UserDefinedDataType smoUserDefinedDataType)
	{
		DataTypeSpec dataTypeSpec = DataTypeSpec.GetDataTypeSpec(smoUserDefinedDataType.SystemType);
		if (smoUserDefinedDataType.Length == -1)
		{
			switch (dataTypeSpec.SqlDataType)
			{
			case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.NVarChar:
				dataTypeSpec = DataTypeSpec.NVarCharMax;
				break;
			case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarBinary:
				dataTypeSpec = DataTypeSpec.VarBinaryMax;
				break;
			case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarChar:
				dataTypeSpec = DataTypeSpec.VarCharMax;
				break;
			}
		}
		if (dataTypeSpec.ArgSpec2 != null)
		{
			return Find(dataTypeSpec, smoUserDefinedDataType.NumericPrecision, smoUserDefinedDataType.NumericScale);
		}
		if (dataTypeSpec.ArgSpec1 != null)
		{
			int precisionOrMaxLength = (dataTypeSpec.ArgIsScale ? smoUserDefinedDataType.NumericScale : smoUserDefinedDataType.Length);
			return Find(dataTypeSpec, precisionOrMaxLength);
		}
		return Find(dataTypeSpec, isMaximum: false);
	}

	private static DataTypeSpec GetDataTypeSpec(Microsoft.SqlServer.Management.Smo.SqlDataType sqlDataType)
	{
		return sqlDataType switch
		{
			Microsoft.SqlServer.Management.Smo.SqlDataType.BigInt => DataTypeSpec.BigInt, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Binary => DataTypeSpec.Binary, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Bit => DataTypeSpec.Bit, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Char => DataTypeSpec.Char, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Date => DataTypeSpec.Date, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.DateTime => DataTypeSpec.DateTime, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.DateTime2 => DataTypeSpec.DateTime2, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.DateTimeOffset => DataTypeSpec.DateTimeOffset, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Decimal => DataTypeSpec.Decimal, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Float => DataTypeSpec.Float, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Geography => DataTypeSpec.Geography, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Geometry => DataTypeSpec.Geometry, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.HierarchyId => DataTypeSpec.HierarchyId, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Image => DataTypeSpec.Image, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Int => DataTypeSpec.Int, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Money => DataTypeSpec.Money, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.NChar => DataTypeSpec.NChar, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.NText => DataTypeSpec.NText, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Numeric => DataTypeSpec.Numeric, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.NVarChar => DataTypeSpec.NVarChar, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.NVarCharMax => DataTypeSpec.NVarCharMax, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Real => DataTypeSpec.Real, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.SmallDateTime => DataTypeSpec.SmallDateTime, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.SmallInt => DataTypeSpec.SmallInt, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.SmallMoney => DataTypeSpec.SmallMoney, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.SysName => DataTypeSpec.SysName, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Text => DataTypeSpec.Text, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Time => DataTypeSpec.Time, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Timestamp => DataTypeSpec.Timestamp, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.TinyInt => DataTypeSpec.TinyInt, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.UniqueIdentifier => DataTypeSpec.UniqueIdentifier, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.VarBinary => DataTypeSpec.VarBinary, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.VarBinaryMax => DataTypeSpec.VarBinaryMax, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.VarChar => DataTypeSpec.VarChar, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.VarCharMax => DataTypeSpec.VarCharMax, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Variant => DataTypeSpec.Variant, 
			Microsoft.SqlServer.Management.Smo.SqlDataType.Xml => DataTypeSpec.Xml, 
			_ => null, 
		};
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private static bool IsSmoUserDefinedDataType(Microsoft.SqlServer.Management.Smo.SqlDataType sqlDataType)
	{
		if (sqlDataType != Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedDataType && sqlDataType != Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedTableType)
		{
			return sqlDataType == Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedType;
		}
		return true;
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaSmoSystemDataTypeLookup
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Singleton.
	/// </summary>
	// ---------------------------------------------------------------------------------


	private static class SingletonI
	{
		static SingletonI()
		{
			Instance = new SmoMetaSmoSystemDataTypeLookup();
		}



		public static SmoMetaSmoSystemDataTypeLookup Instance;

	}


	#endregion Nested types

}
