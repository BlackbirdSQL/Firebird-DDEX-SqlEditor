// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlConnectionStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//									ConnectionStrategy Class
//
/// <summary>
/// Manages connections for a query.
/// </summary>
// =========================================================================================================
public class ConnectionStrategy : AbstractConnectionStrategy
{

	// ----------------------------------------------------
	#region Constructors / Destructors - ConnectionStrategy
	// ----------------------------------------------------

	/// <summary>
	/// Default .ctor.
	/// </summary>
	public ConnectionStrategy() : base()
	{
	}


	protected override bool Dispose(bool disposing)
	{
		if (!base.Dispose(disposing))
			return false;

		_KeepAliveCancelTokenSource?.Cancel();
		_KeepAliveCancelTokenSource?.Dispose();
		_KeepAliveCancelTokenSource = null;

		if (_OnInvalidateToolbar != null)
			_InvalidateToolbarEvent -= _OnInvalidateToolbar;

		if (_OnNotifyConnectionState != null)
			_NotifyConnectionStateEvent -= _OnNotifyConnectionState;

		_InvalidateToolbarEvent = null;
		_NotifyConnectionStateEvent = null;
		_OnNotifyConnectionState = null;
		_OnInvalidateToolbar = null;

		return true;
	}




	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - ConnectionStrategy
	// =========================================================================================================


	private CancellationTokenSource _KeepAliveCancelTokenSource = null;
	private long _KeepAliveConnectionStartTimeEpoch = long.MinValue;
	private ConnectedPropertiesWindow _PropertiesWindowObject;


	private EventHandler _InvalidateToolbarEvent = null;
	private NotifyConnectionStateEventHandler _NotifyConnectionStateEvent = null;

	private EventHandler _OnInvalidateToolbar = null;
	private NotifyConnectionStateEventHandler _OnNotifyConnectionState = null;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ConnectionStrategy
	// =========================================================================================================



	private event EventHandler InvalidateToolbarEvent
	{
		add { _InvalidateToolbarEvent += value; }
		remove { _InvalidateToolbarEvent -= value; }
	}


