// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.StorageVariables

using System;
using System.Data.SqlTypes;


// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Model.IO;

internal class StorageDataEntity
{
	internal Type TypeDbNull;

	internal string StringValue;

	internal Type TypeString;

	internal SqlString SqlStringValue;

	internal Type TypeSqlString;

	internal short Int16Value;

	internal Type TypeInt16;

	internal SqlInt16 SqlInt16Value;

	internal Type TypeSqlInt16;

	internal int Int32Value;

	internal Type TypeInt32;

	internal SqlInt32 SqlInt32Value;

	internal Type TypeSqlInt32;

	internal long Int64Value;

	internal Type TypeInt64;

	internal SqlInt64 SqlInt64Value;

	internal Type TypeSqlInt64;

	internal byte ByteValue;

	internal Type TypeByte;

	internal SqlByte SqlByteValue;

	internal Type TypeSqlByte;

	internal char CharValue;

	internal Type TypeChar;

	internal bool BoolValue;

	internal Type TypeBool;

	internal SqlBoolean SqlBoolValue;

	internal Type TypeSqlBool;

	internal double DoubleValue;

	internal Type TypeDouble;

	internal SqlDouble SqlDoubleValue;

	internal Type TypeSqlDouble;

	internal decimal DecimalValue;

	internal Type TypeDecimal;

	internal SqlDecimal SqlDecimalValue;

	internal Type TypeSqlDecimal;

	internal DateTime DateTimeValue;

	internal Type TypeDateTime;

	internal SqlDateTime SqlDateTimeValue;

	internal Type TypeSqlDateTime;

	internal byte[] BytesValue;

	internal Type TypeBytes;

	internal SqlBytes SqlBytesValue;

	internal Type TypeSqlBytes;

	internal SqlSingle SqlSingleValue;

	internal Type TypeSqlSingle;

	internal SqlGuid SqlGuidValue;

	internal Type TypeSqlGuid;

	internal Guid GuidValue;

	internal Type TypeGuid;

	internal SqlBinary SqlBinaryValue;

	internal Type TypeSqlBinary;

	internal SqlMoney SqlMoneyValue;

	internal Type TypeSqlMoney;

	internal DateTimeOffset DateTimeOffsetValue;

	internal Type TypeDateTimeOffset;

	internal TimeSpan TimeSpanValue;

	internal Type TypeTimeSpan;

	public StorageDataEntity()
	{
		InitVariables();
	}

	protected void InitVariables()
	{
		TypeDbNull = Type.GetType("System.DBNull");
		StringValue = "";
		TypeString = StringValue.GetType();
		SqlStringValue = "";
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
