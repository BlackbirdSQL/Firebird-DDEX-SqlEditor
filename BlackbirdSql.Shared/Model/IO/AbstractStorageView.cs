#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using BlackbirdSql.Shared.Interfaces;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Model.IO;

public abstract class AbstractStorageView : IBsStorageView, IDisposable
{
	protected const int C_DEFAULT_MAX_NUM_BYTES_TO_DISPLAY = 256;

	protected int _MaxBytesToDisplay = C_DEFAULT_MAX_NUM_BYTES_TO_DISPLAY;

	protected StringBuilder _SbWork = new StringBuilder(514);

	protected string[] _HexDigits = ["A", "B", "C", "D", "E", "F"];

	private readonly StorageDataEntity _StorageViewDataEntity;

	public const string C_DateTimeFormatString = "yyyy-MM-dd HH:mm:ss.fff";

	public const string C_DateFormatString = "yyyy-MM-dd";

	public const string C_DateTime2FormatString = "yyyy-MM-dd HH:mm:ss{0}";

	public const string C_SmallDateTimeFormatString = "yyyy-MM-dd HH:mm:ss";

	public const string C_DateTimeOffsetFormatString = "yyyy-MM-dd HH:mm:ss{0} zzz";



	public StorageDataEntity StorageViewDataEntity => _StorageViewDataEntity;

	public abstract long EnsureRowsInBuf(long startRow, long totalRowCount);

	public abstract object GetCellData(long i64Row, int iCol);

	protected AbstractStorageView()
	{
		_StorageViewDataEntity = new StorageDataEntity();
	}