	private event NotifyConnectionStateEventHandler NotifyConnectionStateEvent
	{
		add { _NotifyConnectionStateEvent += value; }
		remove { _NotifyConnectionStateEvent -= value; }
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ConnectionStrategy
	// =========================================================================================================


	/// <summary>
	/// Returns true if there were no exceptions else false.
	/// If transactions do not exist returns true.
	/// </summary>
	public override bool CommitTransactions(bool validate)
	{
		if (validate)
		{
			try
			{
				if (!GetUpdatedTransactionsStatus(false))
					return true;
			}
			catch
			{
				return false;
			}
		}

		bool result = true;

		try
		{
			Transaction?.Commit();
		}
		catch
		{
			result = false;
		}


		DisposeTransaction(true);

		if (validate)
			GetUpdatedTransactionsStatus(true);

		return result;
	}



	public override object GetPropertiesWindowDisplayObject()
	{
		// return null;

		if (MdlCsb == null)
		{
			return DisconnectedPropertiesWindow.Instance;
		}

		_PropertiesWindowObject ??= new ConnectedPropertiesWindow(this);

		return _PropertiesWindowObject;

	}



	public bool InitializeKeepAlive(EventHandler onInvalidateToolbar, NotifyConnectionStateEventHandler onNotifyConnectionState)
	{
		if (_KeepAliveCancelTokenSource != null)
			return false;

		_KeepAliveCancelTokenSource = new();

		CancellationToken cancelToken = _KeepAliveCancelTokenSource.Token;

		InvalidateToolbarEvent += onInvalidateToolbar;
		NotifyConnectionStateEvent += onNotifyConnectionState;

		_OnInvalidateToolbar = onInvalidateToolbar;

		// Fire and forget.
		Task.Factory.StartNew(() => KeepAliveMonitoringAsync(cancelToken),
			default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
			TaskScheduler.Default).Forget();

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Low overhead asynchronous connection keep alive and monitoring task.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<bool> KeepAliveMonitoringAsync(CancellationToken cancelToken)
	{
		bool result = true;
		long connectionId = MdlCsb?.ConnectionId ?? long.MinValue;
		long validationCardinal = 0L;


		while (_KeepAliveCancelTokenSource != null && !cancelToken.IsCancellationRequested)
		{
			try
			{
				(result, connectionId, validationCardinal) =
					await KeepAliveMonitoringImplAsync(connectionId, validationCardinal, cancelToken);

				if (!result)
					break;

				Thread.Sleep(100);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				result = false;
				break;
			}
		}

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Low overhead asynchronous connection keep alive and monitoring task single
	/// loop.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private async Task<(bool, long, long)> KeepAliveMonitoringImplAsync(long connectionId,
		long validationCardinal, CancellationToken cancelToken)
	{
		bool connected = MdlCsb != null && MdlCsb.State == ConnectionState.Open;


		// =================================================================
		// No connection open or query manager locked.
		// =================================================================

		if (!connected || !RaiseNotifyConnectionState(EnNotifyConnectionState.RequestIsUnlocked, false))
		{
			if (!connected)
				RaiseNotifyConnectionState(EnNotifyConnectionState.ConfirmedClosed, false);

			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			validationCardinal = 0L;
			connectionId = long.MinValue;

			return (true, connectionId, validationCardinal);
		}


		// =================================================================
		// Connection open
		// =================================================================


		validationCardinal++;

		bool hadTransactions = HadTransactions;

		// -----------------------------------------------------------------
		// Another process killed the connection unexpectedly.
		// -----------------------------------------------------------------

		if (Connection == null)
		{
			validationCardinal = 0L;
			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			connectionId = long.MinValue;

			// ConnectionChangedEventArgs args = new(null, null);
			// OnConnectionChanged(this, args);

			RaiseInvalidateToolbar();
			RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyDead, hadTransactions);

			return (true, connectionId, validationCardinal);
		}



		// =================================================================
		// Connection drift detection.
		// =================================================================


		// -----------------------------------------------------------------
		// The connection changed. Reset all counters.
		// -----------------------------------------------------------------

		if (connectionId < 0 || connectionId != MdlCsb.ConnectionId)
		{
			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			validationCardinal = 0L;
			connectionId = MdlCsb.ConnectionId;

			return (true, connectionId, validationCardinal);
		}



		// -----------------------------------------------------------------
		// Drift detection. Refresh connection and reset counters.
		// -----------------------------------------------------------------
		if (MdlCsb.IsInvalidated)
		{
			Csb csaRegistered = RctManager.CloneRegistered(MdlCsb);

			// Compare the current connection with the registered connection.
			if (Csb.AreEquivalent(csaRegistered, (DbConnectionStringBuilder)MdlCsb, Csb.DescriberKeys))
			{
				// Nothing's changed.
				MdlCsb.RefreshDriftDetectionState();
			}
			else
			{
				bool hasConnection = Connection != null;
				bool isOpened = MdlCsb.State == ConnectionState.Open;

				LiveMdlCsb = new ModelCsb(csaRegistered);

				if (hasConnection)
				{
					MdlCsb.CreateDataConnection();
					if (isOpened)
					{
						try
						{
							await MdlCsb.OpenOrVerifyConnectionAsync();
						}
						catch { }
					}
				}

				RaiseInvalidateToolbar();

				_KeepAliveConnectionStartTimeEpoch = long.MinValue;
				validationCardinal = 0L;

				return (true, connectionId, validationCardinal);
			}
		}




		// =================================================================
		// Check validation modulus.
		// =================================================================

		if ((validationCardinal % LibraryData.C_ConnectionValidationModulus) != 0)
			return (true, connectionId, validationCardinal);




		// =================================================================
		// Connection Lifetime check if not in TTS.
		// =================================================================


		validationCardinal = 0L;
		int connectionLifeTime = !hadTransactions ? MdlCsb.ConnectionLifeTime : 0;

		if (connectionLifeTime > 0)
		{
			if (_KeepAliveConnectionStartTimeEpoch == long.MinValue)
			{
				_KeepAliveConnectionStartTimeEpoch = DateTime.Now.UnixMilliseconds();
			}
			else
			{
				long currentTimeEpoch = DateTime.Now.UnixMilliseconds();

				if (currentTimeEpoch - _KeepAliveConnectionStartTimeEpoch > (connectionLifeTime * 1000))
				{
					validationCardinal = 0L;
					_KeepAliveConnectionStartTimeEpoch = long.MinValue;

					// Sanity check.
					if (!GetUpdatedTransactionsStatus(true))
					{
						CloseConnection();
						RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyAutoClosed, false);
					}

					RaiseInvalidateToolbar();

					return (true, connectionId, validationCardinal);
				}
			}
		}
		else
		{
			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
		}




		// =================================================================
		// Connection validation / verification.
		// =================================================================


		if (!connected)
			return (true, connectionId, validationCardinal);

		// Remember the connection id because a new connection may be
		// created while we're asynchronously checking.

		connectionId = MdlCsb.ConnectionId;
		bool hasTransactions = hadTransactions;


		// If we HadTransactions validate them.
		try
		{
			if (hadTransactions)
				hasTransactions = MdlCsb.HasTransactions;
		}
		catch
		{
			hasTransactions = false;
		}


		// If we never HadTransactions or we now don't HasTransactions
		// validate the connection.
		try
		{
			if (!hasTransactions)
				_ = await MdlCsb.OpenOrVerifyConnectionAsync();
		}
		catch { }


		if (_KeepAliveCancelTokenSource == null || cancelToken.IsCancellationRequested)
			return (false, connectionId, validationCardinal);

		// If we're on the same connection after returning from an async call
		// and the Connection was disposed of, it means there was a fail.

		if (Connection == null && MdlCsb != null
				&& connectionId == MdlCsb.ConnectionId)
		{
			connectionId = long.MinValue;
			RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyReset, hadTransactions);
		}
		else if (MdlCsb != null && connectionId == MdlCsb.ConnectionId)
		{
			// If we have a solution merge the status flag will be wrong.
			RaiseNotifyConnectionState(EnNotifyConnectionState.ConfirmedOpen, false);
		}

