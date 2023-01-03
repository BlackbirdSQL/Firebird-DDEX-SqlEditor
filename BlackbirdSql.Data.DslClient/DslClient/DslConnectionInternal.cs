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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Client;
using BlackbirdSql.Data.Common;
using BlackbirdSql.Data.Schema;



namespace BlackbirdSql.Data.DslClient;

internal class DslConnectionInternal
{
	#region Fields

	private DatabaseBase _db;
	private DslTransaction _activeTransaction;
	private readonly HashSet<IDslPreparedCommand> _preparedCommands;
	private DbConnectionString _connectionStringOptions;
	private DslConnection _owningConnection;
	private DslEnlistmentNotification _enlistmentNotification;

	#endregion

	#region Properties

	public DatabaseBase Database
	{
		get { return _db; }
	}

	public bool HasActiveTransaction
	{
		get
		{
			return _activeTransaction != null && !_activeTransaction.IsCompleted;
		}
	}

	public DslTransaction ActiveTransaction
	{
		get { return _activeTransaction; }
	}

	public DslConnection OwningConnection
	{
		get { return _owningConnection; }
	}

	public bool IsEnlisted
	{
		get
		{
			return _enlistmentNotification != null && !_enlistmentNotification.IsCompleted;
		}
	}

	public DbConnectionString ConnectionStringOptions
	{
		get { return _connectionStringOptions; }
	}

	public bool CancelDisabled { get; private set; }

	#endregion

	#region Constructors

	public DslConnectionInternal(DbConnectionString options)
	{
		_preparedCommands = new HashSet<IDslPreparedCommand>();

		_connectionStringOptions = options;
	}

	#endregion

	#region Create and Drop database methods

