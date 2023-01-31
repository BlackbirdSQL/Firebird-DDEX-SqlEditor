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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.DslClient;

[Serializable]
[ListBindable(false)]
public sealed class DslErrorCollection : ICollection<DslError>
{
	#region Fields

	private List<DslError> _errors;

	#endregion

	#region Constructors

	internal DslErrorCollection()
	{
		_errors = new List<DslError>();
	}

	#endregion

	#region Properties

	public int Count
	{
		get { return _errors.Count; }
	}

	public bool IsReadOnly
	{
		get { return true; }
	}

	#endregion

	#region Methods

	internal int IndexOf(string errorMessage)
	{
		return _errors.FindIndex(x => string.Equals(x.Message, errorMessage, StringComparison.CurrentCultureIgnoreCase));
	}

	internal DslError Add(DslError error)
	{
		_errors.Add(error);

		return error;
	}

	internal DslError Add(string errorMessage, int number)
	{
		return Add(new DslError(errorMessage, number));
	}

	void ICollection<DslError>.Add(DslError item)
	{
		Diag.Dug(true, "Not supported");
		throw new NotSupportedException();
	}

	void ICollection<DslError>.Clear()
	{
		Diag.Dug(true, "Not supported");
		throw new NotSupportedException();
	}

	public bool Contains(DslError item)
	{
		return _errors.Contains(item);
	}

	public void CopyTo(DslError[] array, int arrayIndex)
	{
		_errors.CopyTo(array, arrayIndex);
	}

	bool ICollection<DslError>.Remove(DslError item)
	{
		Diag.Dug(true, "Not supported");
		throw new NotSupportedException();
	}

	public IEnumerator<DslError> GetEnumerator()
	{
		return _errors.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}
