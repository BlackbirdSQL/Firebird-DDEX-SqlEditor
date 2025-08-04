
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
internal static class SharedConstants
{

	// ---------------------------------------------------------------------------------
	#region Constants - SharedConstants
	// ---------------------------------------------------------------------------------


	#endregion Constants





	// ---------------------------------------------------------------------------------
	#region Model Engine Miscellaneous keys and defaults - SharedConstants
	// ---------------------------------------------------------------------------------


	internal const string C_DefaultBatchSeparator = ";";

	internal const int C_DefaultInitialMaxCharsPerColumnForGrid = 50;
	internal const int C_DefaultInitialMinNumberOfVisibleRows = 8;
	internal const int C_DefaultMaxCharsPerColumnForGrid = 43679;
	internal const int C_DefaultSetRowCount = 0;
	internal const EnBlobSubType C_DefaultSetBlobDisplay = EnBlobSubType.Text;
	internal const int C_DefaultExecutionTimeout = 0;
	internal const bool C_DefaultSetCount = true;
	internal const bool C_DefaultSetPlanOnly = false;
	internal const bool C_DefaultSetPlan = false;
	internal const bool C_DefaultSetExplain = false;
	internal const bool C_DefaultSetParseOnly = false;
	internal const bool C_DefaultSetConcatenationNull = true;
	internal const bool C_DefaultSetBail = true;
	internal const bool C_DefaultSetPlanText = false;
	internal const bool C_DefaultSetStats = false;
	internal const bool C_DefaultSetWarnings = false;
	internal const bool C_DefaultSetStatisticsIO = false;
	internal const int C_DefaultLockTimeout = 0;
	internal const bool C_DefaultSuppressHeaders = false;
	internal const int C_DefaultGridMaxCharsPerColumnStd = 65535;
	internal const int C_DefaultGridMaxCharsPerColumnXml = 2097152;
	internal const int C_DefaultTextMaxCharsPerColumnStd = 256;
	internal const char C_DefaultTextDelimiter = '\0';


	#endregion Model Engine Miscellaneous keys and defaults





	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property descriptor
	internal const string C_KeyExCreationFlags = "CreationFlags";


	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	internal const EnCreationFlags C_DefaultExCreationFlags = EnCreationFlags.None;


	#endregion DbConnectionString Property Default Values

}