	public virtual string GetCellDataAsString(long iRow, int iCol)
	{
		object cellData = GetCellData(iRow, iCol);
		string result;

		if (cellData == null)
		{
			result = "NULL";
			return result;
		}

		Type type = cellData.GetType();

		if (type == StorageViewDataEntity.TypeBytes)
		{
			StorageViewDataEntity.BytesValue = (byte[])cellData;
		}
		else if (type == StorageViewDataEntity.TypeSqlBinary)
		{
			StorageViewDataEntity.SqlBinaryValue = (SqlBinary)cellData;
			StorageViewDataEntity.BytesValue = StorageViewDataEntity.SqlBinaryValue.Value;
			type = StorageViewDataEntity.TypeBytes;
			StorageViewDataEntity.SqlBinaryValue = null;
		}
		else if (type == StorageViewDataEntity.TypeSqlBytes)
		{
			StorageViewDataEntity.SqlBytesValue = (SqlBytes)cellData;
			StorageViewDataEntity.BytesValue = StorageViewDataEntity.SqlBytesValue.Value;
			type = StorageViewDataEntity.TypeBytes;
			StorageViewDataEntity.SqlBytesValue = null;
		}

		if (type == StorageViewDataEntity.TypeBytes)
		{
			_SbWork.Length = 0;
			_SbWork.Append("0x");
			int num = StorageViewDataEntity.BytesValue.Length;
			if (num > _MaxBytesToDisplay)
			{
				num = _MaxBytesToDisplay;
			}

			for (int i = 0; i < num; i++)
			{
				byte b = (byte)(StorageViewDataEntity.BytesValue[i] >> 4);
				byte b2 = (byte)(StorageViewDataEntity.BytesValue[i] & 0xFu);
				if (b >= 10)
				{
					_SbWork.Append(_HexDigits[b - 10]);
				}
				else
				{
					_SbWork.Append(b);
				}

				if (b2 >= 10)
				{
					_SbWork.Append(_HexDigits[b2 - 10]);
				}
				else
				{
					_SbWork.Append(b2);
				}
			}

			result = _SbWork.ToString();
			StorageViewDataEntity.BytesValue = null;
			_SbWork.Length = 0;
		}
		else if (type == StorageViewDataEntity.TypeDateTime)
		{
			string dataTypeName = GetColumnInfo(iCol).DataTypeName;
			string text;
			if (string.Compare(dataTypeName, "date", StringComparison.Ordinal) == 0)
			{
				text = C_DateFormatString;
			}
			else if (string.Compare(dataTypeName, "datetime2", StringComparison.Ordinal) == 0)
			{
				string text2 = "";
				int numericPrecision = GetColumnInfo(iCol).Precision;
				if (numericPrecision > 0)
				{
					text2 = "." + text2.PadLeft(Math.Min(numericPrecision, 7), 'f');
				}

				text = $"yyyy-MM-dd HH:mm:ss{text2}";
			}
			else
			{
				text = C_DateTimeFormatString;
			}

			StorageViewDataEntity.DateTimeValue = (DateTime)cellData;
			result = StorageViewDataEntity.DateTimeValue.ToString(text, CultureInfo.InvariantCulture);
		}
		else if (type == StorageViewDataEntity.TypeDateTimeOffset)
		{
			StorageViewDataEntity.DateTimeOffsetValue = (DateTimeOffset)cellData;
			string text3 = "";
			int numericPrecision2 = GetColumnInfo(iCol).Precision;
			if (numericPrecision2 > 0)
			{
				text3 = "." + text3.PadLeft(Math.Min(numericPrecision2, 7), 'f');
			}

			string text4 = $"yyyy-MM-dd HH:mm:ss{text3} zzz";
			result = StorageViewDataEntity.DateTimeOffsetValue.ToString(text4, CultureInfo.InvariantCulture);
		}
		else if (type == StorageViewDataEntity.TypeSqlDateTime)
		{
			StorageViewDataEntity.SqlDateTimeValue = (SqlDateTime)cellData;
			StorageViewDataEntity.DateTimeValue = StorageViewDataEntity.SqlDateTimeValue.Value;
			string dataTypeName2 = GetColumnInfo(iCol).DataTypeName;
			result = string.Compare(dataTypeName2, "smalldatetime", StringComparison.Ordinal) != 0 ? StorageViewDataEntity.DateTimeValue.ToString(C_DateTimeFormatString, CultureInfo.InvariantCulture) : StorageViewDataEntity.DateTimeValue.ToString(C_SmallDateTimeFormatString, CultureInfo.InvariantCulture);
		}
		else if (type == StorageViewDataEntity.TypeTimeSpan)
		{
			StorageViewDataEntity.TimeSpanValue = (TimeSpan)cellData;
			result = StorageViewDataEntity.TimeSpanValue.ToString();
			int numericPrecision3 = GetColumnInfo(iCol).Precision;
			int num2 = result.LastIndexOf('.');
			if (num2 >= 0 && num2 < 7)
			{
				num2 = -1;
			}

			if (numericPrecision3 > 0)
			{
				if (num2 == -1)
				{
					result += ".";
					num2 = result.Length - 1;
				}

				int num3 = result.Length - (num2 + 1);
				if (num3 < numericPrecision3)
				{
					result = result.PadRight(num2 + (numericPrecision3 - num3) + 1, '0');
				}
				else if (num3 > numericPrecision3)
				{
					int num4 = numericPrecision3 == 0 ? 1 : 0;
					result = result.Remove(num2 + numericPrecision3 + 1 + num4);
				}
			}
		}
		else if (type == StorageViewDataEntity.TypeBool)
		{
			StorageViewDataEntity.BoolValue = (bool)cellData;
			result = StorageViewDataEntity.BoolValue ? "1" : "0";
		}
		else if (type == StorageViewDataEntity.TypeSqlBool)
		{
			StorageViewDataEntity.SqlBoolValue = (SqlBoolean)cellData;
			result = StorageViewDataEntity.SqlBoolValue.Value ? "1" : "0";
		}
		else if (type == StorageViewDataEntity.TypeSqlGuid || type == StorageViewDataEntity.TypeGuid)
		{
			result = cellData.ToString();
			if (result != null)
			{
				result = result.ToUpper(CultureInfo.InvariantCulture);
			}
		}
		else
		{
			result = cellData.ToString();
		}

		return result;
	}

	public abstract long RowCount { get; }

	public abstract IBsColumnInfo GetColumnInfo(int iCol);

	public abstract int ColumnCount { get; }

	public abstract bool IsStorageClosed();

	public abstract void DeleteRow(long iRow);

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public virtual void Dispose(bool disposing)
	{
	}
}
