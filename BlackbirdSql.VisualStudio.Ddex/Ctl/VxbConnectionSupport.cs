// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdDsl;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using C5;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										VxbConnectionSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionSupport"/> interface
/// </summary>
// =========================================================================================================
public class VxbConnectionSupport : AdoDotNetConnectionSupport, IBsDataConnectionSupport
{


	// ----------------------------------------------------
	#region Constructors / Destructors - VxbConnectionSupport
	// ----------------------------------------------------


	public VxbConnectionSupport() : base()
	{
		// Evs.Trace(typeof(VxbConnectionSupport), ".ctor");
	}


	#endregion Constructors / Destructors






	// =====================================================================================================
	#region Fields - VxbConnectionSupport
	// =====================================================================================================

	private readonly object _LockLocal = new();

	private bool _PasswordPromptCancelled = false;
	private EnConnectionSource _ConnectionSource = EnConnectionSource.Undefined;



	#endregion Fields







	// =====================================================================================================
	#region Property Accessors - VxbConnectionSupport
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
				Diag.Ex(ex);
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
			// Evs.Trace(GetType(), "get_ConnectionString");

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
				Diag.Ex(exa);
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
			lock (_LockLocal)
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
	#region Method Implementations - VxbConnectionSupport
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
		// Evs.Trace(GetType(), nameof(CreateService), $"Service requested: {serviceType.Name}");

		if (serviceType == typeof(IVsDataCommand))
			return new VxiCommand(Site);

		if (serviceType == typeof(IVsDataAsyncCommand))
			return new VxiAsyncCommand(Site);


		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for the ProviderObjectFactory if you do.
		 * 
		if (serviceType == typeof(IVsDataSourceInformation))
		{
			// Evs.Trace();
			return new VxbSourceInformation(Site);
		}
		else if (serviceType == typeof(IVsDataObjectSelector))
		{
			return new VxbObjectSelector(Site);
		}
		else if (serviceType == typeof(IVsDataObjectMemberComparer))
		{
			// Evs.Trace();
			return new VxbObjectMemberComparer(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierConverter))
		{
			// Evs.Trace();
			return new VxbObjectIdentifierConverter(Site);
		}
		else if (serviceType == typeof(IVsDataMappedObjectConverter))
		{
			// Evs.Trace();
			return new VxbMappedObjectConverter(Site);
		}
		*/



