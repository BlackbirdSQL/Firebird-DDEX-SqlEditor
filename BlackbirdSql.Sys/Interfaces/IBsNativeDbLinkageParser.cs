
using System.Data;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Sys.Interfaces;


// =========================================================================================================
//										IBsNativeDbLinkageParser Interface
//
/// <summary>
/// Interface for the native db trigger linkage parser.
/// </summary>
// =========================================================================================================
internal interface IBsNativeDbLinkageParser
{
	string ConnectionString { get; }
	bool IsLockedLoaded { get; }
	bool Loaded { get; }



	bool EnsureLoaded();
	bool EnsureLoadedRestricted();


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a trigger in the internal linked trigger table and returns it's row else
	/// return null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	DataRow FindTrigger(object name);


	DataTable GetSequenceSchema(string[] restrictions);

	DataTable GetTriggerSchema(string[] restrictions, int systemFlag, int identityFlag);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Locates and returns the internal linked trigger table row for an identity
	/// column else returns null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	DataRow LocateIdentityTrigger(object objTable, object objField);
}
