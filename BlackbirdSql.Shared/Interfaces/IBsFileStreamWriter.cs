// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IFileStreamWriter
using System;
using System.Data.SqlTypes;


namespace BlackbirdSql.Shared.Interfaces;


internal interface IBsFileStreamWriter : IDisposable
{
	void Init(string sFileName);

	void Init(string sFileName, bool bOpenExisting);

	int WriteNull();

	int WriteInt16(short iVal);

	int WriteInt32(int iVal);

	int WriteInt32(long i64Offset, int iVal);

	int WriteInt64(long iVal);

	int WriteByte(byte bVal);

	int WriteChar(char chVal);

	int WriteBoolean(bool bVal);

	int WriteSingle(float singleVal);

	int WriteDouble(double dVal);

	int WriteDecimal(decimal decimalVal);

	int WriteSqlDecimal(SqlDecimal SqlDecimalValue);

	int WriteDateTime(DateTime dtVal);

	int WriteTimeSpan(TimeSpan tsVal);

	int WriteString(string s);

	int WriteBytes(byte[] bytesVal, int iLen);

	int ReadLength(long i64Offset, out int iLen);

	int WriteDateTimeOffset(DateTimeOffset dtoVal);

	void FlushBuffer();
}
