// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Extensions;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TInternalClassConnectionSupport Class
//
/// <summary>
/// Implementation of the <see cref="IVsDataCommand"/>, <see cref="IVsDataAsyncCommand"/> and
/// <see cref="IVsDataTransaction"/> internal subclass interfaces.
/// </summary>
// =========================================================================================================
internal class TInternalClassConnectionSupport : AbstractConnectionSupport
{



	// =====================================================================================================
	//											TCommand Class
	//
	/// <summary>
	/// Implementation of the <see cref="IVsDataCommand"/> subclass interface.
	/// </summary>
	/// 
	#region TCommand Internal Sub Class
	// =====================================================================================================
	protected class TCommand : DataCommand
	{
		private TInternalClassConnectionSupport _ConnectionSupport = null;


		protected TInternalClassConnectionSupport ConnectionSupport
		{
			get
			{
				_ConnectionSupport ??= Site.GetService(typeof(IVsDataConnectionSupport)) as TInternalClassConnectionSupport;

				return _ConnectionSupport;
			}
		}

		public TCommand() : base()
		{
			Diag.Trace();
		}

		public TCommand(TInternalClassConnectionSupport connectionSupport) : base()
		{
			Diag.Trace();
			_ConnectionSupport = connectionSupport;
		}

		public TCommand(IVsDataConnection connection) : base(connection)
		{
			Diag.Trace();
		}

		public TCommand(TInternalClassConnectionSupport connectionSupport, IVsDataConnection connection)
			: base(connection)
		{
			Diag.Trace();
			_ConnectionSupport = connectionSupport;
		}

		public override IVsDataParameter CreateParameter()
		{
			Diag.Trace();
			return ConnectionSupport.CreateParameterCore();
		}

		public override IVsDataParameter[] DeriveParameters(string command, DataCommandType commandType, int commandTimeout)
		{
			Diag.Trace(command);
			return ConnectionSupport.DeriveParametersCore(command, commandType, commandTimeout);
		}

		public override string Prepare(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace(command);
			return ConnectionSupport.PrepareCore(command, commandType, parameters, commandTimeout);
		}

		public override IVsDataReader DeriveSchema(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace(command);
			return ConnectionSupport.DeriveSchemaCore(command, commandType, parameters, commandTimeout);
		}

		public override IVsDataReader Execute(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace(command);
			return ConnectionSupport.ExecuteCore(command, commandType, parameters, commandTimeout);
		}

		public override int ExecuteWithoutResults(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace(command);
			return ConnectionSupport.ExecuteWithoutResultsCore(command, commandType, parameters, commandTimeout);
		}
	}


	#endregion TCommand Internal Sub Class





	// =====================================================================================================
	//										TAsyncCommand Class
	//
	/// <summary>
	/// Implementation of the <see cref="IVsDataAsyncCommand"/> subclass interfaces.
	/// </summary>
	/// 
	#region TAsyncCommand Internal Sub Class
	// =====================================================================================================
	protected class TAsyncCommand : DataAsyncCommand
	{
		private TInternalClassConnectionSupport _ConnectionSupport = null;


		protected TInternalClassConnectionSupport ConnectionSupport
		{
			get
			{
				_ConnectionSupport ??= Site.GetService(typeof(IVsDataConnectionSupport)) as TInternalClassConnectionSupport;

				return _ConnectionSupport;
			}
		}

		public TAsyncCommand(IVsDataConnection connection)
			: base(connection)
		{
			Diag.Trace();
		}


		public TAsyncCommand(TInternalClassConnectionSupport connectionSupport, IVsDataConnection connection)
			: base(connection)
		{
			Diag.Trace();
			_ConnectionSupport = connectionSupport;
		}


		protected override IVsDataParameter[] OnDeriveParameters(string command, DataCommandType commandType, int commandTimeout)
		{
			Diag.Trace();
			ConnectionSupport.InAsyncMode = true;
			try
			{
				return base.OnDeriveParameters(command, commandType, commandTimeout);
			}
			finally
			{
				ConnectionSupport.InAsyncMode = false;
			}
		}

		protected override string OnPrepare(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace();
			ConnectionSupport.InAsyncMode = true;
			try
			{
				return base.OnPrepare(command, commandType, parameters, commandTimeout);
			}
			finally
			{
				ConnectionSupport.InAsyncMode = false;
			}
		}

