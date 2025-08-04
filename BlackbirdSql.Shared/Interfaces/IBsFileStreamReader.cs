// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IFileStreamReader
using System;
using System.Data.SqlTypes;


namespace BlackbirdSql.Shared.Interfaces;

internal interface IBsFileStreamReader : IDisposable
{
	void Init(string sFileName);

	int ReadInt16(long i64Offset, bool bSkipValue, ref bool IsNull, ref short iVal);

	int ReadInt32(long i64Offset, bool bSkipValue, ref bool IsNull, ref int iVal);

	int ReadInt64(long i64Offset, bool bSkipValue, ref bool IsNull, ref long iVal);

	int ReadByte(long i64Offset, bool bSkipValue, ref bool IsNull, ref byte bVal);

	int ReadChar(long i64Offset, bool bSkipValue, ref bool IsNull, ref char chVal);

	int ReadBoolean(long i64Offset, bool bSkipValue, ref bool IsNull, ref bool bVal);

	int ReadSingle(long i64Offset, bool bSkipValue, ref bool IsNull, ref float singleVal);

	int ReadDouble(long i64Offset, bool bSkipValue, ref bool IsNull, ref double dVal);

	int ReadSqlDecimal(long i64Offset, bool bSkipValue, ref bool IsNull, ref SqlDecimal dtVal);

	int ReadDecimal(long i64Offset, bool bSkipValue, ref bool IsNull, ref decimal dtVal);

	int ReadDateTime(long i64Offset, bool bSkipValue, ref bool IsNull, ref DateTime dtVal);

	int ReadTimeSpan(long i64Offset, bool bSkipValue, ref bool IsNull, ref TimeSpan tsVal);

	int ReadString(long i64Offset, bool bSkipValue, ref bool IsNull, ref string sVal);

	int ReadBytes(long i64Offset, bool bSkipValue, ref bool IsNull, ref byte[] bytesVal);

	int ReadDateTimeOffset(long i64Offset, bool bSkipValue, ref bool IsNull, ref DateTimeOffset dtoVal);
}
