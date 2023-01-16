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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;
using BlackbirdSql.Data.Logging;



namespace BlackbirdSql.Data.DslClient;

[DefaultEvent("InfoMessage")]
public sealed class DslConnection : DbConnection, ICloneable
{
	static readonly IFbLogger Log = FbLogManager.CreateLogger(nameof(DslConnection));

	#region Static Pool Handling Methods

	public static void ClearAllPools()
	{
		Diag.Trace();

		DslConnectionPoolManager.Instance.ClearAllPools();
	}

	public static void ClearPool(DslConnection connection)
	{
		Diag.Trace();

		if (connection == null)
			throw new ArgumentNullException(nameof(connection));

		DslConnectionPoolManager.Instance.ClearPool(connection.ConnectionOptions);
	}

	public static void ClearPool(string connectionString)
	{
		Diag.Trace();

		if (connectionString == null)
			throw new ArgumentNullException(nameof(connectionString));

		DslConnectionPoolManager.Instance.ClearPool(new ConnectionString(connectionString));
	}

	#endregion

	#region Static Database Creation/Drop methods

	public static void CreateDatabase(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false)
	{
		Diag.Trace();

		var options = new ConnectionString(connectionString);
		options.Validate();

		try
		{
			var db = new DslConnectionInternal(options);
			try
			{
				db.CreateDatabase(pageSize, forcedWrites, overwrite);
			}
			finally
			{
				db.Disconnect();
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
	public static async Task CreateDatabaseAsync(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false, CancellationToken cancellationToken = default)
	{
		var options = new ConnectionString(connectionString);
		options.Validate();

		try
		{
			var db = new DslConnectionInternal(options);
			try
			{
				await db.CreateDatabaseAsync(pageSize, forcedWrites, overwrite, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await db.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	public static void DropDatabase(string connectionString)
	{
		Diag.Trace();

		var options = new ConnectionString(connectionString);
		options.Validate();

		try
		{
			var db = new DslConnectionInternal(options);
			try
			{
				db.DropDatabase();
			}
			finally
			{
				db.Disconnect();
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
	public static async Task DropDatabaseAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		var options = new ConnectionString(connectionString);
		options.Validate();

		try
		{
			var db = new DslConnectionInternal(options);
			try
			{
				await db.DropDatabaseAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await db.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (IscException ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	#endregion

	#region Events

	public override event StateChangeEventHandler StateChange;

	public event EventHandler<DslInfoMessageEventArgs> InfoMessage;

	#endregion

	#region Fields

	private DslConnectionInternal _innerConnection;
	private ConnectionState _state;
	private ConnectionString _options;
	private bool _disposed;
	private string _connectionString;

	#endregion

	#region Properties

	[Category("Data")]
	[SettingsBindable(true)]
	[RefreshProperties(RefreshProperties.All)]
	[DefaultValue("")]
	public override string ConnectionString
	{
		get { return _connectionString; }
		set
		{
			Diag.Trace();

			if (_state == ConnectionState.Closed)
			{
				value ??= string.Empty;

				_options = new ConnectionString(value);
				_options.Validate();
				_connectionString = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override int ConnectionTimeout
	{
		get { return _options.ConnectionTimeout; }
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string Database
	{
		get { return _options.Database; }
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string DataSource
	{
		get { return _options.DataSource; }
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string ServerVersion
	{
		get
		{
			if (_state == ConnectionState.Closed)
			{
				Diag.Dug(true, "The connection is closed.");
				throw new InvalidOperationException("The connection is closed.");
			}

			if (_innerConnection != null)
			{
				return _innerConnection.Database.ServerVersion;
			}

			return string.Empty;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override ConnectionState State
	{
		get { return _state; }
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int PacketSize
	{
		get { return _options.PacketSize; }
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int CommandTimeout
	{
		get { return _options.CommandTimeout; }
	}

	#endregion

	#region Internal Properties

	internal DslConnectionInternal InnerConnection
	{
		get { return _innerConnection; }
	}

	internal ConnectionString ConnectionOptions
	{
		get { return _options; }
	}

	internal bool IsClosed
	{
		get { return _state == ConnectionState.Closed; }
	}

	#endregion

	#region Protected Properties

	protected override DbProviderFactory DbProviderFactory
	{
		get { return DslProviderFactory.Instance; }
	}

	#endregion

	#region Constructors

	public DslConnection()
	{
		Diag.Trace();

		_options = new ConnectionString();
		_state = ConnectionState.Closed;
		_connectionString = string.Empty;
	}

	public DslConnection(string connectionString) : this()
	{
		Diag.Trace();

		if (!string.IsNullOrEmpty(connectionString))
		{
			ConnectionString = connectionString;
		}
	}

	#endregion

	#region IDisposable, IAsyncDisposable methods

	protected override void Dispose(bool disposing)
	{
		Diag.Trace();

		if (disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				Close();
				_innerConnection = null;
				_options = null;
				_connectionString = null;
			}
		}
		base.Dispose(disposing);
	}
#if NET
	public override async ValueTask DisposeAsync()
	{
		Diag.Trace();

		if (!_disposed)
		{
			_disposed = true;
			await CloseAsync().ConfigureAwait(false);
			_innerConnection = null;
			_options = null;
			_connectionString = null;
		}
		await base.DisposeAsync().ConfigureAwait(false);
	}
#endif

	#endregion

	#region ICloneable Methods

	object ICloneable.Clone()
	{
		Diag.Trace();

		return new DslConnection(ConnectionString);
	}

	#endregion

	#region Transaction Handling Methods

	public new DslTransaction BeginTransaction() => BeginTransaction(DslTransaction.DefaultIsolationLevel, null);
#if NETFRAMEWORK
	public Task<DslTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#else
	public new Task<DslTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#endif
			=> BeginTransactionAsync(DslTransaction.DefaultIsolationLevel, null, cancellationToken);

	public new DslTransaction BeginTransaction(IsolationLevel level) => BeginTransaction(level, null);
#if NETFRAMEWORK
	public Task<DslTransaction> BeginTransactionAsync(IsolationLevel level, CancellationToken cancellationToken = default)
#else
	public new Task<DslTransaction> BeginTransactionAsync(IsolationLevel level, CancellationToken cancellationToken = default)
#endif
			=> BeginTransactionAsync(level, null, cancellationToken);

	public DslTransaction BeginTransaction(string transactionName) => BeginTransaction(DslTransaction.DefaultIsolationLevel, transactionName);
	public Task<DslTransaction> BeginTransactionAsync(string transactionName, CancellationToken cancellationToken = default) => BeginTransactionAsync(DslTransaction.DefaultIsolationLevel, transactionName, cancellationToken);

	public DslTransaction BeginTransaction(IsolationLevel level, string transactionName)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.BeginTransaction(level, transactionName);
	}
	public Task<DslTransaction> BeginTransactionAsync(IsolationLevel level, string transactionName, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.BeginTransactionAsync(level, transactionName, cancellationToken);
	}

	public DslTransaction BeginTransaction(DslTransactionOptions options) => BeginTransaction(options, null);
	public Task<DslTransaction> BeginTransactionAsync(DslTransactionOptions options, CancellationToken cancellationToken = default) => BeginTransactionAsync(options, null, cancellationToken);

	public DslTransaction BeginTransaction(DslTransactionOptions options, string transactionName)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.BeginTransaction(options, transactionName);
	}
	public Task<DslTransaction> BeginTransactionAsync(DslTransactionOptions options, string transactionName, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.BeginTransactionAsync(options, transactionName, cancellationToken);
	}

	protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel);
#if NET
	protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken) => await BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
#endif

	#endregion

	#region Transaction Enlistement

	public override void EnlistTransaction(System.Transactions.Transaction transaction)
	{
		Diag.Trace();

		CheckClosed();

		if (transaction == null)
			return;

		_innerConnection.EnlistTransaction(transaction);
	}

	#endregion

	#region Database Schema Methods

	public override DataTable GetSchema()
	{
		Diag.Trace("Defaulting to collectionName: MetaDataCollections");

		return GetSchema("MetaDataCollections");
	}
#if NETFRAMEWORK
	public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
#else
	public override Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
#endif
	{
		Diag.Trace();

		return GetSchemaAsync("MetaDataCollections", cancellationToken);
	}

	public override DataTable GetSchema(string collectionName)
	{
		Diag.Trace("collectionName: " + collectionName);

		return GetSchema(collectionName, null);
	}
#if NETFRAMEWORK
	public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
#else
	public override Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
#endif
	{
		Diag.Trace();

		return GetSchemaAsync(collectionName, null, cancellationToken);
	}
	public override DataTable GetSchema(string collectionName, string[] restrictions)
	{
		Diag.Trace("collectionName: " + collectionName);

		string str = "";

		if (restrictions != null)
		{
			foreach (object item in restrictions)
			{
				str += (item != null && item != DBNull.Value ? item.ToString() : "null") + ", ";
			}
		}

		Diag.Trace(String.Format("collectionName: {0} restrictions: {1}", collectionName, str));

		CheckClosed();

		return _innerConnection.GetSchema(collectionName, restrictions);
	}
#if NETFRAMEWORK
	public Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
#else
	public override Task<DataTable> GetSchemaAsync(string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
#endif
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.GetSchemaAsync(collectionName, restrictions, cancellationToken);
	}

	#endregion

	#region Methods

	public new DslCommand CreateCommand()
	{
		Diag.Trace();

		return (DslCommand)CreateDbCommand();
	}

	protected override DbCommand CreateDbCommand()
	{
		Diag.Trace();

		return new DslCommand(null, this);
	}

#if NET6_0_OR_GREATER
	public new DbBatch CreateBatch()
	{
		Diag.Dug(true, "DbBatch is currently not supported. Use FbBatchCommand instead.");
		throw new NotSupportedException("DbBatch is currently not supported. Use FbBatchCommand instead.");
	}

	protected override DbBatch CreateDbBatch()
	{
		Diag.Trace();

		return CreateBatch();
	}
#endif

	public DslBatchCommand CreateBatchCommand()
	{
		Diag.Trace();

		return new DslBatchCommand(null, this);
	}

	public override void ChangeDatabase(string databaseName)
	{
		Diag.Trace();

		CheckClosed();

		if (string.IsNullOrEmpty(databaseName))
		{
			Diag.Dug(true, "Database name is not valid.");
			throw new InvalidOperationException("Database name is not valid.");
		}

		var oldConnectionString = _connectionString;
		try
		{
			var csb = new ConnectionStringBuilder(_connectionString);

			/* Close current connection	*/
			Close();

			/* Set up the new Database	*/
			csb.Database = databaseName;
			ConnectionString = csb.ToString();

			/* Open	new	connection	*/
			Open();
		}
		catch (IscException ex)
		{
			ConnectionString = oldConnectionString;
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
#if NETFRAMEWORK
	public async Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
#else
	public override async Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
#endif
	{
		Diag.Trace();

		CheckClosed();

		if (string.IsNullOrEmpty(databaseName))
		{
			Diag.Dug(true, "Database name is not valid.");
			throw new InvalidOperationException("Database name is not valid.");
		}

		var oldConnectionString = _connectionString;
		try
		{
			var csb = new ConnectionStringBuilder(_connectionString);

			/* Close current connection	*/
			await CloseAsync().ConfigureAwait(false);

			/* Set up the new Database	*/
			csb.Database = databaseName;
			ConnectionString = csb.ToString();

			/* Open	new	connection	*/
			await OpenAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (IscException ex)
		{
			ConnectionString = oldConnectionString;
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	public override void Open()
	{
		Diag.Trace();

		LogMessages.ConnectionOpening(Log, this);

		if (string.IsNullOrEmpty(_connectionString))
		{
			Diag.Dug(true, "onnection String is not initialized.");
			throw new InvalidOperationException("Connection String is not initialized.");
		}
		if (!IsClosed && _state != ConnectionState.Connecting)
		{
			Diag.Dug(true, "Connection already Open.");
			throw new InvalidOperationException("Connection already Open.");
		}

		try
		{
			OnStateChange(_state, ConnectionState.Connecting);

			var createdNew = default(bool);
			if (_options.Pooling)
			{
				_innerConnection = DslConnectionPoolManager.Instance.Get(_options, out createdNew);
			}
			else
			{
				_innerConnection = new DslConnectionInternal(_options);
				createdNew = true;
			}
			if (createdNew)
			{
				try
				{
					try
					{
						_innerConnection.Connect();
					}
					catch (OperationCanceledException ex)
					{
						//cancellationToken.ThrowIfCancellationRequested();
						Diag.Dug(ex);
						throw new TimeoutException("Timeout while connecting.", ex);
					}
				}
				catch
				{
					if (_options.Pooling)
					{
						DslConnectionPoolManager.Instance.Release(_innerConnection, false);
					}
					throw;
				}
			}
			_innerConnection.SetOwningConnection(this);

			if (_options.Enlist)
			{
				try
				{
					EnlistTransaction(System.Transactions.Transaction.Current);
				}
				catch
				{
					// if enlistment fails clean up innerConnection
					_innerConnection.DisposeTransaction();

					if (_options.Pooling)
					{
						DslConnectionPoolManager.Instance.Release(_innerConnection, true);
					}
					else
					{
						_innerConnection.Disconnect();
						_innerConnection = null;
					}

					throw;
				}
			}

			// Bind	Warning	messages event
			_innerConnection.Database.WarningMessage = OnWarningMessage;

			// Update the connection state
			OnStateChange(_state, ConnectionState.Open);
		}
		catch (IscException ex)
		{
			OnStateChange(_state, ConnectionState.Closed);
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
		catch
		{
			OnStateChange(_state, ConnectionState.Closed);
			throw;
		}

		LogMessages.ConnectionOpened(Log, this);
	}
	public override async Task OpenAsync(CancellationToken cancellationToken)
	{
		Diag.Trace();

		LogMessages.ConnectionOpening(Log, this);

		if (string.IsNullOrEmpty(_connectionString))
		{
			Diag.Dug(true, "Connection String is not initialized.");
			throw new InvalidOperationException("Connection String is not initialized.");
		}
		if (!IsClosed && _state != ConnectionState.Connecting)
		{
			Diag.Dug(true, "Connection already Open.");
			throw new InvalidOperationException("Connection already Open.");
		}

		try
		{
			OnStateChange(_state, ConnectionState.Connecting);

			var createdNew = default(bool);
			if (_options.Pooling)
			{
				_innerConnection = DslConnectionPoolManager.Instance.Get(_options, out createdNew);
			}
			else
			{
				_innerConnection = new DslConnectionInternal(_options);
				createdNew = true;
			}
			if (createdNew)
			{
				try
				{
					try
					{
						await _innerConnection.ConnectAsync(cancellationToken).ConfigureAwait(false);
					}
					catch (OperationCanceledException ex)
					{
						cancellationToken.ThrowIfCancellationRequested();
						Diag.Dug(ex);
						throw new TimeoutException("Timeout while connecting.", ex);
					}
				}
				catch
				{
					if (_options.Pooling)
					{
						DslConnectionPoolManager.Instance.Release(_innerConnection, false);
					}
					throw;
				}
			}
			_innerConnection.SetOwningConnection(this);

			if (_options.Enlist)
			{
				try
				{
					EnlistTransaction(System.Transactions.Transaction.Current);
				}
				catch
				{
					// if enlistment fails clean up innerConnection
					await _innerConnection.DisposeTransactionAsync(cancellationToken).ConfigureAwait(false);

					if (_options.Pooling)
					{
						DslConnectionPoolManager.Instance.Release(_innerConnection, true);
					}
					else
					{
						await _innerConnection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
						_innerConnection = null;
					}

					throw;
				}
			}

			// Bind	Warning	messages event
			_innerConnection.Database.WarningMessage = OnWarningMessage;

			// Update the connection state
			OnStateChange(_state, ConnectionState.Open);
		}
		catch (IscException ex)
		{
			OnStateChange(_state, ConnectionState.Closed);
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
		catch
		{
			OnStateChange(_state, ConnectionState.Closed);
			throw;
		}

		LogMessages.ConnectionOpened(Log, this);
	}

	public override void Close()
	{
		Diag.Trace();

		LogMessages.ConnectionClosing(Log, this);

		if (!IsClosed && _innerConnection != null)
		{
			try
			{
				_innerConnection.CloseEventManager();

				if (_innerConnection.Database != null)
				{
					_innerConnection.Database.WarningMessage = null;
				}

				_innerConnection.DisposeTransaction();

				_innerConnection.ReleasePreparedCommands();

				if (_options.Pooling)
				{
					if (_innerConnection.CancelDisabled)
					{
						_innerConnection.EnableCancel();
					}

					var broken = _innerConnection.Database.ConnectionBroken;
					DslConnectionPoolManager.Instance.Release(_innerConnection, !broken);
					if (broken)
					{
						DisconnectEnlistedHelper();
					}
				}
				else
				{
					DisconnectEnlistedHelper();
				}
			}
			catch
			{ }
			finally
			{
				OnStateChange(_state, ConnectionState.Closed);
			}

			LogMessages.ConnectionClosed(Log, this);
		}

		void DisconnectEnlistedHelper()
		{
			if (!_innerConnection.IsEnlisted)
			{
				_innerConnection.Disconnect();
			}
			_innerConnection = null;
		}
	}
#if NETFRAMEWORK
	public async Task CloseAsync()
#else
	public override async Task CloseAsync()
#endif
	{
		Diag.Trace();

		LogMessages.ConnectionClosing(Log, this);

		if (!IsClosed && _innerConnection != null)
		{
			try
			{
				await _innerConnection.CloseEventManagerAsync(CancellationToken.None).ConfigureAwait(false);

				if (_innerConnection.Database != null)
				{
					_innerConnection.Database.WarningMessage = null;
				}

				await _innerConnection.DisposeTransactionAsync(CancellationToken.None).ConfigureAwait(false);

				await _innerConnection.ReleasePreparedCommandsAsync(CancellationToken.None).ConfigureAwait(false);

				if (_options.Pooling)
				{
					if (_innerConnection.CancelDisabled)
					{
						await _innerConnection.EnableCancelAsync(CancellationToken.None).ConfigureAwait(false);
					}

					var broken = _innerConnection.Database.ConnectionBroken;
					DslConnectionPoolManager.Instance.Release(_innerConnection, !broken);
					if (broken)
					{
						await DisconnectEnlistedHelper().ConfigureAwait(false);
					}
				}
				else
				{
					await DisconnectEnlistedHelper().ConfigureAwait(false);
				}
			}
			catch
			{ }
			finally
			{
				OnStateChange(_state, ConnectionState.Closed);
			}

			LogMessages.ConnectionClosed(Log, this);
		}

		async Task DisconnectEnlistedHelper()
		{
			if (!_innerConnection.IsEnlisted)
			{
				await _innerConnection.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
			}
			_innerConnection = null;
		}
	}

	#endregion

	#region Private Methods

	private void CheckClosed()
	{
		Diag.Trace();

		if (IsClosed)
		{
			Diag.Dug(true, "Operation requires an open and available connection.");
			throw new InvalidOperationException("Operation requires an open and available connection.");
		}
	}

	#endregion

	#region Event Handlers

	private void OnWarningMessage(IscException warning)
	{
		Diag.Trace();

		InfoMessage?.Invoke(this, new DslInfoMessageEventArgs(warning));
	}

	private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
	{
		_state = currentState;
		StateChange?.Invoke(this, new StateChangeEventArgs(originalState, currentState));
	}

	#endregion

	#region Cancelation
	public void EnableCancel()
	{
		Diag.Trace();

		CheckClosed();

		_innerConnection.EnableCancel();
	}
	public Task EnableCancelAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.EnableCancelAsync(cancellationToken);
	}

	public void DisableCancel()
	{
		Diag.Trace();

		CheckClosed();

		_innerConnection.DisableCancel();
	}
	public Task DisableCancelAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.DisableCancelAsync(cancellationToken);
	}

	public void CancelCommand()
	{
		Diag.Trace();

		CheckClosed();

		_innerConnection.CancelCommand();
	}
	public Task CancelCommandAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		CheckClosed();

		return _innerConnection.CancelCommandAsync(cancellationToken);
	}
	#endregion

	#region Internal Methods

	internal static void EnsureOpen(DslConnection connection)
	{
		Diag.Trace();

		if (connection == null || connection.State != ConnectionState.Open)
			throw new InvalidOperationException("Connection must be valid and open.");
	}

	#endregion
}
