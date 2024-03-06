// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.StorageVariables

using System;
using System.Data.SqlTypes;


// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Model.IO;

public class StorageDataEntity
{
	public Type TypeDbNull;

	public string StringValue;

	public Type TypeString;

	public SqlString SqlStringValue;

	public Type TypeSqlString;

	public short Int16Value;

	public Type TypeInt16;

	public SqlInt16 SqlInt16Value;

	public Type TypeSqlInt16;

	public int Int32Value;

	public Type TypeInt32;

	public SqlInt32 SqlInt32Value;

	public Type TypeSqlInt32;

	public long Int64Value;

	public Type TypeInt64;

	public SqlInt64 SqlInt64Value;

	public Type TypeSqlInt64;

	public byte ByteValue;

	public Type TypeByte;

	public SqlByte SqlByteValue;

	public Type TypeSqlByte;

	public char CharValue;

	public Type TypeChar;

	public bool BoolValue;

	public Type TypeBool;

	public SqlBoolean SqlBoolValue;

	public Type TypeSqlBool;

	public double DoubleValue;

	public Type TypeDouble;

	public SqlDouble SqlDoubleValue;

	public Type TypeSqlDouble;

	public decimal DecimalValue;

	public Type TypeDecimal;

	public SqlDecimal SqlDecimalValue;

	public Type TypeSqlDecimal;

	public DateTime DateTimeValue;

	public Type TypeDateTime;

	public SqlDateTime SqlDateTimeValue;

	public Type TypeSqlDateTime;

	public byte[] BytesValue;

	public Type TypeBytes;

	public SqlBytes SqlBytesValue;

	public Type TypeSqlBytes;

	public SqlSingle SqlSingleValue;

	public Type TypeSqlSingle;

	public SqlGuid SqlGuidValue;

	public Type TypeSqlGuid;

	public Guid GuidValue;

	public Type TypeGuid;

	public SqlBinary SqlBinaryValue;

	public Type TypeSqlBinary;

	public SqlMoney SqlMoneyValue;

	public Type TypeSqlMoney;

	public DateTimeOffset DateTimeOffsetValue;

	public Type TypeDateTimeOffset;

	public TimeSpan TimeSpanValue;

	public Type TypeTimeSpan;

	public StorageDataEntity()
	{
		InitVariables();
	}

	protected void InitVariables()
	{
		TypeDbNull = Type.GetType("System.DBNull");
		StringValue = string.Empty;
		TypeString = StringValue.GetType();
		SqlStringValue = string.Empty;
		TypeSqlString = SqlStringValue.GetType();
		Int16Value = 0;
		TypeInt16 = Int16Value.GetType();
		SqlInt16Value = 0;
		TypeSqlInt16 = SqlInt16Value.GetType();
		Int32Value = 0;
		TypeInt32 = Int32Value.GetType();
		SqlInt32Value = 0;
		TypeSqlInt32 = SqlInt32Value.GetType();
		Int64Value = 0L;
		TypeInt64 = Int64Value.GetType();
		SqlInt64Value = 0L;
		TypeSqlInt64 = SqlInt64Value.GetType();
		ByteValue = 0;
		TypeByte = ByteValue.GetType();
		SqlByteValue = 0;
		TypeSqlByte = SqlByteValue.GetType();
		CharValue = '\0';
		TypeChar = CharValue.GetType();
		BoolValue = false;
		TypeBool = BoolValue.GetType();
		SqlBoolValue = false;
		TypeSqlBool = SqlBoolValue.GetType();
		DoubleValue = 0.0;
		TypeDouble = DoubleValue.GetType();
		SqlDoubleValue = 0.0;
		TypeSqlDouble = SqlDoubleValue.GetType();
		DecimalValue = default;
		TypeDecimal = DecimalValue.GetType();
		SqlDecimalValue = new SqlDecimal(0);
		TypeSqlDecimal = SqlDecimalValue.GetType();
		DateTimeValue = DateTime.Now;
		TypeDateTime = DateTimeValue.GetType();
		SqlDateTimeValue = DateTime.Now;
		TypeSqlDateTime = SqlDateTimeValue.GetType();
		BytesValue = new byte[3];
		TypeBytes = Type.GetType("System.Byte[]");
		SqlBytesValue = new SqlBytes(BytesValue);
		TypeSqlBytes = SqlBytesValue.GetType();
		SqlBinaryValue = new SqlBinary(BytesValue);
		TypeSqlBinary = SqlBinaryValue.GetType();
		SqlSingleValue = 0f;
		TypeSqlSingle = SqlSingleValue.GetType();
		SqlGuidValue = new SqlGuid(Guid.Empty);
		TypeSqlGuid = SqlGuidValue.GetType();
		GuidValue = Guid.Empty;
		TypeGuid = GuidValue.GetType();
		SqlSingleValue = 0f;
		TypeSqlSingle = SqlSingleValue.GetType();
		SqlMoneyValue = 0m;
		TypeSqlMoney = SqlMoneyValue.GetType();
		DateTimeOffsetValue = DateTimeOffset.Now;
		TypeDateTimeOffset = DateTimeOffsetValue.GetType();
		TimeSpanValue = TimeSpan.Zero;
		TypeTimeSpan = TimeSpanValue.GetType();
	}
}