		return (true, connectionId, validationCardinal);
	}



	public void ResetKeepAliveTimeEpoch()
	{
		_KeepAliveConnectionStartTimeEpoch = long.MinValue;
	}



	/// <summary>
	/// Returns true if there were no exceptions else false.
	/// If transactions do not exist returns true.
	/// </summary>
	public override bool RollbackTransactions(bool validate)
	{
		if (validate)
		{
			try
			{
				if (!GetUpdatedTransactionsStatus(false))
					return true;
			}
			catch
			{
				return false;
			}
		}


		bool result = true;

		try
		{
			Transaction?.Rollback();
		}
		catch
		{
			result = false;
		}


		DisposeTransaction(true);
		GetUpdatedTransactionsStatus(true);

		return result;
	}



	public bool SetDatasetKeyOnConnection(string selectedQualifiedName, IBsCsb csb)
	{
		// Tracer.Trace(GetType(), "SetDatasetKeyOnConnection()", "selectedDatasetKey: {0}, ConnectionString: {1}.", selectedDatasetKey, csb.ConnectionString);

		lock (_LockObject)
		{
			try
			{
				if (csb == null || csb.AdornedQualifiedTitle != selectedQualifiedName || _RctStamp != RctManager.Stamp)
				{
					_RctStamp = RctManager.Stamp;
					csb = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedQualifiedName, EnRctKeyType.AdornedQualifiedTitle);
				}

				if (csb == null)
					return false;

				IBsModelCsb mdlCsb = new ModelCsb(csb);

				LiveMdlCsb = mdlCsb;

				try
				{
					mdlCsb.CreateDataConnection();
				}
				finally
				{
					_DatabaseChangedEvent?.Invoke(this, new EventArgs());
				}

			}
			catch (DbException ex)
			{
				Diag.Expected(ex);
				MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessibleEx.FmtRes(selectedQualifiedName), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		return true;
	}



	protected override void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState)
	{
		/*
		if (currentState == ConnectionState.Open)
		{
			QueryServerSideProperties();
		}

		if (currentState != ConnectionState.Open || previousState != 0 || !string.IsNullOrEmpty(SqlServerConnectionService.GetDatabaseName(MdlCsb)) || Connection == null)
		{
			return;
		}

		try
		{
			FbConnection asSqlConnection = ReliableConnectionHelper.GetAsSqlConnection(Connection);
			string text = null;
			try
			{
				text = GetDefaultDatabaseForLogin(asSqlConnection, useExistingDbOnError: false);
			}
			catch (Exception ex)
			{
				if (ex.menamy != "EnumeratorException")
					throw ex;
			}

			if (string.IsNullOrEmpty(text))
			{
				text = Master;
			}

			SqlServerConnectionService.SetDatabaseName(MdlCsb, text);
		}
		catch (Exception e)
		{
			Diag.Dug(e);
		}
		*/
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ConnectionStrategy
	// =========================================================================================================


	protected override void RaiseInvalidateToolbar()
	{
		_InvalidateToolbarEvent?.Invoke(this, new EventArgs());
	}


	protected override bool RaiseNotifyConnectionState(EnNotifyConnectionState state, bool ttsDiscarded)
	{
		if (_NotifyConnectionStateEvent == null)
			return false;

		NotifyConnectionStateEventArgs args = new(state, ttsDiscarded);

		return _NotifyConnectionStateEvent.Invoke(this, args);
	}



	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - ConnectionStrategy
	// =========================================================================================================




	#endregion Sub-Classes

}
