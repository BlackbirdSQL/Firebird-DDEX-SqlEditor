#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBColumnInfo
{
	string ColumnName { get; }

	string DataTypeName { get; }

	string ProviderSpecificDataTypeName { get; }

	Type FieldType { get; }

	bool IsBlobField { get; }

	bool IsCharsField { get; }
	bool IsBytesField { get; }

	bool IsXml { get; }

	bool IsSqlVariant { get; }

	bool IsUdtField { get; }

	int Precision { get; }
}
