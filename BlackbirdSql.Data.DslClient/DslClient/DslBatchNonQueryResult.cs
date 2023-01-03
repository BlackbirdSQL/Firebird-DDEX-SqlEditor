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
using System.Linq;

using BlackbirdSql.Common;
using static BlackbirdSql.Data.Common.BatchBase;
using static BlackbirdSql.Data.DslClient.DslBatchNonQueryResult;



namespace BlackbirdSql.Data.DslClient;

public sealed class DslBatchNonQueryResult : IEnumerable<FbBatchNonQueryResultItem>
{
	public sealed class FbBatchNonQueryResultItem
	{
		public int RecordsAffected { get; internal set; }
		public bool IsSuccess { get; internal set; }
		public DslException Exception { get; internal set; }
	}

	readonly List<FbBatchNonQueryResultItem> _items;

	public bool AllSuccess => _items.TrueForAll(x => x.IsSuccess);
	public int Count => _items.Count;

	internal DslBatchNonQueryResult(ExecuteResultItem[] result)
	{
		_items = result.Select(x => new FbBatchNonQueryResultItem()
		{
			RecordsAffected = x.RecordsAffected,
			IsSuccess = !x.IsError,
			Exception = x.Exception != null ? (DslException)DslException.Create(x.Exception) : null,
		}).ToList();
	}

	public FbBatchNonQueryResultItem this[int index] => _items[index];

	public void EnsureSuccess()
	{
		var indexes = _items.Select((e, i) => new { Element = e, Index = i }).Where(x => !x.Element.IsSuccess).Select(x => x.Index).ToList();
		if (indexes.Count == 0)
			return;
		Diag.Dug(true, $"Indexes {string.Join(", ", indexes)} failed in batch.");
		throw DslException.Create($"Indexes {string.Join(", ", indexes)} failed in batch.");
	}

	public IEnumerator<FbBatchNonQueryResultItem> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
