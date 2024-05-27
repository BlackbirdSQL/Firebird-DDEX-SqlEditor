/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Collections.Generic;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Sys;

using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//										ModelPropertySet Class
//
/// <summary>
/// Static PropertySet for the base class AbstractModelPropertyAgent for replicating the Firebird
/// ConnectionString class and additional members to support SourceInformation, Root ObjectSelector and
/// SqlServer style ConnectionInfo classes. Descendent AbstractModelPropertyAgent classes may or may not
/// have a separate PropertySet class dependent on the number of additional describers.
/// Refer to the <seealso cref="PropertySet"/> class for more information.
/// </summary>
// =========================================================================================================
public abstract class ModelPropertySet : CorePropertySet
{

	#region Constants and Statics


	public static int _EquivalencyDescriberCount = -1;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// PropertyAgent Descriptor/Parameter Properties.
	/// </summary>
	/// <remarks>
	/// Equivalency describers are tagged with *.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly new Describer[] Describers = Csb.Describers.GetDescribers(
		[
			C_KeyRole, C_KeyDialect, C_KeyCharset, C_KeyNoDatabaseTriggers,
			C_KeyPacketSize, C_KeyConnectionTimeout, C_KeyPooling,
			C_KeyConnectionLifeTime, C_KeyMinPoolSize, C_KeyMaxPoolSize, C_KeyFetchSize, 
			C_KeyIsolationLevel, C_KeyReturnRecordsAffected, C_KeyEnlist,
			C_KeyClientLibrary, C_KeyDbCachePages, C_KeyNoGarbageCollect,
			C_KeyCompression, C_KeyCryptKey, C_KeyWireCrypt, C_KeyApplicationName,
			C_KeyCommandTimeout, C_KeyParallelWorkers, C_KeyExClientVersion
		]);

	public static readonly new KeyValuePair<string, string>[] Synonyms = Csb.Describers.GetSynonyms(Describers);



	#endregion Constants and Statics





	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================



	public static new void CreateAndPopulatePropertySetFromStatic(DescriberDictionary describers)
	{
		CorePropertySet.CreateAndPopulatePropertySetFromStatic(describers);

		describers.AddRange(Describers);
		describers.AddSynonyms(Synonyms);
	}



	#endregion Static Methods


}
