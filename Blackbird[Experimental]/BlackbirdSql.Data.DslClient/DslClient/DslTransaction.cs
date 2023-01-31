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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;
using BlackbirdSql.Data.Logging;



namespace BlackbirdSql.Data.DslClient;

public sealed class DslTransaction : DbTransaction
{
	static readonly IFbLogger Log = FbLogManager.CreateLogger(nameof(DslTransaction));

	internal const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

	#region Fields

	private DslConnection _connection;
	private TransactionBase _transaction;
	private bool _disposed;
	private bool _isCompleted;

	#endregion

	#region Properties

	public new DslConnection Connection
	{
		get { return !_isCompleted ? _connection : null; }
	}

	public override IsolationLevel IsolationLevel { get; }

	internal TransactionBase Transaction
	{
		get { return _transaction; }
	}

	internal bool IsCompleted
	{
		get { return _isCompleted; }
	}

	protected override DbConnection DbConnection
	{
		get { return _connection; }
	}

#if NET
	public override bool SupportsSavepoints
	{
		get { return true; }
	}
#endif

	#endregion

	#region Constructors

	internal DslTransaction(DslConnection connection)
		: this(connection, IsolationLevel.ReadCommitted)
	{ }

	internal DslTransaction(DslConnection connection, IsolationLevel il)
	{
		_connection = connection;
		IsolationLevel = il;
	}

	#endregion

	#region IDisposable, IAsyncDisposable methods

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_transaction != null)
			{
				if (!_isCompleted)
				{
					try
					{
						_transaction.Dispose2();
					}
					catch (IscException ex)
					{
						Diag.Dug(ex);
						throw DslException.Create(ex);
					}
				}
			}
			_connection = null;
			_transaction = null;
			_isCompleted = true;
		}
		base.Dispose(disposing);
	}
#if NET
	public override async ValueTask DisposeAsync()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_transaction != null)
			{
				if (!_isCompleted)
				{
					try
					{
						await _transaction.Dispose2Async(CancellationToken.None).ConfigureAwait(false);
					}
					catch (IscException ex)
					{
						Diag.Dug(ex);
	throw DslException.Create(ex);
					}
				}
			}
			_connection = null;
			_transaction = null;
			_isCompleted = true;
		}
		await base.DisposeAsync().ConfigureAwait(false);
	}
