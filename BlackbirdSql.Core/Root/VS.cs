
using System;
using Microsoft.VisualStudio;




namespace BlackbirdSql.Core;


// =========================================================================================================
//											VS Class
//
/// <summary>
/// Central location for accessing of Visual Studio, SSDT and ScopeStudio members. 
/// </summary>
// =========================================================================================================

public static class VS
{
	public const uint dwReserved = 0u;

	public const int STG_E_FILEALREADYEXISTS = -2147286960; // 0x80030050
	public const int STG_E_NOTCURRENT = -2147286783; // 0x80030101
	public const int OLE_E_NOTIFYCANCELLED = -2147217842; // 0x80040E4E


	// ---------------------------------------------------------------------------------
	#region DataTools Members - VS
	// ---------------------------------------------------------------------------------


	public static string AdoDotNetTechnologyGuid = "77AB9A9D-78B9-4ba7-91AC-873F5338F1D2";

	public const string IVsDataConnectionManagerInteropGuid = "E7A0D4E0-D0E4-4AFA-A8A1-DD4636073D98";
	public const string IVsDataConnectionInteropGuid = "902A17C6-B166-485F-A49F-9029549442DD";


	public static readonly IntPtr DSREFNODEID_NIL = (IntPtr)0;
	public static readonly IntPtr DSREFNODEID_ROOT = (IntPtr)0;

	public static Guid CLSID_DSRef = new Guid("E09EE6AC-FEF0-41ae-9F77-3C394DA49849");
	public static Guid CLSID_DSRefProperty_Provider = new Guid("B30985D6-6BBB-45f2-9AB8-371664F03270");
	public static Guid CLSID_DSRefProperty_PreciseType = new Guid("39A5A7E7-513F-44a4-B79D-7652CD8962D9");

	public static Guid CLSID_Mode_QueryDesigner = new Guid("B2C40B32-3A37-4ca9-97B9-FA44248B69FF");

	#endregion DataTools Members



	// ---------------------------------------------------------------------------------
	#region SqlEditor Members - VS
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Visual Studio built-in Sql Editor Guid
	/// </summary>
	public const string SqlEditorFactoryGuid = "cc5d8df0-88f4-4bb2-9dbb-b48cee65c30a";

	/// <summary>
	/// Visual Studio built-in Encoded Sql Editor Guid
	/// </summary>
	public const string SqlEditorEncodedFactoryGuid = "F9D1E5B1-8A59-439C-9BB9-D5598B830ECB";

	/// <summary>
	/// Visual Studio built-in Sql Editor Command Set Guid
	/// </summary>
	public const string SqlEditorCommandsGuid = "52692960-56BC-4989-B5D3-94C47A513E8D";


	#endregion SqlEditor Members





	// ---------------------------------------------------------------------------------
	#region SSDT and SqlServer Members - VS
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Suffix of Sql Results Column returning Xml data from SqlServer.
	/// </summary>
	public const string IXMLDocumentGuid = "F52E2B61-18A1-11d1-B105-00805F49916B";


	/// <summary>
	/// SqlStudio Data Tools project guid
	/// </summary>
	public static Guid CLSID_SSDTProjectNode => new("00D1A9C2-B5F0-4AF3-8072-F6C62B433612");


	/// <summary>
	/// Transact-SQL database project guid
	/// </summary>
	public static Guid CLSID_TSqlDataProjectNode => new("C8D11400-126E-41CD-887F-60BD40844F9E");


	/// <summary>
	/// SqlServer debugger launch command UICONTEXT.
	/// </summary>
	public static Guid UICONTEXT_DebuggerLaunching => new("00E544E0-635F-4352-B0FD-A4263DE87BE4");


	/// <summary>
	/// SqlServer publishing and preview commit off command UICONTEXT.
	/// </summary>
	public static Guid UICONTEXT_PublishingPreviewCommitOff = new("0064B9A9-9B52-456D-B51B-A542E33532ED");


	/// <summary>
	/// SqlServer dedicated admin connection Transact-Sql editor launch command UICONTEXT.
	/// </summary>
	public static Guid UICONTEXT_DacTSqlEditorLaunching => new("0E6844B5-C1A8-4243-AFDF-6B10896D6222");


