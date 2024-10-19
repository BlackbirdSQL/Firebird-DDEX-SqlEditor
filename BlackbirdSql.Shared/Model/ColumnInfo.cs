// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ColumnInfo

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.IO;



namespace BlackbirdSql.Shared.Model;


public class ColumnInfo : IBsColumnInfo
{
	protected string _ColumnName;

	protected string _DataTypeName;

	protected string _ProviderSpecificDataTypeName;

	protected Type _FieldType;

	protected int _MaxLength;

	protected int _Precision;

	public const int C_ColumnSizeIndex = 2;

	public const int C_PrecisionIndex = 4;

	private static readonly Dictionary<string, bool> _AllServerDataTypes;

	public const int C_UdtAssemblyQualifiedNameIndex = 28;

	private static readonly StorageDataEntity _SColumnDataEntity;

	private bool _IsUdtField;

	protected bool _IsBlobField;

	protected bool _IsCharsField;

	protected bool _IsBytesField;

	protected bool _IsXml;

	protected bool _IsSqlVariant;




	public bool IsUdtField => _IsUdtField;


	public bool IsBlobField => _IsBlobField;


	public bool IsCharsField => _IsCharsField;

	public bool IsBytesField => _IsBytesField;

	public bool IsXml => _IsXml;


	public bool IsSqlVariant => _IsSqlVariant;


	public int MaxLength => _MaxLength;


	public int Precision => _Precision;


	public string DataTypeName => _DataTypeName;


	public string ProviderSpecificDataTypeName => _ProviderSpecificDataTypeName;


	public string ColumnName => _ColumnName;


	public Type FieldType => _FieldType;





	static ColumnInfo()
	{
		_SColumnDataEntity = new StorageDataEntity();
		_AllServerDataTypes = new Dictionary<string, bool>(29);
		_AllServerDataTypes.Add("bigint", value: false);
		_AllServerDataTypes.Add("binary", value: false);
		_AllServerDataTypes.Add("boolean", value: false);
		_AllServerDataTypes.Add("char", value: false);
		_AllServerDataTypes.Add("datetime", value: false);
		_AllServerDataTypes.Add("decimal", value: false);
		_AllServerDataTypes.Add("float", value: false);
		_AllServerDataTypes.Add("image", value: false);
		_AllServerDataTypes.Add("integer", value: false);
		_AllServerDataTypes.Add("double", value: false);
		_AllServerDataTypes.Add("dec16", value: false);
		_AllServerDataTypes.Add("dec34", value: false);
		_AllServerDataTypes.Add("null", value: false);
		_AllServerDataTypes.Add("int128", value: false);
		_AllServerDataTypes.Add("guid", value: false);
		_AllServerDataTypes.Add("timetz", value: false);
		_AllServerDataTypes.Add("smallint", value: false);
		_AllServerDataTypes.Add("numeric", value: false);
		_AllServerDataTypes.Add("text", value: false);
		_AllServerDataTypes.Add("timestamp", value: false);
		_AllServerDataTypes.Add("tinyint", value: false);
		_AllServerDataTypes.Add("varbinary", value: false);
		_AllServerDataTypes.Add("blob sub_type 1", value: false);
		_AllServerDataTypes.Add("blob sub_type 2", value: false);
		_AllServerDataTypes.Add("blob sub_type 3", value: false);
		_AllServerDataTypes.Add("varchar", value: false);
		_AllServerDataTypes.Add("array", value: false);
		_AllServerDataTypes.Add("timetzex", value: false);
		_AllServerDataTypes.Add("date", value: false);
		_AllServerDataTypes.Add("time", value: false);
		_AllServerDataTypes.Add("timestamptz", value: false);
		_AllServerDataTypes.Add("timestamptzex", value: false);
	}

	public ColumnInfo(string name, string serverDataTypeName, string providerSpecificDataTypeName, Type fieldType, int maxLength)
	{
		_ColumnName = name;
		_DataTypeName = serverDataTypeName;
		_ProviderSpecificDataTypeName = providerSpecificDataTypeName;
		_FieldType = fieldType;
		_MaxLength = maxLength;
		_Precision = 0;
		InitFieldTypes(providerSpecificDataTypeName);
	}


	public ColumnInfo()
	{
	}

