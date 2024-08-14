// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlConnectionStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
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
		// _KeepAliveCancelTokenSource?.Dispose();
		// _KeepAliveCancelTokenSource = null;

		if (_OnInvalidateToolbar != null)
			InvalidateToolbarEvent -= _OnInvalidateToolbar;

		if (_OnNotifyConnectionState != null)
			NotifyConnectionStateEvent -= _OnNotifyConnectionState;

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
				if (!GetUpdatedTtsStatus())
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


		DisposeTransaction();

		LiveTransactions = false;

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



	private bool GetUpdatedTtsStatus()
	{
		bool hasTransactions;

		try
		{
			hasTransactions = MdlCsb?.HasTransactions ?? false;
		}
		catch
		{
			LiveTransactions = false;
			throw;
		}

		LiveTransactions = hasTransactions;

		return hasTransactions;
	}



	public bool Initialize(IBsCsb csb, EventHandler onInvalidateToolbar, NotifyConnectionStateEventHandler onNotifyConnectionState)
	{
		if (csb != null)
			SetDatabaseCsb(new ModelCsb(csb), true);

		if (_KeepAliveCancelTokenSource != null)
			return false;

		_KeepAliveCancelTokenSource = new();

		CancellationToken cancelToken = _KeepAliveCancelTokenSource.Token;

		_OnInvalidateToolbar = onInvalidateToolbar;
		_OnNotifyConnectionState = onNotifyConnectionState;

		InvalidateToolbarEvent += _OnInvalidateToolbar;
		NotifyConnectionStateEvent += _OnNotifyConnectionState;

		
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


		while (!cancelToken.IsCancellationRequested)
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

		_KeepAliveCancelTokenSource?.Dispose();
		_KeepAliveCancelTokenSource = null;

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

		if (cancelToken.IsCancellationRequested || !connected ||
			!RaiseNotifyConnectionState(EnNotifyConnectionState.RequestIsUnlocked, false))
		{
			if (!cancelToken.IsCancellationRequested && !connected)
				RaiseNotifyConnectionState(EnNotifyConnectionState.ConfirmedClosed, false);

			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			validationCardinal = 0L;
			connectionId = long.MinValue;

			return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
		}



		// =================================================================
		// Connection open
		// =================================================================


		validationCardinal++;

		bool hasTransactions = HasTransactions;

		// -----------------------------------------------------------------
		// Another process killed the connection unexpectedly.
		// -----------------------------------------------------------------

		if (cancelToken.IsCancellationRequested || Connection == null)
		{
			validationCardinal = 0L;
			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			connectionId = long.MinValue;

			// ConnectionChangedEventArgs args = new(null, null);
			// OnConnectionChanged(this, args);

			if (!cancelToken.IsCancellationRequested)
			{

				RaiseInvalidateToolbar();
				RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyDead, hasTransactions);
			}

			return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
		}



		// =================================================================
		// Connection drift detection.
		// =================================================================


		// -----------------------------------------------------------------
		// The connection changed. Reset all counters.
		// -----------------------------------------------------------------

		if (cancelToken.IsCancellationRequested || connectionId < 0 || connectionId != MdlCsb.ConnectionId)
		{
			_KeepAliveConnectionStartTimeEpoch = long.MinValue;
			validationCardinal = 0L;
			connectionId = MdlCsb.ConnectionId;

			return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
		}



		// -----------------------------------------------------------------
		// Drift detection. Refresh connection and reset counters.
		// -----------------------------------------------------------------
		if (!cancelToken.IsCancellationRequested && !hasTransactions && RctManager.Loaded && MdlCsb.IsInvalidated)
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

				SetDatabaseCsb(new ModelCsb(csaRegistered), hasConnection);

				if (isOpened)
					await VerifyOpenConnectionAsync(cancelToken);


				if (!cancelToken.IsCancellationRequested)
					RaiseInvalidateToolbar();

				_KeepAliveConnectionStartTimeEpoch = long.MinValue;
				validationCardinal = 0L;

				return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
			}
		}




		// =================================================================
		// Check validation modulus.
		// =================================================================

		if ((validationCardinal % LibraryData.C_ConnectionValidationModulus) != 0
			|| cancelToken.IsCancellationRequested
			|| !RaiseNotifyConnectionState(EnNotifyConnectionState.RequestIsUnlocked, false))
		{
			return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
		}




		// =================================================================
		// Connection Lifetime check if not in TTS.
		// =================================================================


		validationCardinal = 0L;

		int connectionLifeTime = !hasTransactions ? MdlCsb.ConnectionLifeTime : 0;

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
					if (!LiveTransactions && !cancelToken.IsCancellationRequested)
					{
						CloseConnection();

						if (!cancelToken.IsCancellationRequested)
							RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyAutoClosed, false);
					}

					if (!cancelToken.IsCancellationRequested)
						RaiseInvalidateToolbar();

					return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
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


		if (!connected || cancelToken.IsCancellationRequested)
			return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);

		// Remember the connection id because a new connection may be
		// created while we're asynchronously checking.

		connectionId = MdlCsb.ConnectionId;

		await VerifyOpenConnectionAsync(cancelToken);


		if (cancelToken.IsCancellationRequested)
			return (false, connectionId, validationCardinal);

		// If we're on the same connection after returning from an async call
		// and the Connection was disposed of, it means there was a fail.

		if (MdlCsb != null && connectionId == MdlCsb.ConnectionId)
		{
			if (Connection == null)
			{
				connectionId = long.MinValue;
				RaiseNotifyConnectionState(EnNotifyConnectionState.NotifyReset, hasTransactions);
			}
			else
			{
				// If we have a solution merge the status flag will be wrong.
				RaiseNotifyConnectionState(EnNotifyConnectionState.ConfirmedOpen, false);
			}
		}
		else if (hasTransactions != HasTransactions)
		{
			RaiseInvalidateToolbar();
		}

		return (!cancelToken.IsCancellationRequested, connectionId, validationCardinal);
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
				if (!GetUpdatedTtsStatus())
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


		DisposeTransaction();

		LiveTransactions = false;

		return result;
	}



	public bool SetDatasetKeyOnConnection(string selectedQualifiedName)
	{
		// Tracer.Trace(GetType(), "SetDatasetKeyOnConnection()", "selectedDatasetKey: {0}, ConnectionString: {1}.", selectedDatasetKey, csb.ConnectionString);

		lock (_LockObject)
		{
			_RctStamp = RctManager.Stamp;

			IBsCsb csb = RctManager.ShutdownState
				? null
				: RctManager.CloneRegistered(selectedQualifiedName, EnRctKeyType.AdornedQualifiedTitle);

			if (csb == null)
				return false;

			SetDatabaseCsb(new ModelCsb(csb), true);
		}

		return true;
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