	#endregion SSDT and SqlServer Members





	// ---------------------------------------------------------------------------------
	#region Miscellaneous Visual Studio Members - VS
	// ---------------------------------------------------------------------------------


	public static Guid CLSID_CTextViewCommandGroup => new(VSConstants.CMDSETID.StandardCommandSet2K_guid.ToString());


	// Visual Studio Text Manager Clsid
	public static Guid CLSID_TextManager => new("F5E7E71D-1401-11d1-883B-0000F87579D2");


	/// <summary>
	/// Visual Studio Xml UI commands CLSID
	/// </summary>
	public static readonly Guid CLSID_XmlUiCmds = new("FB87333B-16C8-400E-BC8F-F6B890410582");


	#endregion Miscellaneous Visual Studio Members





	// ---------------------------------------------------------------------------------
	#region Language Service Members - VS
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// The built-in default Visual Studio Language service clsid
	/// </summary>
	public static Guid CLSID_LanguageServiceDefault = new("8239bec4-ee87-11d0-8c98-00c04fc2ab22");

	/// <summary>
	/// Transact-SQL Message error marker clsid
	/// </summary>
	public static Guid CLSID_TSqlEditorMessageErrorMarker = new("E08D10C8-D5C9-493C-AEE3-87C5419F9C6F");



	/// <summary>
	/// Transact-SQL Language service guid
	/// </summary>
	public const string TSql90LanguageServiceGuid = "43AF1158-FED5-432e-8E8F-23B6FD592857";

	/// <summary>
	/// Unified Sql Language service guid
	/// </summary>
	public const string USqlLanguageServiceGuid = "ce0b201a-1f8b-42a5-ad08-72026287ea92";

	/// <summary>
	/// SqlServer Data Tools Language service guid
	/// </summary>
	public const string SSDTLanguageServiceGuid = "ed1a9c1c-d95c-4dc1-8db8-e5a28707a864";

	/// <summary>
	/// Xml Language service guid
	/// </summary>
	public const string XmlLanguageServiceGuid = "f6819a78-a205-47b5-be1c-675b3c7f0b8e";

	/// <summary>
	/// Transact-SQL Language Evaluator Guid
	/// </summary>
	public const string TSqlExpressionEvaluatorGuid = "3A12D0B9-C26C-11D0-B442-00A0244A1DD2";


	#endregion Language Service Members





	// ---------------------------------------------------------------------------------
	#region Font and Colors Members - VS
	// ---------------------------------------------------------------------------------

	/// <summary>
	/// All text ToolWindows for Font and Colors Guid
	/// </summary>
	public static readonly string FontAndColorsTextToolWindows = "{C34C709B-C855-459e-B38C-3021F162D3B1}";

	/// <summary>
	/// Visual Studio Font and Colors Category for Text Editor CLSID
	/// </summary>
	public static readonly Guid CLSID_FontAndColorsTextEditorCategory = new Guid("A27B4E24-A735-4d1d-B8E7-9716E1E3D8E0");

	/// <summary>
	/// Visual Studio Font and Colors Category for Text SqlResults CLSID
	/// </summary>
	public static readonly Guid CLSID_FontAndColorsSqlResultsTextCategory = new("587D0421-E473-4032-B214-9359F3B7BC80");

	/// <summary>
	/// Visual Studio Font and Colors Category for Grid SqlResults CLSID
	/// </summary>
	public static readonly Guid CLSID_FontAndColorsSqlResultsGridCategory = new("6202FF3E-488E-4EAD-92CB-BE089659F8D7");

	/// <summary>
	/// Visual Studio Font and Colors Category for Execution Plan SqlResults CLSID
	/// </summary>
	public static readonly Guid CLSID_FontAndColorsSqlResultsExecutionPlanCategory = new("93D586A1-64DC-4802-9B83-7007FAD2C861");



	#endregion Font and Colors Members





	// =========================================================================================================
	#region Static Methods - VS
	// =========================================================================================================




	#endregion Static Methods


}