	public void CreateDatabase(int pageSize, bool forcedWrites, bool overwrite)
	{
		Diag.Dug();

		var db = ClientFactory.CreateDatabase(_connectionStringOptions);

		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)_connectionStringOptions.Dialect, 0, 0, 0 });
		if (!string.IsNullOrEmpty(_connectionStringOptions.UserID))
		{
			dpb.Append(IscCodes.isc_dpb_user_name, _connectionStringOptions.UserID);
		}
		if (_connectionStringOptions.Charset.Length > 0)
		{
			if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
			{
				Diag.Dug(true, "Invalid character set specified");
				throw new ArgumentException("Invalid character set specified.");
			}
			dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
		}
		dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
		dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
		if (pageSize > 0)
		{
			if (!SizeHelper.IsValidPageSize(pageSize))
			{
				Diag.Dug(true, "Invalid page size");
				throw SizeHelper.InvalidSizeException("page size");
			}
			dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
		}

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				db.CreateDatabaseWithTrustedAuth(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				db.CreateDatabase(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
		}
		finally
		{
			db.Detach();
		}
	}
	public async Task CreateDatabaseAsync(int pageSize, bool forcedWrites, bool overwrite, CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		var db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);

		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)_connectionStringOptions.Dialect, 0, 0, 0 });
		if (!string.IsNullOrEmpty(_connectionStringOptions.UserID))
		{
			dpb.Append(IscCodes.isc_dpb_user_name, _connectionStringOptions.UserID);
		}
		if (_connectionStringOptions.Charset.Length > 0)
		{
			if (!Charset.TryGetByName(_connectionStringOptions.Charset, out var charset))
			{
				Diag.Dug(true, "Invalid character set specified");
				throw new ArgumentException("Invalid character set specified.");
			}
			dpb.Append(IscCodes.isc_dpb_set_db_charset, charset.Name);
		}
		dpb.Append(IscCodes.isc_dpb_force_write, (short)(forcedWrites ? 1 : 0));
		dpb.Append(IscCodes.isc_dpb_overwrite, (overwrite ? 1 : 0));
		if (pageSize > 0)
		{
			if (!SizeHelper.IsValidPageSize(pageSize))
			{
				Diag.Dug(true, "Invalid page size");
				throw SizeHelper.InvalidSizeException("page size");
			}
			dpb.Append(IscCodes.isc_dpb_page_size, pageSize);
		}

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await db.CreateDatabaseWithTrustedAuthAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await db.CreateDatabaseAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
		}
		finally
		{
			await db.DetachAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public void DropDatabase()
	{
		Diag.Dug();

		var db = ClientFactory.CreateDatabase(_connectionStringOptions);

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				db.AttachWithTrustedAuth(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				db.Attach(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			db.DropDatabase();
		}
		finally
		{
			db.Detach();
		}
	}
	public async Task DropDatabaseAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		var db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);

		try
		{
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await db.AttachWithTrustedAuthAsync(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await db.AttachAsync(BuildDpb(db, _connectionStringOptions), _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			await db.DropDatabaseAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await db.DetachAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Connect and Disconnect methods

	public void Connect()
	{
		Diag.Dug();

		if (!Charset.TryGetByName(_connectionStringOptions.Charset, out _))
		{
			Diag.Dug(true, "Invalid character set specified");
			throw new ArgumentException("Invalid character set specified.");
		}

		try
		{
			_db = ClientFactory.CreateDatabase(_connectionStringOptions);
			var dpb = BuildDpb(_db, _connectionStringOptions);
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				_db.AttachWithTrustedAuth(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
			else
			{
				_db.Attach(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(true, ex, "[Check that your password is not ambiguous between Firebird versions (eg. 'masterke' and 'masterkey') Credentials(" + _connectionStringOptions.UserID + ":" + _connectionStringOptions.Password + ")]");
			throw DslException.Create(ex);
		}
	}
	public async Task ConnectAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		if (!Charset.TryGetByName(_connectionStringOptions.Charset, out _))
		{
			Diag.Dug(true, "Invalid character set specified");
			throw new ArgumentException("Invalid character set specified.");
		}

		try
		{
			_db = await ClientFactory.CreateDatabaseAsync(_connectionStringOptions, cancellationToken).ConfigureAwait(false);
			var dpb = BuildDpb(_db, _connectionStringOptions);
			if (string.IsNullOrEmpty(_connectionStringOptions.UserID) && string.IsNullOrEmpty(_connectionStringOptions.Password))
			{
				await _db.AttachWithTrustedAuthAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await _db.AttachAsync(dpb, _connectionStringOptions.Database, _connectionStringOptions.CryptKey, cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	public void Disconnect()
	{
		Diag.Dug();

		if (_db != null)
		{
			try
			{
				_db.Detach();
			}
			catch
			{ }
			finally
			{
				_db = null;
				_owningConnection = null;
				_connectionStringOptions = null;
			}
		}
	}
	public async Task DisconnectAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		if (_db != null)
		{
			try
			{
				await _db.DetachAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{ }
			finally
			{
				_db = null;
				_owningConnection = null;
				_connectionStringOptions = null;
			}
		}
	}

	#endregion

	#region Transaction Handling Methods

	public DslTransaction BeginTransaction(IsolationLevel level, string transactionName)
	{
		Diag.Dug();

		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new DslTransaction(_owningConnection, level);
			_activeTransaction.BeginTransaction();

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			DisposeTransaction();
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		return _activeTransaction;
	}
	public async Task<DslTransaction> BeginTransactionAsync(IsolationLevel level, string transactionName, CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new DslTransaction(_owningConnection, level);
			await _activeTransaction.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			await DisposeTransactionAsync(cancellationToken).ConfigureAwait(false);
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		return _activeTransaction;
	}

	public DslTransaction BeginTransaction(DslTransactionOptions options, string transactionName)
	{
		Diag.Dug();

		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new DslTransaction(_owningConnection, IsolationLevel.Unspecified);
			_activeTransaction.BeginTransaction(options);

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			DisposeTransaction();
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		return _activeTransaction;
	}
	public async Task<DslTransaction> BeginTransactionAsync(DslTransactionOptions options, string transactionName, CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		EnsureNoActiveTransaction();

		try
		{
			_activeTransaction = new DslTransaction(_owningConnection, IsolationLevel.Unspecified);
			await _activeTransaction.BeginTransactionAsync(options, cancellationToken).ConfigureAwait(false);

			if (transactionName != null)
			{
				_activeTransaction.Save(transactionName);
			}
		}
		catch (IscException ex)
		{
			await DisposeTransactionAsync(cancellationToken).ConfigureAwait(false);
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}

		return _activeTransaction;
	}

	public void DisposeTransaction()
	{
		Diag.Dug();

		if (_activeTransaction != null && !IsEnlisted)
		{
			_activeTransaction.Dispose();
			_activeTransaction = null;
		}
	}
	public async Task DisposeTransactionAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		if (_activeTransaction != null && !IsEnlisted)
		{
#if NETFRAMEWORK
			_activeTransaction.Dispose();
			await Task.CompletedTask.ConfigureAwait(false);
#else
			await _activeTransaction.DisposeAsync().ConfigureAwait(false);
#endif
			_activeTransaction = null;
		}
	}

	public void TransactionCompleted()
	{
		Diag.Dug();

		foreach (var command in _preparedCommands)
		{
			command.TransactionCompleted();
		}
	}
	public async Task TransactionCompletedAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		foreach (var command in _preparedCommands)
		{
			await command.TransactionCompletedAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	#endregion

	#region Transaction Enlistment

	public void EnlistTransaction(System.Transactions.Transaction transaction)
	{
		Diag.Dug();

		if (_owningConnection != null)
		{
			if (_enlistmentNotification != null && _enlistmentNotification.SystemTransaction == transaction)
				return;

			if (HasActiveTransaction)
			{
				Diag.Dug(true, "Unable to enlist in transaction, a local transaction already exists");
				throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
			}
			if (_enlistmentNotification != null)
			{
				Diag.Dug(true, "Already enlisted in a transaction");
				throw new ArgumentException("Already enlisted in a transaction");
			}

			_enlistmentNotification = new DslEnlistmentNotification(this, transaction);
			_enlistmentNotification.Completed += new EventHandler(EnlistmentCompleted);
		}
	}

	private void EnlistmentCompleted(object sender, EventArgs e)
	{
		Diag.Dug();

		_enlistmentNotification = null;
	}

	public DslTransaction BeginTransaction(System.Transactions.IsolationLevel isolationLevel)
	{
		Diag.Dug();

		var il = isolationLevel switch
		{
			System.Transactions.IsolationLevel.Chaos => IsolationLevel.Chaos,
			System.Transactions.IsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
			System.Transactions.IsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
			System.Transactions.IsolationLevel.Serializable => IsolationLevel.Serializable,
			System.Transactions.IsolationLevel.Snapshot => IsolationLevel.Snapshot,
			System.Transactions.IsolationLevel.Unspecified => IsolationLevel.Unspecified,
			_ => IsolationLevel.ReadCommitted,
		};
		return BeginTransaction(il, null);
	}
	public Task<DslTransaction> BeginTransactionAsync(System.Transactions.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		var il = isolationLevel switch
		{
			System.Transactions.IsolationLevel.Chaos => IsolationLevel.Chaos,
			System.Transactions.IsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
			System.Transactions.IsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
			System.Transactions.IsolationLevel.Serializable => IsolationLevel.Serializable,
			System.Transactions.IsolationLevel.Snapshot => IsolationLevel.Snapshot,
			System.Transactions.IsolationLevel.Unspecified => IsolationLevel.Unspecified,
			_ => IsolationLevel.ReadCommitted,
		};
		return BeginTransactionAsync(il, null, cancellationToken);
	}

	#endregion

	#region Schema Methods

	public DataTable GetSchema(string collectionName, string[] restrictions)
	{
		Diag.Dug("collectionName: " + collectionName);

		return FbSchemaFactory.GetSchema(_owningConnection, collectionName, restrictions);
	}
	public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		return FbSchemaFactory.GetSchemaAsync(_owningConnection, collectionName, restrictions, cancellationToken);
	}

	#endregion

	#region Prepared Commands Methods

	public void AddPreparedCommand(IDslPreparedCommand command)
	{
		Diag.Dug();

		if (_preparedCommands.Contains(command))
			return;
		_preparedCommands.Add(command);
	}

	public void RemovePreparedCommand(IDslPreparedCommand command)
	{
		Diag.Dug();

		_preparedCommands.Remove(command);
	}

	public void ReleasePreparedCommands()
	{
		Diag.Dug();

		// copy the data because the collection will be modified via RemovePreparedCommand from Release
		var data = _preparedCommands.ToList();
		foreach (var item in data)
		{
			try
			{
				item.Release();
			}
			catch (IOException)
			{
				// If an IO error occurs when trying to release the command
				// avoid it. (It maybe the connection to the server was down
				// for unknown reasons.)
			}
			catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_network_error
				|| ex.ErrorCode == IscCodes.isc_net_read_err
				|| ex.ErrorCode == IscCodes.isc_net_write_err)
			{ }
		}
	}
	public async Task ReleasePreparedCommandsAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		// copy the data because the collection will be modified via RemovePreparedCommand from Release
		var data = _preparedCommands.ToList();
		foreach (var item in data)
		{
			try
			{
				await item.ReleaseAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				// If an IO error occurs when trying to release the command
				// avoid it. (It maybe the connection to the server was down
				// for unknown reasons.)
			}
			catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_network_error
				|| ex.ErrorCode == IscCodes.isc_net_read_err
				|| ex.ErrorCode == IscCodes.isc_net_write_err)
			{ }
		}
	}

	#endregion

	#region Blackbird Events Methods

	public void CloseEventManager()
	{
		Diag.Dug();

		if (_db != null && _db.HasRemoteEventSupport)
		{
			_db.CloseEventManager();
		}
	}
	public Task CloseEventManagerAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		if (_db != null && _db.HasRemoteEventSupport)
		{
			return _db.CloseEventManagerAsync(cancellationToken).AsTask();
		}
		return Task.CompletedTask;
	}

	#endregion

	#region Private Methods

	private void EnsureNoActiveTransaction()
	{
		Diag.Dug();

		if (HasActiveTransaction)
		{
			Diag.Dug(true, "A transaction is currently active. Parallel transactions are not supported");
			throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
		}
	}

	private static DatabaseParameterBufferBase BuildDpb(DatabaseBase db, DbConnectionString options)
	{
		Diag.Dug();

		var dpb = db.CreateDatabaseParameterBuffer();

		if (db.UseUtf8ParameterBuffer)
		{
			dpb.Append(IscCodes.isc_dpb_utf8_filename, 0);
		}
		dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_sql_dialect, new byte[] { (byte)options.Dialect, 0, 0, 0 });
		dpb.Append(IscCodes.isc_dpb_lc_ctype, options.Charset);
		if (options.DbCachePages > 0)
		{
			dpb.Append(IscCodes.isc_dpb_num_buffers, options.DbCachePages);
		}
		if (!string.IsNullOrEmpty(options.UserID))
		{
			dpb.Append(IscCodes.isc_dpb_user_name, options.UserID);
		}
		if (!string.IsNullOrEmpty(options.Role))
		{
			dpb.Append(IscCodes.isc_dpb_sql_role_name, options.Role);
		}
		dpb.Append(IscCodes.isc_dpb_connect_timeout, options.ConnectionTimeout);
		dpb.Append(IscCodes.isc_dpb_process_id, GetProcessId());
		dpb.Append(IscCodes.isc_dpb_process_name, GetProcessName(options));
		dpb.Append(IscCodes.isc_dpb_client_version, GetClientVersion());
		if (options.NoDatabaseTriggers)
		{
			dpb.Append(IscCodes.isc_dpb_no_db_triggers, 1);
		}
		if (options.NoGarbageCollect)
		{
			dpb.Append(IscCodes.isc_dpb_no_garbage_collect, (byte)0);
		}
		if (options.ParallelWorkers > 0)
		{
			dpb.Append(IscCodes.isc_dpb_parallel_workers, options.ParallelWorkers);
		}

		return dpb;
	}

	private static string GetProcessName(DbConnectionString options)
	{
		Diag.Dug();

		if (!string.IsNullOrEmpty(options.ApplicationName))
		{
			return options.ApplicationName;
		}
		return GetSystemWebHostingPath() ?? GetRealProcessName() ?? string.Empty;
	}


	private static string GetSystemWebHostingPath()
	{
		Diag.Dug();

#if NETFRAMEWORK
		var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals("System.Web", StringComparison.Ordinal)).FirstOrDefault();
		if (assembly == null)
			return null;
		// showing ApplicationPhysicalPath may be wrong because of connection pooling
		// better idea?
		return (string)assembly.GetType("System.Web.Hosting.HostingEnvironment").GetProperty("ApplicationPhysicalPath").GetValue(null, null);
#else
		return null;
#endif
	}

	private static string GetRealProcessName()
	{
		Diag.Dug();

#if NETFRAMEWORK
		return Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule.FileName;
#else
		static string FromProcess()
		{
			try
			{
				return Environment.ProcessPath;
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}
		return Assembly.GetEntryAssembly()?.Location ?? FromProcess();
#endif
	}

	private static int GetProcessId()
	{
		Diag.Dug();

		try
		{
#if NETFRAMEWORK
			return Process.GetCurrentProcess().Id;
#else
			return Environment.ProcessId;
#endif
		}
		catch (InvalidOperationException)
		{
			return -1;
		}
	}

	private static string GetClientVersion()
	{
		Diag.Dug();

		return typeof(DslConnectionInternal).GetTypeInfo().Assembly.GetName().Version.ToString();
	}
	#endregion

	#region Cancelation
	public void EnableCancel()
	{
		Diag.Dug();

		_db.CancelOperation(IscCodes.fb_cancel_enable);
		CancelDisabled = false;
	}
	public async Task EnableCancelAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		await _db.CancelOperationAsync(IscCodes.fb_cancel_enable, cancellationToken).ConfigureAwait(false);
		CancelDisabled = false;
	}

	public void DisableCancel()
	{
		Diag.Dug();

		_db.CancelOperation(IscCodes.fb_cancel_disable);
		CancelDisabled = true;
	}
	public async Task DisableCancelAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		await _db.CancelOperationAsync(IscCodes.fb_cancel_disable, cancellationToken).ConfigureAwait(false);
		CancelDisabled = true;
	}

	public void CancelCommand()
	{
		Diag.Dug();

		_db.CancelOperation(IscCodes.fb_cancel_raise);
	}
	public Task CancelCommandAsync(CancellationToken cancellationToken = default)
	{
		Diag.Dug();

		return _db.CancelOperationAsync(IscCodes.fb_cancel_raise, cancellationToken).AsTask();
	}
	#endregion

	#region Infrastructure
	public DslConnectionInternal SetOwningConnection(DslConnection owningConnection)
	{
		Diag.Dug();

		_owningConnection = owningConnection;
		return this;
	}
	#endregion
}
