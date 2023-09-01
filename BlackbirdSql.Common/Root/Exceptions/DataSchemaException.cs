#region Assembly Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.Tools.Utilities.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.Serialization;



namespace BlackbirdSql.Common.Exceptions;


[Serializable]
public class DataSchemaException : Exception
{
	public DataSchemaException()
	{
	}

	public DataSchemaException(string message)
		: base(message)
	{
	}

	public DataSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected DataSchemaException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
