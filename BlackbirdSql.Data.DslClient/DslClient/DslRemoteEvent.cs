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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;



namespace BlackbirdSql.Data.DslClient;

public sealed class DslRemoteEvent : IDisposable
#if NET
		, IAsyncDisposable
#endif
{
	private DslConnectionInternal _connection;
	private RemoteEvent _revent;
	private SynchronizationContext _synchronizationContext;

	public event EventHandler<DslRemoteEventCountsEventArgs> RemoteEventCounts;
	public event EventHandler<DslRemoteEventErrorEventArgs> RemoteEventError;

	public string this[int index] => _revent != null ? _revent.Events[index] : throw new InvalidOperationException();
	public int RemoteEventId => _revent != null ? _revent.RemoteId : throw new InvalidOperationException();

	public DslRemoteEvent(string connectionString)
	{
		_connection = new DslConnectionInternal(new DbConnectionString(connectionString));
	}

	public void Open()
	{
		if (_revent != null)
			throw new InvalidOperationException($"{nameof(DslRemoteEvent)} already open.");

		_connection.Connect();
		_revent = new RemoteEvent(_connection.Database);
		_revent.EventCountsCallback = OnRemoteEventCounts;
		_revent.EventErrorCallback = OnRemoteEventError;
		_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
	}
	public async Task OpenAsync(CancellationToken cancellationToken = default)
	{
		if (_revent != null)
			throw new InvalidOperationException($"{nameof(DslRemoteEvent)} already open.");

		await _connection.ConnectAsync(cancellationToken).ConfigureAwait(false);
		_revent = new RemoteEvent(_connection.Database);
		_revent.EventCountsCallback = OnRemoteEventCounts;
		_revent.EventErrorCallback = OnRemoteEventError;
		_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
	}

	public void Dispose()
	{
		_connection.Disconnect();
	}
#if NET
	public ValueTask DisposeAsync()
	{
		return new ValueTask(_connection.DisconnectAsync(CancellationToken.None));
	}
#endif

	public void QueueEvents(ICollection<string> events)
	{
		if (_revent == null)
			throw new InvalidOperationException($"{nameof(DslRemoteEvent)} must be opened.");

		try
		{
			_revent.QueueEvents(events);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
	public async Task QueueEventsAsync(ICollection<string> events, CancellationToken cancellationToken = default)
	{
		if (_revent == null)
			throw new InvalidOperationException($"{nameof(DslRemoteEvent)} must be opened.");

		try
		{
			await _revent.QueueEventsAsync(events, cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	public void CancelEvents()
	{
		try
		{
			_revent.CancelEvents();
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
	public async Task CancelEventsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _revent.CancelEventsAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	private void OnRemoteEventCounts(string name, int count)
	{
		var args = new DslRemoteEventCountsEventArgs(name, count);
		_synchronizationContext.Post(_ =>
		{
			RemoteEventCounts?.Invoke(this, args);
		}, null);
	}

	private void OnRemoteEventError(Exception error)
	{
		var args = new DslRemoteEventErrorEventArgs(error);
		_synchronizationContext.Post(_ =>
		{
			RemoteEventError?.Invoke(this, args);
		}, null);
	}
}
