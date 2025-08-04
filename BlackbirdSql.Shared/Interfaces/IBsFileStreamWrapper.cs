// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IFileStreamWrapper
using System;


namespace BlackbirdSql.Shared.Interfaces;

internal interface IBsFileStreamWrapper : IDisposable
{
	void Init(string sFileName, bool bOpenExisting);

	void Init(string sFileName, bool bOpenExisting, int iBufferSize);

	int ReadData(byte[] buf, int iBytes);

	int ReadData(byte[] buf, int iBytes, long i64Offset);

	int WriteData(byte[] buf, int iBytes);

	int WriteDataDirect(long i64Offset, byte[] buf, int iBytes);

	void FlushBuffer();
}
