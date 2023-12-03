#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Common.Ctl;

public class ThreadSafeCollection<T>
{
	private readonly ReaderWriterLock _LockObject = new ReaderWriterLock();

	private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(5.0);

	private ObservableCollection<T> _dataList = [];

	public ReadOnlyObservableCollection<T> CloneList
	{
		get
		{
			_dataList ??= [];

			T[] newData = new T[_dataList.Count];
			new AutoLock(_LockObject, isWriteLock: false, _lockTimeout, delegate
			{
				_dataList.CopyTo(newData, 0);
			}, out var exception);
			ReadOnlyObservableCollection<T> readOnlyObservableCollection = new ReadOnlyObservableCollection<T>(new ObservableCollection<T>([.. newData]));
			if (exception != null && Trace != null)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to return data list", 75, "ThreadSafeCollection.cs", "CloneList");
			}

			readOnlyObservableCollection ??= new ReadOnlyObservableCollection<T>([]);

			return readOnlyObservableCollection;
		}
	}

	public bool IsListEmpty
	{
		get
		{
			bool isEmpty = false;
			new AutoLock(_LockObject, isWriteLock: false, _lockTimeout, delegate
			{
				isEmpty = _dataList == null || _dataList.Count == 0;
			}, out var exception);
			if (exception != null && Trace != null)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to return data list", 96, "ThreadSafeCollection.cs", "IsListEmpty");
			}

			return isEmpty;
		}
	}

	public int NumberOfConnections
	{
		get
		{
			int numberOfConnections = 0;
			new AutoLock(_LockObject, isWriteLock: false, _lockTimeout, delegate
			{
				numberOfConnections = _dataList != null ? _dataList.Count : 0;
			}, out var exception);
			if (exception != null && Trace != null)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to return number of connections", 117, "ThreadSafeCollection.cs", "NumberOfConnections");
			}

			return numberOfConnections;
		}
	}

	public Traceable Trace { get; set; }

	public void Add(T newItem)
	{
		new AutoLock(_LockObject, isWriteLock: true, _lockTimeout, delegate
		{
			_dataList.Add(newItem);
		}, out var exception);
		if (exception != null && Trace != null)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to return data list", 34, "ThreadSafeCollection.cs", "Add");
		}
	}

	public void Remove(T newItem)
	{
		new AutoLock(_LockObject, isWriteLock: true, _lockTimeout, delegate
		{
			_dataList.Remove(newItem);
		}, out var exception);
		if (exception != null && Trace != null)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to return data list", 48, "ThreadSafeCollection.cs", "Remove");
		}
	}

	public ReadOnlyObservableCollection<T> SetDataList(ObservableCollection<T> dataList)
	{
		new AutoLock(_LockObject, isWriteLock: true, _lockTimeout, delegate
		{
			_dataList = dataList;
		}, out var exception);
		if (exception != null && Trace != null)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to set data list", 133, "ThreadSafeCollection.cs", "SetDataList");
		}

		return CloneList;
	}
}
