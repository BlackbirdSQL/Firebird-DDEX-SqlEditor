#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;



namespace BlackbirdSql.Shared.Events;


public delegate Task<bool> MoreRowsAvailableEventHandler(object sender, MoreRowsAvailableEventArgs a);


public class MoreRowsAvailableEventArgs : EventArgs
{
	public MoreRowsAvailableEventArgs()
	{
	}

	public MoreRowsAvailableEventArgs(CancellationToken cancelToken)
	{
		_CancelToken = cancelToken;
	}

	private CancellationToken _CancelToken;

	private bool _allRows;

	private long _newRowsNum;

	public bool AllRows => _allRows;

	public CancellationToken CancelToken => _CancelToken;

	public long NewRowsNumber => _newRowsNum;

	public void SetEventInfo(bool allRows, long newRows, CancellationToken cancelToken)
	{
		_allRows = allRows;
		_newRowsNum = newRows;
		_CancelToken = cancelToken;
	}
}
