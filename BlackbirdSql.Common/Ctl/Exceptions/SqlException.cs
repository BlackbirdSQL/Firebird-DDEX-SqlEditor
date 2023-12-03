#region Assembly Microsoft.Data.SqlClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.SqlClient.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Ctl.Exceptions;


[Serializable]
public sealed class SqlException : DbException
{
	private const string C_OriginalClientConnectionIdKey = "OriginalClientConnectionId";

	private const string C_RoutingDestinationKey = "RoutingDestination";

	private const int C_UnkownExceptionHResult = -2146232060;

	private SqlErrorCollection _errors;

	[OptionalField(VersionAdded = 4)]
	private Guid _clientConnectionId = Guid.Empty;

	public bool _doNotReconnect;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public SqlErrorCollection Errors
	{
		get
		{
			_errors ??= [];

			return _errors;
		}
	}

	public Guid ClientConnectionId => _clientConnectionId;

	public byte Class
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return 0;
			}

			return Errors[0].Class;
		}
	}

	public int LineNumber
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return 0;
			}

			return Errors[0].LineNumber;
		}
	}

	public int Number
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return 0;
			}

			return Errors[0].Number;
		}
	}

	public string Procedure
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return null;
			}

			return Errors[0].Procedure;
		}
	}

	public string Server
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return null;
			}

			return Errors[0].Server;
		}
	}

	public byte State
	{
		get
		{
			if (Errors.Count <= 0)
			{
				return 0;
			}

			return Errors[0].State;
		}
	}

	public override string Source => "Framework Microsoft SqlClient Data Provider";

	private SqlException(string message, SqlErrorCollection errorCollection, Exception innerException, Guid conId)
		: base(message, innerException)
	{
		HResult = C_UnkownExceptionHResult;
		_errors = errorCollection;
		_clientConnectionId = conId;
	}

	private SqlException(SerializationInfo si, StreamingContext sc)
		: base(si, sc)
	{
		_errors = (SqlErrorCollection)si.GetValue("Errors", typeof(SqlErrorCollection));
		HResult = C_UnkownExceptionHResult; // General error/unhandled
		SerializationInfoEnumerator enumerator = si.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ("ClientConnectionId" == enumerator.Current.Name)
			{
				_clientConnectionId = (Guid)si.GetValue("ClientConnectionId", typeof(Guid));
				break;
			}
		}
	}

	public override void GetObjectData(SerializationInfo si, StreamingContext context)
	{
		base.GetObjectData(si, context);
		si.AddValue("Errors", null);
		si.AddValue("ClientConnectionId", _clientConnectionId, typeof(object));
		for (int i = 0; i < Errors.Count; i++)
		{
			string key = "SqlError " + (i + 1);
			if (Data.Contains(key))
			{
				Data.Remove(key);
			}

			Data.Add(key, Errors[i].ToString());
		}
	}

	private bool ShouldSerializeErrors()
	{
		if (_errors != null)
		{
			return 0 < _errors.Count;
		}

		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.ToString());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat(ExceptionsResources.SQL_ExClientConnectionId, _clientConnectionId);
		if (Errors.Count > 0 && Number != 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat(ExceptionsResources.SQL_ExErrorNumberStateClass, Number, State, Class);
		}

		if (Data.Contains(C_OriginalClientConnectionIdKey))
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat(ExceptionsResources.SQL_ExOriginalClientConnectionId, Data[C_OriginalClientConnectionIdKey]);
		}

		if (Data.Contains(C_RoutingDestinationKey))
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat(ExceptionsResources.SQL_ExRoutingDestination, Data[C_RoutingDestinationKey]);
		}

		return stringBuilder.ToString();
	}

	public static SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion)
	{
		return CreateException(errorCollection, serverVersion, Guid.Empty);
	}

	/*
	public static SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, SqlInternalConnectionTds internalConnection, Exception innerException = null)
	{
		Guid conId = internalConnection?._clientConnectionId ?? Guid.Empty;
		SqlException ex = CreateException(errorCollection, serverVersion, conId, innerException);
		if (internalConnection != null)
		{
			if (internalConnection.OriginalClientConnectionId != Guid.Empty && internalConnection.OriginalClientConnectionId != internalConnection.ClientConnectionId)
			{
				ex.Data.Add(C_OriginalClientConnectionIdKey, internalConnection.OriginalClientConnectionId);
			}

			if (!string.IsNullOrEmpty(internalConnection.RoutingDestination))
			{
				ex.Data.Add(C_RoutingDestinationKey, internalConnection.RoutingDestination);
			}
		}

		return ex;
	}
	*/


	public static SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, Guid conId, Exception innerException = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < errorCollection.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(Environment.NewLine);
			}

			stringBuilder.Append(errorCollection[i].Message);
		}

		if (innerException == null && errorCollection[0].Win32ErrorCode != 0 && errorCollection[0].Win32ErrorCode != -1)
		{
			innerException = new Win32Exception(errorCollection[0].Win32ErrorCode);
		}

		SqlException ex = new SqlException(stringBuilder.ToString(), errorCollection, innerException, conId);
		ex.Data.Add("HelpLink.ProdName", "Microsoft SQL Server");
		if (!string.IsNullOrEmpty(serverVersion))
		{
			ex.Data.Add("HelpLink.ProdVer", serverVersion);
		}

		ex.Data.Add("HelpLink.EvtSrc", "MSSQLServer");
		ex.Data.Add("HelpLink.EvtID", errorCollection[0].Number.ToString(CultureInfo.InvariantCulture));
		ex.Data.Add("HelpLink.BaseHelpUrl", "http://go.microsoft.com/fwlink");
		ex.Data.Add("HelpLink.LinkId", "20476");
		return ex;
	}

	public SqlException InternalClone()
	{
		SqlException ex = new SqlException(Message, _errors, InnerException, _clientConnectionId);
		if (Data != null)
		{
			foreach (DictionaryEntry datum in Data)
			{
				ex.Data.Add(datum.Key, datum.Value);
			}
		}

		ex._doNotReconnect = _doNotReconnect;
		return ex;
	}
}
