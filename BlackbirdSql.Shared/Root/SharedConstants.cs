using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql;


// =========================================================================================================
//											SharedConstants Class
//
/// <summary>
/// Shared db constants class.
/// </summary>
// =========================================================================================================
public static class SharedConstants
{

	// ---------------------------------------------------------------------------------
	#region Constants - SharedConstants
	// ---------------------------------------------------------------------------------


	#endregion Constants





	// ---------------------------------------------------------------------------------
	#region Model Engine Miscellaneous keys and defaults - SharedConstants
	// ---------------------------------------------------------------------------------


	public const string C_DefaultBatchSeparator = ";";

	public const int C_DefaultInitialMaxCharsPerColumnForGrid = 50;
	public const int C_DefaultInitialMinNumberOfVisibleRows = 8;
	public const int C_DefaultMaxCharsPerColumnForGrid = 43679;
	public const int C_DefaultSetRowCount = 0;
	public const EnBlobSubType C_DefaultSetBlobDisplay = EnBlobSubType.Text;
	public const int C_DefaultExecutionTimeout = 0;
	public const bool C_DefaultSetCount = true;
	public const bool C_DefaultSetPlanOnly = false;
	public const bool C_DefaultSetPlan = false;
	public const bool C_DefaultSetExplain = false;
	public const bool C_DefaultSetParseOnly = false;
	public const bool C_DefaultSetConcatenationNull = true;
	public const bool C_DefaultSetBail = true;
	public const bool C_DefaultSetPlanText = false;
	public const bool C_DefaultSetStats = false;
	public const bool C_DefaultSetWarnings = false;
	public const bool C_DefaultSetStatisticsIO = false;
	public const int C_DefaultLockTimeout = 0;
	public const bool C_DefaultSuppressHeaders = false;
	public const int C_DefaultGridMaxCharsPerColumnStd = 65535;
	public const int C_DefaultGridMaxCharsPerColumnXml = 2097152;
	public const int C_DefaultTextMaxCharsPerColumnStd = 256;
	public const char C_DefaultTextDelimiter = '\0';


	#endregion Model Engine Miscellaneous keys and defaults





	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property descriptor
	public const string C_KeyExCreationFlags = "CreationFlags";


	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	public const EnCreationFlags C_DefaultExCreationFlags = EnCreationFlags.None;


	#endregion DbConnectionString Property Default Values

}
