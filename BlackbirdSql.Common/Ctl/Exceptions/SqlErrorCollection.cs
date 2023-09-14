#region Assembly Microsoft.Data.SqlClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.SqlClient.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel;


namespace BlackbirdSql.Common.Ctl.Exceptions;


[Serializable]
[ListBindable(false)]
public sealed class SqlErrorCollection : ICollection, IEnumerable
{
	private readonly ArrayList errors = new ArrayList();

	public int Count => errors.Count;

	object ICollection.SyncRoot => this;

	bool ICollection.IsSynchronized => false;

	public SqlError this[int index] => (SqlError)errors[index];

	public SqlErrorCollection()
	{
	}

	public void CopyTo(Array array, int index)
	{
		errors.CopyTo(array, index);
	}

	public void CopyTo(SqlError[] array, int index)
	{
		errors.CopyTo(array, index);
	}

	public IEnumerator GetEnumerator()
	{
		return errors.GetEnumerator();
	}

	public void Add(SqlError error)
	{
		errors.Add(error);
	}
}