		// Evs.Trace(serviceType.FullName + " is not directly supported");

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
		// Evs.Trace(GetType(), nameof(Open), $"doPromptCheck: {doPromptCheck}");

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
			Diag.Expected(ex);
			throw;
		}

		string connectionString = Connection != null ? ConnectionString : "";

		try
		{
			if (_ConnectionSource != EnConnectionSource.Undefined)
				(connectionUIProperties as IBsDataConnectionProperties).ConnectionSource = _ConnectionSource;

			connectionUIProperties.Parse(connectionString);
		}
		catch (Exception ex)
		{
			Diag.Expected(ex, $"\nConnectionString: {connectionString}");
			throw;
		}

		if (doPromptCheck && !connectionUIProperties.IsComplete)
			return false;


		try
		{
			Connection.OpenDb();
		}
		catch (Exception ex)
		{
			bool exceptionHandled = false;

			if (ex is DbException exd && exd.HasSqlException())
			{
				if (exd.GetErrorCode() == 335544325)
				{
					if (exd.Message.IndexOf("CHARACTER SET", StringComparison.OrdinalIgnoreCase) != -1)
					{
						exceptionHandled = true;
						string msg = Resources.ExceptionDbCharacterSet.Fmt(exd.Message);
						MessageCtl.ShowX(msg, Resources.ExceptionDbCharacterSetCaption, MessageBoxButtons.OK);
					}
				}
				else if (exd.GetErrorCode() == 335545004)
				{
					if (exd.Message.IndexOf("Error loading plugin", StringComparison.OrdinalIgnoreCase) != -1)
					{
						exceptionHandled = true;
						string msg = Resources.ExceptionLoadingPlugin.Fmt(exd.Message);
						MessageCtl.ShowX(msg, Resources.ExceptionLoadingPluginCaption, MessageBoxButtons.OK);
					}
				}
				else if (exd.GetErrorCode() == 335544379)
				{
					exceptionHandled = true;
					string msg = Resources.ExceptionDbVersionMismatch.Fmt(exd.Message);
					MessageCtl.ShowX(msg, Resources.ExceptionDbVersionMismatchCaption, MessageBoxButtons.OK);
				}
			}
			if (!exceptionHandled)
				Diag.Expected(ex, $"\nConnectionString: {Connection?.ConnectionString}");
			throw;
		}

		return true;
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region								Nested types - VxbConnectionSupport
	// =========================================================================================================


	/// <summary>
	/// Trace replacement for AdoDotNetCommand but doesn't seem to do anything.
	/// </summary>
	public class VxiCommand : DataCommand
	{
		private VxbConnectionSupport ConnectionSupport => base.Site.GetService(typeof(IVsDataConnectionSupport)) as VxbConnectionSupport;


		public VxiCommand() : base()
		{
			// Evs.Trace(GetType(), "VxiCommand.VxiCommand");
		}

		public VxiCommand(IVsDataConnection connection)
			: base(connection)
		{
			// Evs.Trace(GetType(), "VxiCommand.VxiCommand(IVsDataConnection)");
		}

		public override IVsDataParameter CreateParameter()
		{
			/// Evs.Trace(GetType(), "CreateParameter");
			IVsDataParameter parameter = ConnectionSupport.CreateParameterCore();

			// Evs.Trace(GetType(), "CreateParameter", $"{parameter.Name}:{parameter.Type}:{parameter.Value}: {parameter.ToString()}");

			return parameter;
		}

		public override IVsDataParameter[] DeriveParameters(string command, DataCommandType commandType, int commandTimeout)
		{
			/// Evs.Trace(GetType(), "DeriveParameters", $"commandType: {commandType}, command: {command}");
			IVsDataParameter[] parameters = ConnectionSupport.DeriveParametersCore(command, commandType, commandTimeout);
			// Evs.Trace(GetType(), "DeriveParameters", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");
			return parameters;
		}

		public override string Prepare(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Evs.Trace(GetType(), "Prepare", $"commandType: {commandType}, command: {command}");

			string retval = ConnectionSupport.PrepareCore(command, commandType, parameters, commandTimeout);

			// Evs.Trace(GetType(), "Prepare", $"commandType: {commandType}, command: {command}\n{retval}");

			return retval;
		}

		public override IVsDataReader DeriveSchema(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Evs.Trace(GetType(), "DeriveSchema", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");
			return ConnectionSupport.DeriveSchemaCore(command, commandType, parameters, commandTimeout);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private string DataParametersToString(IVsDataParameter[] parameters)
		{
			if (parameters == null)
				return "null";

			string retval = "";
			foreach (IVsDataParameter parameter in parameters)
			{
				retval += $"{parameter.Name}:{parameter.Type}:{parameter.Value}: {parameter.ToString()},  ";
			}

			return retval;
		}

		public override IVsDataReader Execute(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Evs.Trace(GetType(), "Execute", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");

			// Basic hack to get past mixed and lower case Firebird identifiers.
			// Composite tokens of the form "identifier1".identifier2."identifier3".identifier4 are handled
			// Composite tokens beginning and ending with double quotes will be skipped.
			// eg. "identifier1".identifier2."identifier3" will not detect identifier2 is unquoted.
			// "identifier1".identifier2."identifier3".identifier4 will convert
			// to "identifier1"."identifier2"."identifier3"."identifier4"

			if (command != null && command != command.ToUpper())
			{
				// Use the C++ parser to tokenize.
				Parser DslParser = new Parser(EnParserOptions.TOKENIZE_ONLY);

				StringCell tokens = DslParser.Execute(command);
				string[] identifiers;
				string composite;
				string token;
				string str = "";


				foreach (StringCell tokenCell in tokens.Enumerator)
				{
					token = tokenCell.ToString().Trim();

					if (String.IsNullOrEmpty(token))
						continue;

					if (token == token.ToUpper() || token.StartsWith("'") ||
						(token.StartsWith("\"") && token.EndsWith("\"")))
					{
						str += (token == "," || token == ";" ? "" : " ") + token;
						continue;
					}

					identifiers = token.Split('.');

					composite = "";

					foreach (string identifier in identifiers)
					{
						if (composite != "")
							composite += ".";

						if (identifier == identifier.ToUpper() || identifier.StartsWith("'") || identifier.StartsWith("\""))
						{
							composite += identifier;
							continue;
						}

						composite += "\"" + identifier + "\"";
					}




					str += " " + composite;
				}

				command = str.Trim();

				// Evs.Trace(GetType(), "Execute", $"Converted command: {command}");
			}




			return ConnectionSupport.ExecuteCore(command, commandType, parameters, commandTimeout);
		}

		public override int ExecuteWithoutResults(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			// Evs.Trace(GetType(), "ExecuteWithoutResults", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");

			return ConnectionSupport.ExecuteWithoutResultsCore(command, commandType, parameters, commandTimeout);
		}
	}




	private class VxiAsyncCommand : DataAsyncCommand
	{
		private VxbConnectionSupport ConnectionSupport => base.Site.GetService(typeof(IVsDataConnectionSupport)) as VxbConnectionSupport;

		public VxiAsyncCommand(IVsDataConnection connection)
			: base(connection)
		{
		}



		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private string DataParametersToString(IVsDataParameter[] parameters)
		{
			if (parameters == null)
				return "null";

			string retval = "";
			foreach (IVsDataParameter parameter in parameters)
			{
				retval += $"{parameter.Name}:{parameter.Type}:{parameter.Value}: {parameter.ToString()},  ";
			}

			return retval;
		}



		protected override IVsDataParameter[] OnDeriveParameters(string command, DataCommandType commandType, int commandTimeout)
		{
			Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", true);

			try
			{
				IVsDataParameter[] parameters = base.OnDeriveParameters(command, commandType, commandTimeout);
				// Evs.Trace(GetType(), "OnDeriveParameters", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");
				return parameters;
			}
			finally
			{
				Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", false);
			}
		}

		protected override string OnPrepare(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", true);

			try
			{
				string retval = base.OnPrepare(command, commandType, parameters, commandTimeout);

				// Evs.Trace(GetType(), "OnPrepare", $"commandType: {commandType}, command: {command}\n{retval}");

				return retval;
			}
			finally
			{
				Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", false);
			}
		}

		protected override IVsDataReader OnDeriveSchema(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", true);

			// Evs.Trace(GetType(), "OnDeriveSchema", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");

			return base.OnDeriveSchema(command, commandType, parameters, commandTimeout);
		}

		protected override void OnDeriveSchemaCompleted(DataAsyncCommandCompletedEventArgs<IVsDataReader> e)
		{
			AdoDotNetConnectionSupport connectionSupport = ConnectionSupport;
			try
			{
				base.OnDeriveSchemaCompleted(e);
			}
			finally
			{
				Reflect.SetFieldValue(connectionSupport, "_currentCommand", null);
				Reflect.SetFieldValue(connectionSupport, "_inAsyncMode", false);
			}
		}

		protected override IVsDataReader OnExecute(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", true);

			// Evs.Trace(GetType(), "OnExecute", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");

			// Basic hack to get past mixed and lower case Firebird identifiers.
			// Composite tokens of the form "identifier1".identifier2."identifier3".identifier4 are handled
			// Composite tokens beginning and ending with double quotes will be skipped.
			// eg. "identifier1".identifier2."identifier3" will not detect identifier2 is unquoted.
			// "identifier1".identifier2."identifier3".identifier4 will convert
			// to "identifier1"."identifier2"."identifier3"."identifier4"

			if (command != null && command != command.ToUpper())
			{
				// Use the C++ parser to tokenize.
				Parser DslParser = new Parser(EnParserOptions.TOKENIZE_ONLY);

				StringCell tokens = DslParser.Execute(command);
				string[] identifiers;
				string composite;
				string token;
				string str = "";


				foreach (StringCell tokenCell in tokens.Enumerator)
				{
					token = tokenCell.ToString().Trim();

					if (String.IsNullOrEmpty(token))
						continue;

					if (token == token.ToUpper() || token.StartsWith("'") ||
						(token.StartsWith("\"") && token.EndsWith("\"")))
					{
						str += (token == "," || token == ";" ? "" : " ") + token;
						continue;
					}

					identifiers = token.Split('.');

					composite = "";

					foreach (string identifier in identifiers)
					{
						if (composite != "")
							composite += ".";

						if (identifier == identifier.ToUpper() || identifier.StartsWith("'") || identifier.StartsWith("\""))
						{
							composite += identifier;
							continue;
						}

						composite += "\"" + identifier + "\"";
					}




					str += " " + composite;
				}

				command = str.Trim();

				// Evs.Trace(GetType(), "Execute", $"Converted command: {command}");
			}

			return base.OnExecute(command, commandType, parameters, commandTimeout);
		}

		protected override void OnExecuteCompleted(DataAsyncCommandCompletedEventArgs<IVsDataReader> e)
		{
			AdoDotNetConnectionSupport connectionSupport = ConnectionSupport;
			try
			{
				base.OnExecuteCompleted(e);
			}
			finally
			{
				Reflect.SetFieldValue(connectionSupport, "_currentCommand", null);
				Reflect.SetFieldValue(connectionSupport, "_inAsyncMode", false);
			}
		}

		protected override int OnExecuteWithoutResults(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
		{
			Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", true);
			try
			{
				// Evs.Trace(GetType(), "OnExecuteWithoutResults", $"commandType: {commandType}, command: {command}\n{DataParametersToString(parameters)}");

				return base.OnExecuteWithoutResults(command, commandType, parameters, commandTimeout);
			}
			finally
			{
				Reflect.SetFieldValue(ConnectionSupport, "_inAsyncMode", false);
			}
		}

		protected override void OnCancel(object userState)
		{
			base.OnCancel(userState);
		}
	}


	#endregion Nested types


}