		protected override IVsDataReader OnDeriveSchema(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace();
			ConnectionSupport.InAsyncMode = true;
			return base.OnDeriveSchema(command, commandType, parameters, commandTimeout);
		}

		protected override void OnDeriveSchemaCompleted(DataAsyncCommandCompletedEventArgs<IVsDataReader> e)
		{
			Diag.Trace();
			TInternalClassConnectionSupport connectionSupport = ConnectionSupport;
			try
			{
				base.OnDeriveSchemaCompleted(e);
			}
			finally
			{
				connectionSupport.CurrentCommand = null;
				connectionSupport.InAsyncMode = false;
			}
		}

		protected override IVsDataReader OnExecute(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace();
			ConnectionSupport.InAsyncMode = true;
			return base.OnExecute(command, commandType, parameters, commandTimeout);
		}

		protected override void OnExecuteCompleted(DataAsyncCommandCompletedEventArgs<IVsDataReader> e)
		{
			Diag.Trace();
			TInternalClassConnectionSupport connectionSupport = ConnectionSupport;
			try
			{
				base.OnExecuteCompleted(e);
			}
			finally
			{
				connectionSupport.CurrentCommand = null;
				connectionSupport.InAsyncMode = false;
			}
		}

		protected override int OnExecuteWithoutResults(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Diag.Trace();
			ConnectionSupport.InAsyncMode = true;
			try
			{
				return base.OnExecuteWithoutResults(command, commandType, parameters, commandTimeout);
			}
			finally
			{
				ConnectionSupport.InAsyncMode = false;
			}
		}

		protected override void OnCancel(object userState)
		{
			Diag.Trace();
			ConnectionSupport.CurrentCommand?.Cancel();
			base.OnCancel(userState);
		}
	}


	#endregion TAsyncCommand Internal Sub Class





	// =====================================================================================================
	//										TTransaction Class
	//
	/// <summary>
	/// Implementation of the <see cref="IVsDataTransaction"/> subclass interface.
	/// </summary>
	/// 
	#region TTransaction Internal Sub Class
	// =====================================================================================================
	protected class TTransaction : DataTransaction
	{
		private TInternalClassConnectionSupport _ConnectionSupport = null;


		protected TInternalClassConnectionSupport ConnectionSupport
		{
			get
			{
				_ConnectionSupport ??= Site.GetService(typeof(IVsDataConnectionSupport)) as TInternalClassConnectionSupport;

				return _ConnectionSupport;
			}
		}

		public TTransaction(IVsDataConnection connection)
			: base(connection)
		{
			Diag.Trace();
		}


		public TTransaction(TInternalClassConnectionSupport connectionSupport, IVsDataConnection connection)
			: base(connection)
		{
			Diag.Trace();
			_ConnectionSupport = connectionSupport;
		}


		public override int BeginTransaction()
		{
			Diag.Trace();
			ConnectionSupport.BeginTransactionCore();
			return base.BeginTransaction();
		}

		public override int CommitTransaction()
		{
			Diag.Trace();
			ConnectionSupport.CommitTransactionCore();
			return base.CommitTransaction();
		}

		public override int RollbackTransaction()
		{
			Diag.Trace();
			try
			{
				ConnectionSupport.RollbackTransactionCore();
			}
			catch
			{
			}

			return base.RollbackTransaction();
		}
	}

	#endregion TTransaction Internal Sub Class





	// =====================================================================================================
	#region Method Implementations - TInternalClassConnectionSupport
	// =====================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new service object internal sub class based on the specified interface
	/// service type.
	/// </summary>
	/// <param name="container">A service provider object to contain the service.</param>
	/// <param name="serviceType">A System.Type of the service to create.</param>
	/// <returns>The service object.</returns>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(IServiceContainer container, Type serviceType)
	{

		if (serviceType == typeof(IVsDataCommand))
		{
			return new TCommand(this, Site);
		}
		else if (serviceType == typeof(IVsDataAsyncCommand))
		{
			return new TAsyncCommand(this, Site);
		}
		else if (serviceType == typeof(IVsDataTransaction))
		{
			return new TTransaction(this, Site);
		}


		Diag.Dug(true, serviceType.FullName + " is not directly supported");

		return base.CreateService(container, serviceType);
	}

	#endregion Method Implementations

}