#endif

	#endregion

	#region Methods

	public override void Commit()
	{
		LogMessages.TransactionCommitting(Log, this);

		EnsureCompleted();
		try
		{
			_transaction.Commit();
			CompleteTransaction();
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionCommitted(Log, this);
	}
#if NETFRAMEWORK
	public async Task CommitAsync(CancellationToken cancellationToken = default)
#else
	public override async Task CommitAsync(CancellationToken cancellationToken = default)
#endif
	{
		LogMessages.TransactionCommitting(Log, this);

		EnsureCompleted();
		try
		{
			await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			await CompleteTransactionAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionCommitted(Log, this);
	}

	public override void Rollback()
	{
		LogMessages.TransactionRollingBack(Log, this);

		EnsureCompleted();
		try
		{
			_transaction.Rollback();
			CompleteTransaction();
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBack(Log, this);
	}
#if NETFRAMEWORK
	public async Task RollbackAsync(CancellationToken cancellationToken = default)
#else
	public override async Task RollbackAsync(CancellationToken cancellationToken = default)
#endif
	{
		LogMessages.TransactionRollingBack(Log, this);

		EnsureCompleted();
		try
		{
			await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
			await CompleteTransactionAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBack(Log, this);
	}

#if NETFRAMEWORK
	public void Save(string savePointName)
#else
	public override void Save(string savePointName)
#endif
	{
		LogMessages.TransactionSaving(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			using (command)
#endif
			{
				command.ExecuteNonQuery();
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionSaved(Log, this);
	}
#if NETFRAMEWORK
	public async Task SaveAsync(string savePointName, CancellationToken cancellationToken = default)
#else
	public override async Task SaveAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
	{
		LogMessages.TransactionSaving(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			await using (command.ConfigureAwait(false))
#endif
			{
				await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionSaved(Log, this);
	}

#if NETFRAMEWORK
	public void Release(string savePointName)
#else
	public override void Release(string savePointName)
#endif
	{
		LogMessages.TransactionReleasingSavepoint(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"RELEASE SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			using (command)
#endif
			{
				command.ExecuteNonQuery();
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionReleasedSavepoint(Log, this);
	}
#if NETFRAMEWORK
	public async Task ReleaseAsync(string savePointName, CancellationToken cancellationToken = default)
#else
	public override async Task ReleaseAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
	{
		LogMessages.TransactionReleasingSavepoint(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"RELEASE SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			await using (command.ConfigureAwait(false))
#endif
			{
				await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionReleasedSavepoint(Log, this);
	}

#if NETFRAMEWORK
	public void Rollback(string savePointName)
#else
	public override void Rollback(string savePointName)
#endif
	{
		LogMessages.TransactionRollingBackSavepoint(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"ROLLBACK WORK TO SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			using (command)
#endif
			{
				command.ExecuteNonQuery();
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBackSavepoint(Log, this);
	}
#if NETFRAMEWORK
	public async Task RollbackAsync(string savePointName, CancellationToken cancellationToken = default)
#else
	public override async Task RollbackAsync(string savePointName, CancellationToken cancellationToken = default)
#endif
	{
		LogMessages.TransactionRollingBackSavepoint(Log, this);

		EnsureSavePointName(savePointName);
		EnsureCompleted();
		try
		{
			var command = new DslCommand($"ROLLBACK WORK TO SAVEPOINT {savePointName}", _connection, this);
#if NETFRAMEWORK
			using (command)
#else
			await using (command.ConfigureAwait(false))
#endif
			{
				await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBackSavepoint(Log, this);
	}

	public void CommitRetaining()
	{
		LogMessages.TransactionCommittingRetaining(Log, this);

		EnsureCompleted();
		try
		{
			_transaction.CommitRetaining();
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionCommittedRetaining(Log, this);
	}
	public async Task CommitRetainingAsync(CancellationToken cancellationToken = default)
	{
		LogMessages.TransactionCommittingRetaining(Log, this);

		EnsureCompleted();
		try
		{
			await _transaction.CommitRetainingAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionCommittedRetaining(Log, this);
	}

	public void RollbackRetaining()
	{
		LogMessages.TransactionRollingBackRetaining(Log, this);

		EnsureCompleted();
		try
		{
			_transaction.RollbackRetaining();
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBackRetaining(Log, this);
	}
	public async Task RollbackRetainingAsync(CancellationToken cancellationToken = default)
	{
		LogMessages.TransactionRollingBackRetaining(Log, this);

		EnsureCompleted();
		try
		{
			await _transaction.RollbackRetainingAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		LogMessages.TransactionRolledBackRetaining(Log, this);
	}

	#endregion

	#region Internal Methods

	internal void BeginTransaction()
	{
		LogMessages.TransactionBeginning(Log, this);

		_transaction = _connection.InnerConnection.Database.BeginTransaction(BuildTpb());

		LogMessages.TransactionBegan(Log, this);
	}
	internal async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		LogMessages.TransactionBeginning(Log, this);

		_transaction = await _connection.InnerConnection.Database.BeginTransactionAsync(BuildTpb(), cancellationToken).ConfigureAwait(false);

		LogMessages.TransactionBegan(Log, this);
	}

	internal void BeginTransaction(DslTransactionOptions options)
	{
		LogMessages.TransactionBeginning(Log, this);

		_transaction = _connection.InnerConnection.Database.BeginTransaction(BuildTpb(options));

		LogMessages.TransactionBegan(Log, this);
	}
	internal async Task BeginTransactionAsync(DslTransactionOptions options, CancellationToken cancellationToken = default)
	{
		LogMessages.TransactionBeginning(Log, this);

		_transaction = await _connection.InnerConnection.Database.BeginTransactionAsync(BuildTpb(options), cancellationToken).ConfigureAwait(false);

		LogMessages.TransactionBegan(Log, this);
	}

	internal static void EnsureActive(DslTransaction transaction)
	{
		if (transaction == null || transaction.IsCompleted)
			throw new InvalidOperationException("Transaction must be valid and active.");
	}

	#endregion

	#region Private Methods

	private void CompleteTransaction()
	{
		var innerConnection = _connection?.InnerConnection;
		if (innerConnection != null)
		{
			innerConnection.TransactionCompleted();
		}
		_connection = null;
		_transaction.Dispose2();
		_transaction = null;
		_isCompleted = true;
	}
	private async Task CompleteTransactionAsync(CancellationToken cancellationToken = default)
	{
		var innerConnection = _connection?.InnerConnection;
		if (innerConnection != null)
		{
			await innerConnection.TransactionCompletedAsync(cancellationToken).ConfigureAwait(false);
		}
		_connection = null;
		await _transaction.Dispose2Async(cancellationToken).ConfigureAwait(false);
		_transaction = null;
		_isCompleted = true;
	}

	private TransactionParameterBuffer BuildTpb()
	{
		var options = new DslTransactionOptions();
		options.WaitTimeout = null;
		options.TransactionBehavior = DslTransactionBehavior.Write;

		options.TransactionBehavior |= DslTransactionBehavior.NoWait;

		switch (IsolationLevel)
		{
			case IsolationLevel.Serializable:
				options.TransactionBehavior |= DslTransactionBehavior.Consistency;
				break;

			case IsolationLevel.RepeatableRead:
			case IsolationLevel.Snapshot:
				options.TransactionBehavior |= DslTransactionBehavior.Concurrency;
				break;

			case IsolationLevel.ReadCommitted:
			case IsolationLevel.ReadUncommitted:
			default:
				options.TransactionBehavior |= DslTransactionBehavior.ReadCommitted;
				options.TransactionBehavior |= DslTransactionBehavior.RecVersion;
				break;
		}

		return BuildTpb(options);
	}

	private void EnsureCompleted()
	{
		if (_isCompleted)
		{
			Diag.Dug(true, "This transaction has completed and it is no longer usable.");
			throw new InvalidOperationException("This transaction has completed and it is no longer usable.");
		}
	}

	private TransactionParameterBuffer BuildTpb(DslTransactionOptions options)
	{
		var tpb = Connection.InnerConnection.Database.CreateTransactionParameterBuffer();

		tpb.Append(IscCodes.isc_tpb_version3);

		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Consistency))
		{
			tpb.Append(IscCodes.isc_tpb_consistency);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Concurrency))
		{
			tpb.Append(IscCodes.isc_tpb_concurrency);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Wait))
		{
			tpb.Append(IscCodes.isc_tpb_wait);
			if (options.WaitTimeoutTPBValue != null)
			{
				tpb.Append(IscCodes.isc_tpb_lock_timeout, (short)options.WaitTimeoutTPBValue);
			}
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.NoWait))
		{
			tpb.Append(IscCodes.isc_tpb_nowait);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Read))
		{
			tpb.Append(IscCodes.isc_tpb_read);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Write))
		{
			tpb.Append(IscCodes.isc_tpb_write);
		}
		foreach (var table in options.LockTables)
		{
			int lockType;
			if (table.Value.HasFlag(DslTransactionBehavior.LockRead))
			{
				lockType = IscCodes.isc_tpb_lock_read;
			}
			else if (table.Value.HasFlag(DslTransactionBehavior.LockWrite))
			{
				lockType = IscCodes.isc_tpb_lock_write;
			}
			else
			{
				Diag.Dug(true, "Must specify either LockRead or LockWrite.");
				throw new ArgumentException("Must specify either LockRead or LockWrite.");
			}
			tpb.Append(lockType, table.Key);

			int? lockBehavior = null;
			if (table.Value.HasFlag(DslTransactionBehavior.Exclusive))
			{
				lockBehavior = IscCodes.isc_tpb_exclusive;
			}
			else if (table.Value.HasFlag(DslTransactionBehavior.Protected))
			{
				lockBehavior = IscCodes.isc_tpb_protected;
			}
			else if (table.Value.HasFlag(DslTransactionBehavior.Shared))
			{
				lockBehavior = IscCodes.isc_tpb_shared;
			}
			if (lockBehavior != null)
			{
				tpb.Append((int)lockBehavior);
			}
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.ReadCommitted))
		{
			tpb.Append(IscCodes.isc_tpb_read_committed);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.Autocommit))
		{
			tpb.Append(IscCodes.isc_tpb_autocommit);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.RecVersion))
		{
			tpb.Append(IscCodes.isc_tpb_rec_version);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.NoRecVersion))
		{
			tpb.Append(IscCodes.isc_tpb_no_rec_version);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.RestartRequests))
		{
			tpb.Append(IscCodes.isc_tpb_restart_requests);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.NoAutoUndo))
		{
			tpb.Append(IscCodes.isc_tpb_no_auto_undo);
		}
		if (options.TransactionBehavior.HasFlag(DslTransactionBehavior.ReadConsistency))
		{
			tpb.Append(IscCodes.isc_tpb_read_consistency);
		}

		if (options.SnapshotAtNumber != null)
		{
			tpb.Append(IscCodes.isc_tpb_at_snapshot_number, (long)options.SnapshotAtNumber);
		}

		return tpb;
	}

	private static void EnsureSavePointName(string savePointName)
	{
		if (string.IsNullOrWhiteSpace(savePointName))
		{
			Diag.Dug(true, "No transaction name was specified.");
			throw new ArgumentException("No transaction name was specified.");
		}
	}

	#endregion
}
