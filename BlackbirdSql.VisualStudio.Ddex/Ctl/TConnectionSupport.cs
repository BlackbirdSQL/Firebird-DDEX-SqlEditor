// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionSupport"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionSupport : AdoDotNetConnectionSupport, IBsDataConnectionSupport
{


	// ----------------------------------------------------
	#region Constructors / Destructors - TConnectionSupport
	// ----------------------------------------------------


	public TConnectionSupport() : base()
	{
		// Tracer.Trace(typeof(TConnectionSupport), ".ctor");
	}


	#endregion Constructors / Destructors






	// =====================================================================================================
	#region Fields - TConnectionSupport
	// =====================================================================================================

	private readonly object _LockLocal = new();

	private bool _PasswordPromptCancelled = false;
	private EnConnectionSource _ConnectionSource = EnConnectionSource.Undefined;



	#endregion Fields







	// =====================================================================================================
	#region Property Accessors - TConnectionSupport
	// =====================================================================================================

	public EnConnectionSource ConnectionSource
	{
		get
		{

			if (_ConnectionSource != EnConnectionSource.Undefined)
				return _ConnectionSource;

			try
			{
				_ConnectionSource = RctManager.ConnectionSource;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}


			return _ConnectionSource;
		}
		set
		{
			_ConnectionSource = value;
		}
	}


	public override string ConnectionString
	{
		get
		{
			return Connection.ConnectionString;
		}
		set
		{
			if (Connection.State != 0)
				Close();

			try
			{
				Connection.ConnectionString = value;
			}
			catch (Exception ex)
			{
				ArgumentException exa = new(ex.Message, "value", ex);
				Diag.Dug(exa);
				throw exa;
			}
		}
	}


	public override int ConnectionTimeout
	{
		get { return Connection.ConnectionTimeout; }
		set { throw new NotSupportedException(); }
	}


	public bool PasswordPromptCancelled
	{
		get
		{
			lock(_LockLocal)
				return _PasswordPromptCancelled;
		}
		set
		{
			lock (_LockLocal)
			{
				if (_PasswordPromptCancelled == value)
					return;

				_PasswordPromptCancelled = value;
			}

			if (!value)
				return;

			// Fire and forget

			Task.Factory.StartNew(
				async () =>
				{

					await Task.Delay(640);

					lock (_LockLocal)
						_PasswordPromptCancelled = false;
				},
				default, TaskCreationOptions.None, TaskScheduler.Default).Forget();
		}
	}


	public override DataConnectionState State => Connection.State switch
	{
		ConnectionState.Closed => DataConnectionState.Closed,
		ConnectionState.Open => DataConnectionState.Open,
		ConnectionState.Broken => DataConnectionState.Broken,
		_ => DataConnectionState.Closed,
	};



	#endregion Property Accessors





	// =========================================================================================================
	#region Method Implementations - TConnectionSupport
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new service object based on the specified interface service type.
	/// </summary>
	/// <param name="container">
	/// A service provider object to contain the service.
	/// </param>
	/// <param name="serviceType">A System.Type of the service to create.</param>
	/// <returns>The service object.</returns>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(IServiceContainer container, Type serviceType)
	{
		// Tracer.Trace(GetType(), "CreateService()", "Service requested: {0}", serviceType.Name);

		if (serviceType == typeof(IVsDataCommand))
			return new TCommand(Site);


		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for the ProviderObjectFactory if you do.
		 * 
		if (serviceType == typeof(IVsDataSourceInformation))
		{
			// Tracer.Trace();
			return new TSourceInformation(Site);
		}
		else if (serviceType == typeof(IVsDataObjectSelector))
		{
			return new TObjectSelector(Site);
		}
		else if (serviceType == typeof(IVsDataObjectMemberComparer))
		{
			// Tracer.Trace();
			return new TObjectMemberComparer(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierConverter))
		{
			// Tracer.Trace();
			return new TObjectIdentifierConverter(Site);
		}
		else if (serviceType == typeof(IVsDataMappedObjectConverter))
		{
			// Tracer.Trace();
			return new TMappedObjectConverter(Site);
		}
		*/



		// Tracer.Trace(serviceType.FullName + " is not directly supported");

		return base.CreateService(container, serviceType);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens the specified data connection.
	/// </summary>
	/// <param name="doPromptCheck">
	/// Indicates whether the call to the Open method should return false for specified
	/// errors that relate to missing connection information.
	/// </param>
	/// <returns>
	/// true if the connection opened successfully and does not require a prompt, false
	/// if the connection is missing required connection information and a prompt should
	/// be displayed to obtain the missing information form the user.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override bool Open(bool doPromptCheck)
	{
		// Tracer.Trace(GetType(), "Open()", "doPromptCheck: {0}", doPromptCheck);

		if (State == DataConnectionState.Open || PasswordPromptCancelled)
			return true;


		IVsDataSiteableObject<IVsDataProvider> @this = this;
		IVsDataConnectionUIProperties connectionUIProperties;

		try
		{
			connectionUIProperties = @this.Site.CreateObject<IVsDataConnectionUIProperties>(Site.Source);
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
			throw;
		}

		try
		{
			if (_ConnectionSource != EnConnectionSource.Undefined)
				(connectionUIProperties as IBsDataConnectionProperties).ConnectionSource = _ConnectionSource;

			connectionUIProperties.Parse(ConnectionString);
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
			throw;
		}

		if (doPromptCheck && !connectionUIProperties.IsComplete)
			return false;


		try
		{
			Connection.Open();
			/*
			// Fire and wait.
			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await Cmd.AwaitableAsync();
				NativeDb.DatabaseEngineSvc.OpenConnection_(Connection);
				return true;
			});
			*/
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
			throw;
		}

		return true;
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Internal Classes - TConnectionSupport
	// =========================================================================================================


	/// <summary>
	/// Trace replacement for AdoDotNetCommand but doesn't seem to do anything.
	/// </summary>
	public class TCommand : DataCommand
	{
		private TConnectionSupport ConnectionSupport => base.Site.GetService(typeof(IVsDataConnectionSupport)) as TConnectionSupport;


		public TCommand() : base()
		{
			// Tracer.Trace(GetType(), "TCommand.TCommand");
		}

		public TCommand(IVsDataConnection connection)
			: base(connection)
		{
			// Tracer.Trace(GetType(), "TCommand.TCommand(IVsDataConnection)");
		}

		public override IVsDataParameter CreateParameter()
		{
			// Tracer.Trace(GetType(), "CreateParameter()");
			return ConnectionSupport.CreateParameterCore();
		}

		public override IVsDataParameter[] DeriveParameters(string command, DataCommandType commandType, int commandTimeout)
		{
			// Tracer.Trace(GetType(), "DeriveParameters()", "commandType: {0}, command: {1}", commandType, command);
			return ConnectionSupport.DeriveParametersCore(command, commandType, commandTimeout);
		}

		public override string Prepare(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Tracer.Trace(GetType(), "Prepare()", "commandType: {0}, command: {1}", commandType, command);
			return ConnectionSupport.PrepareCore(command, commandType, parameters, commandTimeout);
		}

		public override IVsDataReader DeriveSchema(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Tracer.Trace(GetType(), "DeriveSchema()", "commandType: {0}, command: {1}", commandType, command);
			return ConnectionSupport.DeriveSchemaCore(command, commandType, parameters, commandTimeout);
		}

		public override IVsDataReader Execute(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Tracer.Trace(GetType(), "Execute()", "commandType: {0}, command: {1}", commandType, command);
			return ConnectionSupport.ExecuteCore(command, commandType, parameters, commandTimeout);
		}

		public override int ExecuteWithoutResults(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Tracer.Trace(GetType(), "ExecuteWithoutResults()", "commandType: {0}, command: {1}", commandType, command);
			return ConnectionSupport.ExecuteWithoutResultsCore(command, commandType, parameters, commandTimeout);
		}
	}


	#endregion Internal Classes


}
