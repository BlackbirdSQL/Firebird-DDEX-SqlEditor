/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;

using BlackbirdSql.Data.Common;

namespace BlackbirdSql.Data.DslClient;

[Serializable]
public sealed class DslException : DbException
{
	#region Fields

	private DslErrorCollection _errors;

	#endregion

	#region Properties

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public DslErrorCollection Errors => _errors ??= new DslErrorCollection();

	public override int ErrorCode => (InnerException as IscException)?.ErrorCode ?? 0;

	public string SQLSTATE => (InnerException as IscException)?.SQLSTATE;

	#endregion

	#region Constructors

	private DslException(string message, Exception innerException)
		: base(message, innerException)
	{ }

	private DslException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_errors = (DslErrorCollection)info.GetValue("errors", typeof(DslErrorCollection));
	}

	#endregion

	#region Methods

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);

		info.AddValue("errors", _errors);
	}

	#endregion

	#region Private Methods

	private void ProcessIscExceptionErrors(IscException innerException)
	{
		foreach (var error in innerException.Errors)
		{
			Errors.Add(error.Message, error.ErrorCode);
		}
	}

	#endregion

	internal static Exception Create(string message) => Create(message, null);
	internal static Exception Create(Exception innerException) => Create(null, innerException);
	internal static Exception Create(string message, Exception innerException)
	{
		message ??= innerException?.Message;
		if (innerException is IscException iscException)
		{
			if (iscException.ErrorCode == IscCodes.isc_cancelled)
			{
				return new OperationCanceledException(message, innerException);
			}
			else
			{
				var result = new DslException(message, innerException);
				result.ProcessIscExceptionErrors(iscException);
				return result;
			}
		}
		else
		{
			return new DslException(message, innerException);
		}
	}
}
