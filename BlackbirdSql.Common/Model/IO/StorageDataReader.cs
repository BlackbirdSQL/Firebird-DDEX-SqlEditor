#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Common.Model.IO;

public class StorageDataReader
{
	public delegate bool QueryKeepStoringData();

	private class StringWriterWithMaxCapacity(IFormatProvider formatProvider)
		: StringWriter(formatProvider)
	{
		public StringWriterWithMaxCapacity(IFormatProvider formatProvider, int capacity,
				QueryKeepStoringData _KeepStoringDataDelegate)
			: this(formatProvider)
		{
			_MaxCapacity = capacity;
			this._KeepStoringDataDelegate = _KeepStoringDataDelegate;
		}


		private readonly int _MaxCapacity = int.MaxValue;

		private bool _StopWriting;

		private readonly QueryKeepStoringData _KeepStoringDataDelegate;

		private int CurrentLength => base.GetStringBuilder().Length;

		public int MaximumCapacity => _MaxCapacity;



		public override void Write(char value)
		{
			if (_KeepStoringDataDelegate != null && !_KeepStoringDataDelegate())
			{
				StorageAbortedException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			if (!_StopWriting)
			{
				if (CurrentLength < _MaxCapacity)
				{
					base.Write(value);
				}
				else
				{
					_StopWriting = true;
				}
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (_KeepStoringDataDelegate != null && !_KeepStoringDataDelegate())
			{
				StorageAbortedException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			if (_StopWriting)
			{
				return;
			}

			int currentLength = CurrentLength;
			if (currentLength + (count - index) > _MaxCapacity)
			{
				_StopWriting = true;
				count = _MaxCapacity - currentLength + index;
				if (count < 0)
				{
					count = 0;
				}
			}

			base.Write(buffer, index, count);
		}

		public override void Write(string value)
		{
			if (_KeepStoringDataDelegate != null && !_KeepStoringDataDelegate())
			{
				StorageAbortedException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			if (!_StopWriting)
			{
				int currentLength = CurrentLength;
				if (value.Length + currentLength > _MaxCapacity)
				{
					_StopWriting = true;
					base.Write(value[..(_MaxCapacity - currentLength)]);
				}
				else
				{
					base.Write(value);
				}
			}
		}
	}

	[Serializable]
	[ComVisible(false)]
	public class StorageAbortedException : Exception
	{
		public StorageAbortedException()
		{
		}

		public StorageAbortedException(string message)
			: base(message)
		{
		}

		public StorageAbortedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected StorageAbortedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}




	private readonly IDataReader dataReader;
	private readonly DbDataReader _DbDataReader;
	private readonly FbDataReader _SqlReader;
	// private readonly MethodInfo getSqlXmlMethod;




	public int FieldCount
	{
		get
		{
			if (_DbDataReader != null)
			{
				return _DbDataReader.VisibleFieldCount;
			}

			return dataReader.FieldCount;
		}
	}

	/*
	private bool SupportSqlXml
	{
		get
		{
			if (_SqlReader == null)
			{
				return getSqlXmlMethod != null;
			}

			return true;
		}
	}
	*/

	public StorageDataReader(IDataReader reader)
	{
		dataReader = reader;
		_DbDataReader = reader as DbDataReader;
		if (_DbDataReader != null)
		{
			_SqlReader = reader as FbDataReader;
			if (_SqlReader == null)
			{
				_DbDataReader = null;
			}
		}

		if (_DbDataReader == null)
		{
			ArgumentException ex = new("Invalid IDataReader", "reader");
			Diag.Dug(ex);
			throw ex;
		}

	}

	public string GetName(int i)
	{
		return dataReader.GetName(i);
	}

	public string GetDataTypeName(int i)
	{
		return dataReader.GetDataTypeName(i);
	}

	public string GetProviderSpecificDataTypeName(int i)
	{
		return GetFieldType(i).ToString();
	}

	public Type GetFieldType(int i)
	{
		if (_DbDataReader != null)
		{
			return _DbDataReader.GetProviderSpecificFieldType(i);
		}

		return dataReader.GetFieldType(i);
	}

	public bool Read()
	{
		return dataReader.Read();
	}

	public void GetValues(object[] values)
	{
		if (_SqlReader != null)
		{
			_SqlReader.GetValues(values);
			// _SqlReader.GetSqlValues(values, _Statistics);
		}
		else
		{
			dataReader.GetValues(values);
		}
	}

	public object GetValue(int i)
	{
		_SqlReader?.GetValue(i);

		return dataReader.GetValue(i);
	}

	public bool IsDBNull(int i)
	{
		if (_DbDataReader != null)
		{
			return _DbDataReader.IsDBNull(i);
		}

		return dataReader.IsDBNull(i);
	}

	public DataTable GetSchemaTable()
	{
		if (_DbDataReader != null)
		{
			return _DbDataReader.GetSchemaTable();
		}

		return dataReader.GetSchemaTable();
	}

	public byte[] GetBytesWithMaxCapacity(int iCol, int maxNumBytesToReturn)
	{
		if (maxNumBytesToReturn <= 0)
		{
			ArgumentException ex = new("maxNumBytesToReturn");
			Diag.Dug(ex);
			throw ex;
		}

		long num;
		long num2 = num = GetBytes(iCol, 0L, null, 0, 0);
		if (num == -1 || num > maxNumBytesToReturn)
		{
			num = maxNumBytesToReturn;
		}

		byte[] array = new byte[num];
		GetBytes(iCol, 0L, array, 0, (int)num);
		if (num2 == -1 || num2 > num)
		{
			long num3 = num;
			byte[] buffer;
			for (buffer = new byte[100000]; GetBytes(iCol, num3, buffer, 0, 100000) == 100000; num3 += 100000)
			{
			}
		}

		return array;
	}

	public string GetCharsWithMaxCapacity(int iCol, int maxCharsToReturn)
	{
		if (maxCharsToReturn <= 0)
		{
			ArgumentException ex = new("maxNumBytesToReturn");
			Diag.Dug(ex);
			throw ex;
		}

		long num;
		long num2 = num = GetChars(iCol, 0L, null, 0, 0);
		if (num == -1 || num > maxCharsToReturn)
		{
			num = maxCharsToReturn;
		}

		char[] array = new char[num];
		if (num > 0)
		{
			GetChars(iCol, 0L, array, 0, (int)num);
		}

		if (num2 == -1 || num2 > num)
		{
			long num3 = num;
			char[] buffer;
			for (buffer = new char[100000]; GetChars(iCol, num3, buffer, 0, 100000) == 100000; num3 += 100000)
			{
			}
		}

		string result = new string(array);

		// Tracer.Trace(GetType(), "GetCharsWithMaxCapacity()", "Col: {0}[{1}], maxReturnBytes: {2}, result: {3}.", GetName(iCol), iCol, maxCharsToReturn, result);

		return result;
	}

	public string GetXmlWithMaxCapacity(int iCol, int maxCharsToReturn)
	{
		return GetXmlWithMaxCapacity(iCol, maxCharsToReturn, null);
	}

	public string GetXmlWithMaxCapacity(int iCol, int maxCharsToReturn, QueryKeepStoringData _KeepStoringDataDelegate)
	{
		NotImplementedException ex = new("GetXmlWithMaxCapacity()");
		Diag.Dug(ex);
		throw ex;
		/*
		if (SupportSqlXml)
		{
			SqlXml sqlXml = GetSqlXml(iCol);
			if (sqlXml == SqlXml.Null)
			{
				return null;
			}

			StringWriterWithMaxCapacity stringWriterWithMaxCapacity = new StringWriterWithMaxCapacity(null, maxCharsToReturn, _KeepStoringDataDelegate);
			XmlWriterSettings xmlWriterSettings = new()
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Fragment
			};
			XmlWriter xmlWriter = XmlWriter.Create(stringWriterWithMaxCapacity, xmlWriterSettings);
			XmlReader xmlReader = sqlXml.CreateReader();
			xmlReader.Read();
			try
			{
				while (!xmlReader.EOF)
				{
					xmlWriter.WriteNode(xmlReader, defattr: true);
				}
			}
			catch (StorageAbortedException)
			{
			}

			xmlWriter.Flush();
			return stringWriterWithMaxCapacity.ToString();
		}

		return GetValue(iCol)?.ToString();
		*/
	}

	private long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		if (_DbDataReader != null)
		{
			return _DbDataReader.GetBytes(i, dataIndex, buffer, bufferIndex, length);
		}

		return dataReader.GetBytes(i, dataIndex, buffer, bufferIndex, length);
	}

	private long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		if (_DbDataReader != null)
		{
			return _DbDataReader.GetChars(i, dataIndex, buffer, bufferIndex, length);
		}

		return dataReader.GetChars(i, dataIndex, buffer, bufferIndex, length);
	}


	/*
	public object GetSerializedWithMaxCapacity(int i, int maxReturnBytes)
	{
		if (_DataStorage == null)
			return GetBytesWithMaxCapacity(i, maxReturnBytes);


		object result;
		IBColumnInfo columnInfo = _DataStorage.GetColumnInfo(i);

		if (!columnInfo.IsBlobField)
		{
			result = GetValue(i);
		}
		else if (columnInfo.IsBytesField)
		{
			result = GetBytesWithMaxCapacity(i, maxReturnBytes);
		}
		else if (columnInfo.IsCharsField)
		{
			result = GetCharsWithMaxCapacity(i, maxReturnBytes);
		}
		else
		{
			result = GetValue(i);
		}

		// Tracer.Trace(GetType(), "GetSerializedWithMaxCapacity()", "Col: {0}[{1}], maxReturnBytes: {2}, result: {3}.", GetName(i), i, maxReturnBytes, result);


		return result;
	}
	*/


	/*
	private SqlXml GetSqlXml(int i)
	{
		if (_SqlReader != null)
		{
			return _SqlReader.GetSqlXml(i);
		}

		if (getSqlXmlMethod != null)
		{
			return (SqlXml)getSqlXmlMethod.Invoke(dataReader, new object[1] { i });
		}

		InvalidOperationException ex = new();
		Diag.Dug(ex);
		throw ex;
	}
	*/

}
