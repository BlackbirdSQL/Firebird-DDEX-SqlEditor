/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

// $OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Collections.Generic;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;

using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Core.Ctl;


// =========================================================================================================
//										CorePropertySet Class
//
/// <summary>
/// The static constants base class for use by connection classes derived from AbstractPropertyAgent.
/// Refer to the <seealso cref="PropertySet"/> class for more information.
/// </summary>
// =========================================================================================================
public abstract class CorePropertySet : PropertySet
{

	#region Constants


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Core Descriptors.
	/// </summary>
	/// <remarks>
	/// Equivalency describers are tagged with *.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] Describers = Csb.Describers.GetDescribers(
	[
		C_KeyExIcon, C_KeyDataSource, C_KeyPort, C_KeyServerType, C_KeyDatabase,
		C_KeyUserID, C_KeyPassword, C_KeyExDataset, C_KeyExDatasetKey,
		C_KeyExConnectionKey, C_KeyExDatasetId, C_KeyExConnectionName,
		C_KeyExConnectionSource, C_KeyExServerEngine, C_KeyExServerVersion,
		C_KeyExPersistPassword, C_KeyExAdministratorLogin,
		C_KeyExServerFullyQualifiedDomainName, C_KeyExOtherParams
	]);

	public static readonly KeyValuePair<string, string>[] Synonyms = Csb.Describers.GetSynonyms(Describers);


	#endregion Constants



	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================


	public static void CreateAndPopulatePropertySetFromStatic(DescriberDictionary describers)
	{
		describers.AddRange(Describers);
		describers.AddSynonyms(Synonyms);
	}


	#endregion Static Methods


}
