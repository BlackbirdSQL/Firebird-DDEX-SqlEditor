// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.DefaultSqlEditorStrategy

using System;
using System.Data.Common;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//									ConnectionStrategyFactory Class
//
/// <summary>
/// ConnectionStrategy factory.
/// </summary>
// =========================================================================================================
public sealed class ConnectionStrategyFactory : IBsConnectionStrategyFactory, IDisposable
{

	// -----------------------------------------------------------
	#region Constructors / Destructors - ConnectionStrategyFactory
	// -----------------------------------------------------------

	/// <summary>
	/// Default .ctor.
	/// </summary>
	public ConnectionStrategyFactory(DbConnectionStringBuilder csb, EnEditorCreationFlags creationFlags)
	{
		if (csb is Csb csa)
		{
			bool createDataConnection = (creationFlags & EnEditorCreationFlags.CreateConnection) > 0;

			_DefaultConnectionInfo = new ConnectionInfoPropertyAgent();
			_DefaultConnectionInfo.Parse(csa.ConnectionString);

			if (createDataConnection)
				_DefaultConnectionInfo.CreateDataConnection();
		}

		// _IsOnline = isOnline;
	}


	/*
	public ConnectionStrategyFactory()
	{
	}



	public ConnectionStrategyFactory(ConnectionInfoPropertyAgent defaultConnectionInfo)
		: this(defaultConnectionInfo, isOnline: false)
	{
	}



	public ConnectionStrategyFactory(ConnectionInfoPropertyAgent defaultConnectionInfo, bool isOnline)
	{
		if (defaultConnectionInfo == null)
		{
			ArgumentNullException ex = new("defaultUiConnectionInfo");
			Diag.Dug(ex);
			throw ex;
		}

		_DefaultConnectionInfo = defaultConnectionInfo;
		_isOnline = isOnline;
	}
	*/



	public void Dispose()
	{
		_DefaultConnectionInfo?.Dispose();
		GC.SuppressFinalize(this);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - ConnectionStrategyFactory
	// =========================================================================================================


	private readonly IBsConnectionInfo _DefaultConnectionInfo;
	// private readonly bool _IsOnline;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ConnectionStrategyFactory
	// =========================================================================================================


	public string DatabaseName
	{
		get
		{
			NotImplementedException ex = new("Not implemented in " + GetType().FullName);
			Diag.Dug(ex);
			throw ex;
		}
	}

	public IBsExtendedCommandHandler ExtendedCommandHandler => null;

	public bool IsOnline => false; // _IsOnline;

	public object MetadataProviderProvider => null;

	public EnEditorMode Mode => EnEditorMode.Standard;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ConnectionStrategyFactory
	// =========================================================================================================


	public ConnectionStrategy CreateConnectionStrategy()
	{
		ConnectionStrategy strategy = new ConnectionStrategy();

		if (_DefaultConnectionInfo != null)
			strategy.ConnInfo = _DefaultConnectionInfo;

		return strategy;
	}



	public IBsErrorTaskFactory GetErrorTaskFactory()
	{
		return null;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ConnectionStrategyFactory
	// =========================================================================================================



	#endregion Event Handling

}
