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

//$OriginalAuthors = Jiri Cincura (jiri@cincura.net)

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlackbirdSql.Data.DslClient;

[ListBindable(false)]
public sealed class DslBatchParameterCollection : IList<DslParameterCollection>
{
	readonly List<DslParameterCollection> _inner;

	internal DslBatchParameterCollection()
	{
		_inner = new List<DslParameterCollection>();
	}

	public DslParameterCollection this[int index]
	{
		get => _inner[index];
		set => _inner[index] = value;
	}

	public int Count => _inner.Count;

	public bool IsReadOnly => false;

	public void Add(DslParameterCollection item) => _inner.Add(item);

	public void Clear() => _inner.Clear();

	public bool Contains(DslParameterCollection item) => _inner.Contains(item);

	public void CopyTo(DslParameterCollection[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

	public IEnumerator<DslParameterCollection> GetEnumerator() => _inner.GetEnumerator();

	public int IndexOf(DslParameterCollection item) => _inner.IndexOf(item);

	public void Insert(int index, DslParameterCollection item) => _inner.Insert(index, item);

	public bool Remove(DslParameterCollection item) => _inner.Remove(item);

	public void RemoveAt(int index) => _inner.RemoveAt(index);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