	public ColumnInfo(string name)
	{
		_ColumnName = name;
	}


	public async Task<bool> InitializeAsync(StorageDataReader reader, int colIndex, CancellationToken cancelToken)
	{
		_ColumnName = reader.GetName(colIndex);
		_DataTypeName = reader.GetDataTypeName(colIndex);

		DataTable schemaTable;

		// Evs.Trace(GetType(), ".InitializeAsync", "ASYNC GetSchemaTableAsync()");

		try
		{
			schemaTable = await reader.GetSchemaTableAsync(cancelToken);
		}
		catch (Exception ex)
		{
			if (ex is OperationCanceledException || cancelToken.Cancelled())
				return false;
			throw;
		}

		_MaxLength = (int)schemaTable.Rows[colIndex][C_ColumnSizeIndex];
		if (!DBNull.Value.Equals(schemaTable.Rows[colIndex][C_PrecisionIndex]))
		{
			_Precision = Convert.ToInt32(schemaTable.Rows[colIndex][C_PrecisionIndex]);
		}

		InitFieldTypes();
		if (!_IsUdtField)
		{
			_ProviderSpecificDataTypeName = reader.GetProviderSpecificDataTypeName(colIndex);
			_FieldType = reader.GetFieldType(colIndex);

			if (_IsBytesField && _ProviderSpecificDataTypeName != null
				&& _ProviderSpecificDataTypeName.ToLowerInvariant() == "system.string")
			{
				_IsBytesField = false;
				_IsCharsField = true;
			}

			return true;
		}

		object obj = schemaTable.Rows[colIndex][C_UdtAssemblyQualifiedNameIndex];
		string text = "MICROSOFT.SQLSERVER.TYPES.SQLHIERARCHYID";
		if (obj != null && string.Compare(obj.ToString(), 0, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			_ProviderSpecificDataTypeName = "System.Data.SqlTypes.SqlBinary";
			_FieldType = _SColumnDataEntity.TypeSqlBinary;
		}
		else
		{
			_ProviderSpecificDataTypeName = "System.Byte[]";
			_FieldType = _SColumnDataEntity.TypeBytes;
			_MaxLength = int.MaxValue;
		}

		return true;
	}




	public void InitFieldTypes(string providerSpecificDataTypeName = null)
	{
		string dataTypeName = DataTypeName.ToLowerInvariant();

		if (providerSpecificDataTypeName != null)
			providerSpecificDataTypeName = providerSpecificDataTypeName.ToLowerInvariant();
		// if (text.Contains("date"))
		//	Tracer.Trace(GetType(), "InitFieldTypes()", "Type: {0}.", text);
		switch (dataTypeName)
		{
			case "varchar":
			case "nvarchar":
				_IsCharsField = true;
				if (MaxLength == int.MaxValue)
				{
					if ("Microsoft SQL Server 2005 XML Showplan" == ColumnName)
					{
						_IsXml = true;
					}

					_IsBlobField = true;
				}

				break;
			case "text":
			case "ntext":
				_IsCharsField = true;
				_IsBlobField = true;
				break;
			case "xml":
				_IsXml = true;
				_IsBlobField = true;
				break;
			case "array":
			case "guid":
			case "binary":
			case "blob":
			case "blob sub_type 1":
			case "blob sub_type 2":
			case "blob sub_type 3":
			case "image":
				_IsBytesField = true;
				_IsBlobField = true;
				break;
			case "varbinary":
			case "rowversion":
				_IsBytesField = true;
				if (MaxLength == int.MaxValue)
				{
					_IsBlobField = true;
				}

				break;
			case "timestamp":
			case "timestamptz":
			case "timestamptzex":
			case "timetz":
			case "timetzex":
				_IsCharsField = true;
				break;
			case "sql_variant":
				_IsSqlVariant = true;
				break;
			default:
				if (!_AllServerDataTypes.ContainsKey(dataTypeName))
				{
					Diag.StackException("Invalid DataTypeName _FieldType: " + dataTypeName);
					_IsUdtField = true;
					_IsBytesField = true;
					_IsBlobField = true;
				}
				break;
		}


		if (_IsBytesField && providerSpecificDataTypeName != null
			&& providerSpecificDataTypeName == "system.string")
		{
			_IsBytesField = false;
			_IsCharsField = true;
		}
	}
}
