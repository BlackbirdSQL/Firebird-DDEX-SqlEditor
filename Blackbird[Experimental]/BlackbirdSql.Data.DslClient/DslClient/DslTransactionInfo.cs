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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Data.Common;

namespace BlackbirdSql.Data.DslClient;

public sealed class DslTransactionInfo
{
	#region Properties

	public DslTransaction Transaction { get; set; }

	#endregion

	#region Methods

	public long GetTransactionSnapshotNumber()
	{
		return GetValue<long>(IscCodes.fb_info_tra_snapshot_number);
	}
	public Task<long> GetTransactionSnapshotNumberAsync(CancellationToken cancellationToken = default)
	{
		return GetValueAsync<long>(IscCodes.fb_info_tra_snapshot_number, cancellationToken);
	}

	#endregion

	#region Constructors

	public DslTransactionInfo(DslTransaction transaction = null)
	{
		Transaction = transaction;
	}

	#endregion

	#region Private Methods

	private T GetValue<T>(byte item)
	{
		DslTransaction.EnsureActive(Transaction);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};
		var info = Transaction.Transaction.GetTransactionInfo(items);
		return info.Any() ? InfoValuesHelper.ConvertValue<T>(info[0]) : default;
	}
	private async Task<T> GetValueAsync<T>(byte item, CancellationToken cancellationToken = default)
	{
		DslTransaction.EnsureActive(Transaction);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};
		var info = await Transaction.Transaction.GetTransactionInfoAsync(items, cancellationToken).ConfigureAwait(false);
		return info.Any() ? InfoValuesHelper.ConvertValue<T>(info[0]) : default;
	}

	private List<T> GetList<T>(byte item)
	{
		DslTransaction.EnsureActive(Transaction);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};

		return (Transaction.Transaction.GetTransactionInfo(items)).Select(InfoValuesHelper.ConvertValue<T>).ToList();
	}
	private async Task<List<T>> GetListAsync<T>(byte item, CancellationToken cancellationToken = default)
	{
		DslTransaction.EnsureActive(Transaction);

		var items = new byte[]
		{
			item,
			IscCodes.isc_info_end
		};

		return (await Transaction.Transaction.GetTransactionInfoAsync(items, cancellationToken).ConfigureAwait(false)).Select(InfoValuesHelper.ConvertValue<T>).ToList();
	}

	#endregion
}
