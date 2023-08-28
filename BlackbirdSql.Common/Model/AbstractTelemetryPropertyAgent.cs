#region Assembly Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.RegSvrEnum.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;

using static BlackbirdSql.Common.Model.TelemetryConstants;



namespace BlackbirdSql.Common.Model;


// =========================================================================================================
//
//										AbstractConnectionDlgPropertyAgent Class
//
// =========================================================================================================
public abstract class AbstractTelemetryPropertyAgent : AbstractDispatcherConnection
{


	// ---------------------------------------------------------------------------------
	#region Variables - AbstractConnectionDlgPropertyAgent
	// ---------------------------------------------------------------------------------


	protected static new DescriberDictionary _Describers = null;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractConnectionDlgPropertyAgent
	// =========================================================================================================


	public override DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet(null);

			return _Describers;
		}
	}





	#endregion Property Accessors





	// =========================================================================================================
	#region Descriptors Property Accessors - AbstractConnectionDlgPropertyAgent
	// =========================================================================================================

	public int CountConnections
	{
		get { return (int)GetProperty(C_KeyCountConnections); }
		set { SetProperty(C_KeyCountConnections, value); }
	}
	public string EngineProduct
	{
		get { return (string)GetProperty(C_KeyEngineProduct); }
		set { SetProperty(C_KeyEngineProduct, value); }
	}
	public EnEngineType EngineType
	{
		get { return (EnEngineType)GetProperty(C_KeyEngineType); }
		set { SetProperty(C_KeyEngineType, value); }
	}
	public string TabOpen
	{
		get { return (string)GetProperty(C_KeyTabOpen); }
		set { SetProperty(C_KeyTabOpen, value); }
	}
	public string HistoryTab
	{
		get { return (string)GetProperty(C_KeyHistoryTab); }
		set { SetProperty(C_KeyHistoryTab, value); }
	}
	public string BrowseTab
	{
		get { return (string)GetProperty(C_KeyBrowseTab); }
		set { SetProperty(C_KeyBrowseTab, value); }
	}
	public int CountConnectionErrors
	{
		get { return (int)GetProperty(C_KeyCountConnectionErrors); }
		set { SetProperty(C_KeyCountConnectionErrors, value); }
	}
	public bool ConnectionSuccess
	{
		get { return (bool)GetProperty(C_KeyConnectionSuccess); }
		set { SetProperty(C_KeyConnectionSuccess, value); }
	}
	public int CountRecentConnections
	{
		get { return (int)GetProperty(C_KeyCountRecentConnections); }
		set { SetProperty(C_KeyCountRecentConnections, value); }
	}
	public int CountFavoriteConnections
	{
		get { return (int)GetProperty(C_KeyCountFavoriteConnections); }
		set { SetProperty(C_KeyCountFavoriteConnections, value); }
	}
	public bool IsLocalDb
	{
		get { return (bool)GetProperty(C_KeyIsLocalDb); }
		set { SetProperty(C_KeyIsLocalDb, value); }
	}
	public int CountEngineUnknown
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.Unknown.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.Unknown.ToString(), value); }
	}
	public int CountEngineLocalClassicServer
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.LocalClassicServer.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.LocalClassicServer.ToString(), value); }
	}
	public int CountEngineLocalSuperClassic
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.LocalSuperClassic.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.LocalSuperClassic.ToString(), value); }
	}
	public int CountEngineLocalSuperServer
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.LocalSuperServer.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.LocalSuperServer.ToString(), value); }
	}
	public int CountEngineClassicServer
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.ClassicServer.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.ClassicServer.ToString(), value); }
	}
	public int CountEngineSuperClassic
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.SuperClassic.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.SuperClassic.ToString(), value); }
	}
	public int CountEngineSuperServer
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.SuperServer.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.SuperServer.ToString(), value); }
	}
	public int CountEngineEmbeddedDatabase
	{
		get { return (int)GetProperty(C_CountEnginePrefix + EnEngineType.EmbeddedDatabase.ToString()); }
		set { SetProperty(C_CountEnginePrefix + EnEngineType.EmbeddedDatabase.ToString(), value); }
	}


	#endregion 	Descriptors Property Accessors





	// =========================================================================================================
	#region Property Getters/Setters - AbstractConnectionDlgPropertyAgent
	// =========================================================================================================





	#endregion Property Getters/Setters




	// =========================================================================================================
	#region Constructors / Destructors - AbstractConnectionDlgPropertyAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Universal .ctor
	/// </summary>
	/// <remarks>
	/// It is the final owning (child) class's responsibility to perform the once off
	/// creation of the static private property set for it's class by calling the static
	/// CreatePropertySet(). The final child class is the class that initiated
	/// instanciation and is identifiable in that it has no _Owner private instance
	/// variable.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public AbstractTelemetryPropertyAgent() : base()
	{
	}



	static AbstractTelemetryPropertyAgent()
	{
	}


	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		if (_Describers == null)
		{
			_Describers = new();

			AbstractModelPropertyAgent.CreateAndPopulatePropertySet(_Describers);

			_Describers.AddRange(TelemetryPropertySet.Describers);

			EnEngineType[] engineTypeValues = Enum.GetValues(typeof(EnEngineType)) as EnEngineType[];

			foreach (EnEngineType engineType in engineTypeValues)
			{
				_Describers.Add(C_CountEnginePrefix + engineType.ToString(), typeof(int), 0);
			}

		}

		// If null then this was a call from our own .ctor so no need to pass anything back
		describers?.AddRange(_Describers);

	}

	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractConnectionDlgPropertyAgent
	// =========================================================================================================


	public override IBPropertyAgent Copy()
	{
		NotImplementedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}


	#endregion Methods


}
